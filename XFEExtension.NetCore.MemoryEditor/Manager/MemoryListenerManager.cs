using System.Collections;
using System.Diagnostics;
using XFEExtension.NetCore.DelegateExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存监听器
/// </summary>
public class MemoryListenerManager : MemoryListenerManagerBase, IDisposable, IEnumerable
{
    private bool disposedValue;
    private readonly Dictionary<string, MemoryListener> listenerDictionary = [];
    /// <summary>
    /// 当监听值发生改变时触发
    /// </summary>
    public event XFEEventHandler<MemoryListener, MemoryValue>? ValueChanged;
    /// <summary>
    /// 当前进程结束时触发
    /// </summary>
    public event EventHandler? CurrentProcessExit;
    /// <summary>
    /// 目标进程启动时触发
    /// </summary>
    public event EventHandler? CurrentProcessEntered;
    /// <summary>
    /// 进程句柄
    /// </summary>
    public nint ProcessHandler { get; set; }
    private string processName = string.Empty;
    /// <summary>
    /// 目标进程名称
    /// </summary>
    public string ProcessName
    {
        get { return processName; }
        set { processName = value; }
    }
    private Process? currentProcess;
    /// <summary>
    /// 当前监听内存的目标进程
    /// </summary>
    public Process? CurrentProcess
    {
        get => currentProcess;
        set
        {
            if (value is not null)
            {
                currentProcess?.Dispose();
                currentProcess = value;
                ProcessName = value.ProcessName;
                ProcessHandler = MemoryEditor.GetProcessHandle(value.Id, ProcessAccessFlags);
                currentProcess.Exited += CurrentProcess_Exited;
                if (IsListening)
                {
                    foreach (var listener in listenerDictionary.Values)
                    {
                        if (listener is UpdatableMemoryListener updatableMemoryListener)
                            updatableMemoryListener.UpdateAddress(ProcessHandler);
                        if (!listener.IsListening)
                        {
                            listener.ProcessHandler = ProcessHandler;
                            _ = listener.StartListen(ProcessHandler, listener.Frequency);
                        }
                    }
                }
                CurrentProcessEntered?.Invoke(value, EventArgs.Empty);
            }
        }
    }
    private async void CurrentProcess_Exited(object? sender, EventArgs e)
    {
        CurrentProcessExit?.Invoke(sender, e);
        if (AutoReacquireProcess)
            _ = WaitProcessEnter();
        await StopListeners();
    }
    /// <summary>
    /// 是否启用监听
    /// </summary>
    public bool IsListening { get; set; }
    /// <summary>
    /// 当目标进程退出后，是否自动重新获取进程
    /// </summary>
    public bool AutoReacquireProcess { get; set; }
    /// <summary>
    /// 重新获取进程的检测频率（单位毫秒）
    /// </summary>
    public int ReacquireProcessFrequency { get; set; } = 500;
    /// <summary>
    /// 进程句柄权限
    /// </summary>
    public ProcessAccessFlags ProcessAccessFlags { get; set; } = ProcessAccessFlags.All;
    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="customName">监听器自定义标识名</param>
    /// <returns></returns>
    public MemoryListener this[string customName]
    {
        get => listenerDictionary[customName];
        set => listenerDictionary[customName] = value;
    }
    internal override StaticMemoryListener AddStaticListener(string customName, nint memoryAddress, TimeSpan? frequency, Type type, bool startListen)
    {
        if (!IsListening)
            IsListening = true;
        var staticMemoryListener = MemoryListener.CreateStaticListener(customName, memoryAddress, type);
        staticMemoryListener.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);
        if (startListen)
            _ = staticMemoryListener.StartListen(ProcessHandler, frequency);
        return staticMemoryListener;
    }
    internal override DynamicMemoryListener AddDynamicListener(string customName, Func<nint?> memoryAddressGetFunc, TimeSpan? frequency, Type type, bool startListen)
    {
        if (!IsListening)
            IsListening = true;
        var dynamicMemoryListener = MemoryListener.CreateDynamicListener(customName, memoryAddressGetFunc, type);
        dynamicMemoryListener.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);
        if (startListen)
            _ = dynamicMemoryListener.StartListen(ProcessHandler, frequency);
        return dynamicMemoryListener;
    }
    internal override UpdatableMemoryListener AddUpdatableListener(string customName, Func<nint?> memoryAddressUpdateFunc, TimeSpan? frequency, Type type, bool startListen)
    {
        if (!IsListening)
            IsListening = true;
        var updatableMemoryListener = MemoryListener.CreateUpdatableListener(customName, memoryAddressUpdateFunc, type);
        updatableMemoryListener.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);
        if (startListen)
            _ = updatableMemoryListener.StartListen(ProcessHandler, frequency);
        return updatableMemoryListener;
    }
    /// <summary>
    /// 等待目标进程出现
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="frequency">检测频率</param>
    /// <returns></returns>
    public async Task WaitProcessEnter(string? processName = null, int frequency = 500)
    {
        if (processName is not null)
            ProcessName = processName;
        ReacquireProcessFrequency = frequency;
        await Task.Run(async () =>
        {
            while (currentProcess is null || currentProcess.HasExited)
            {
                var result = Process.GetProcessesByName(ProcessName);
                if (result is not null && result.Length >= 1)
                {
                    CurrentProcess = result[0];
                    return;
                }
                await Task.Delay(ReacquireProcessFrequency);
            }
        });
    }
    /// <inheritdoc/>
    public override async Task StopListeners()
    {
        IsListening = false;
        var taskList = new List<Task>();
        foreach (var listener in listenerDictionary.Values)
        {
            taskList.Add(listener.StopListen());
        }
        await Task.WhenAll(taskList);
    }
    /// <inheritdoc/>
    public override async Task StopListener(string customName) => await listenerDictionary[customName].StopListen();
    /// <inheritdoc/>
    public override void RemoveListener(string customName)
    {
        listenerDictionary.Remove(customName);
        if (listenerDictionary.Count == 0)
            IsListening = false;
    }
    /// <inheritdoc/>
    public override void ClearListeners()
    {
        IsListening = false;
        listenerDictionary.Clear();
    }
    /// <summary>
    /// 内存监听器
    /// </summary>
    public MemoryListenerManager() { }
    /// <summary>
    /// 内存监听器
    /// </summary>
    /// <param name="targetProcess">目标进程</param>
    public MemoryListenerManager(Process targetProcess) => CurrentProcess = targetProcess;
    /// <summary>
    /// 内存监听器
    /// </summary>
    /// <param name="targetProcessId">目标进程ID</param>
    public MemoryListenerManager(int targetProcessId) => CurrentProcess = Process.GetProcessById(targetProcessId);
    /// <summary>
    /// 内存监听器
    /// </summary>
    /// <param name="processName">进程名称</param>
    public MemoryListenerManager(string processName) => CurrentProcess = Process.GetProcessesByName(processName).First();
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                IsListening = false;
                ValueChanged = null;
                CurrentProcessExit = null;
            }

            disposedValue = true;
        }
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public IEnumerator GetEnumerator() => listenerDictionary.GetEnumerator();
}