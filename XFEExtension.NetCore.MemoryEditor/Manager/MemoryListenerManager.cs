using System.Collections;
using System.Diagnostics;
using XFEExtension.NetCore.DelegateExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存监听器
/// </summary>
public class MemoryListenerManager : IDisposable, IEnumerable
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
    /// 进程句柄
    /// </summary>
    public nint ProcessHandler { get; set; }
    private Process? currentProcess;
    /// <summary>
    /// 当前监听内存的目标进程
    /// </summary>
    public Process? CurrentProcess
    {
        get { return currentProcess; }
        set
        {
            if (value is not null)
            {
                currentProcess?.Dispose();
                currentProcess = value;
                ProcessHandler = MemoryEditor.GetProcessHandle(value.Id, ProcessAccessFlags.All);
                currentProcess.Exited += CurrentProcess_Exited;
                if (IsListening)
                {
                    foreach (var listener in listenerDictionary.Values)
                    {
                        if (listener is UpdatableMemoryListener updatableMemoryListener)
                            updatableMemoryListener.UpdateAddress();
                        if (!listener.IsListening)
                            _ = listener.StartListen(ProcessHandler, listener.Frequency);
                    }
                }
            }
        }
    }
    private async void CurrentProcess_Exited(object? sender, EventArgs e)
    {
        CurrentProcessExit?.Invoke(sender, e);
        await StopListeners();
    }
    /// <summary>
    /// 是否启用监听
    /// </summary>
    public bool IsListening { get; set; }
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
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="customName">自定义监听器标识名</param>
    /// <param name="memoryAddress">待监听的内存地址</param>
    /// <param name="frequency">检测频率，以毫秒为单位</param>
    /// <returns></returns>
    public MemoryListener AddListener<T>(string customName, nint memoryAddress, int frequency = 1) where T : struct => AddListener<T>(customName
        , memoryAddress, TimeSpan.FromMilliseconds(frequency));
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="customName">自定义监听器标识名</param>
    /// <param name="memoryAddress">待监听的内存地址</param>
    /// <param name="frequency">检测频率</param>
    /// <returns></returns>
    public MemoryListener AddListener<T>(string customName, nint memoryAddress, TimeSpan frequency) where T : struct
    {

    }
    internal StaticMemoryListener AddStaticListener(string customName, nint memoryAddress, TimeSpan frequency, Type type)
    {
        if (!IsListening)
            IsListening = true;
        var staticMemoryListener = MemoryListener.CreateStaticListener(customName, memoryAddress, type);
        _ = staticMemoryListener.StartListen(ProcessHandler, frequency);
        return staticMemoryListener;
    }
    internal DynamicMemoryListener AddDynamicListener(string customName, Func<nint?> memoryAddressGetFunc, TimeSpan frequency, Type type)
    {
        if (!IsListening)
            IsListening = true;
        var dynamicMemoryListener = MemoryListener.CreateDynamicListener(customName, memoryAddressGetFunc, type);
        _ = dynamicMemoryListener.StartListen(ProcessHandler, frequency);
        return dynamicMemoryListener;
    }
    /// <summary>
    /// 停止所有监听
    /// </summary>
    public async Task StopListeners()
    {
        IsListening = false;
        var taskList = new List<Task>();
        foreach (var listener in listenerDictionary.Values)
        {
            taskList.Add(listener.StopListen());
        }
        await Task.WhenAll(taskList);
    }
    /// <summary>
    /// 停止指定的监听器
    /// </summary>
    /// <param name="customName">自定义名称</param>
    public async Task StopListener(string customName) => await listenerDictionary[customName].StopListen();
    /// <summary>
    /// 移除指定监听器
    /// </summary>
    /// <param name="customName"></param>
    public void RemoveListener(string customName)
    {
        listenerDictionary.Remove(customName);
        if (listenerDictionary.Count == 0)
            IsListening = false;
    }
    /// <summary>
    /// 清除所有监听器
    /// </summary>
    public void Clear()
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
