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
    private readonly List<nint> addressListToListen = [];
    private readonly Dictionary<string, Func<nint?>> addressFuncDictionaryToListen = [];
    private readonly Dictionary<nint, bool> listeningAddressDictionary = [];
    private readonly Dictionary<string, bool> listeningAddressFuncDictionary = [];
    /// <summary>
    /// 当监听值发生改变时触发
    /// </summary>
    public event XFEEventHandler<MemoryValue>? ValueChanged;
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
                isCurrentProcessRunning = true;
                currentProcess.Exited += (sender, e) => CurrentProcessExit?.Invoke(sender, e);
                foreach (var address in addressListToListen)
                {

                }
            }
        }
    }
    /// <summary>
    /// 是否启用监听
    /// </summary>
    public bool IsListening { get; set; }
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="memoryAddress">待监听的内存地址</param>
    /// <param name="customName">自定义监听器标识名</param>
    /// <returns></returns>
    public async Task StartListen<T>(nint memoryAddress, string? customName = null) where T : struct
    {
        IsListening = true;
        await Task.Run(() =>
        {
            bool lastSuccess = false;
            var lastValue = default(T);
            try { lastSuccess = MemoryEditor.ReadMemory(ProcessHandler, memoryAddress, out lastValue); } catch { }
            addressListToListen.Add(memoryAddress);
            listeningAddressDictionary.Add(memoryAddress, true);
        ReStart:
            try
            {
                while (listeningAddressDictionary[memoryAddress] && IsListening && isCurrentProcessRunning)
                {
                    try
                    {
                        var currentSuccess = MemoryEditor.ReadMemory<T>(ProcessHandler, memoryAddress, out var currentValue);
                        if (!currentValue.Equals(lastValue) || lastSuccess != currentSuccess)
                        {
                            ValueChanged?.Invoke(memoryAddress, new(lastSuccess, currentSuccess, lastValue, currentValue, customName));
                            lastValue = currentValue;
                            lastSuccess = currentSuccess;
                        }
                    }
                    catch { }
                }
                listeningAddressDictionary.Remove(memoryAddress);
            }
            catch
            {
                goto ReStart;
            }
        });
    }
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="memoryAddress">待监听的内存地址</param>
    /// <param name="delay">检测频率，以毫秒为单位</param>
    /// <param name="customName">自定义监听器标识名</param>
    /// <returns></returns>
    public async Task StartListen<T>(nint memoryAddress, int delay, string? customName = null) where T : struct => await StartListen<T>(memoryAddress, TimeSpan.FromMilliseconds(delay), customName);
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="memoryAddress">待监听的内存地址</param>
    /// <param name="delay">检测频率</param>
    /// <param name="customName">自定义监听器标识名</param>
    /// <returns></returns>
    public async Task StartListen<T>(nint memoryAddress, TimeSpan delay, string? customName = null) where T : struct
    {
        IsListening = true;
        await Task.Run(async () =>
        {
            bool lastSuccess = false;
            var lastValue = default(T);
            try { lastSuccess = MemoryEditor.ReadMemory(ProcessHandler, memoryAddress, out lastValue); } catch { }
            addressListToListen.Add(memoryAddress);
            listeningAddressDictionary.Add(memoryAddress, true);
        ReStart:
            try
            {
                while (listeningAddressDictionary[memoryAddress] && IsListening && isCurrentProcessRunning)
                {
                    try
                    {
                        var currentSuccess = MemoryEditor.ReadMemory<T>(ProcessHandler, memoryAddress, out var currentValue);
                        if (!currentValue.Equals(lastValue) || lastSuccess != currentSuccess)
                        {
                            ValueChanged?.Invoke(memoryAddress, new(lastSuccess, currentSuccess, lastValue, currentValue, customName));
                            lastValue = currentValue;
                            lastSuccess = currentSuccess;
                        }
                        await Task.Delay(delay);
                    }
                    catch { }
                }
                listeningAddressDictionary.Remove(memoryAddress);
            }
            catch
            {
                goto ReStart;
            }
        });
    }
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="getMemoryAddressFunc">动态获取内存地址的方法</param>
    /// <param name="delay">检测频率</param>
    /// <param name="customName">自定义监听器标识名</param>
    /// <returns></returns>
    public async Task StartListen<T>(Func<nint?> getMemoryAddressFunc, int delay, string customName) where T : struct => await StartListen<T>(getMemoryAddressFunc, TimeSpan.FromMilliseconds(delay), customName);
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="getMemoryAddressFunc">动态获取内存地址的方法</param>
    /// <param name="delay">检测频率</param>
    /// <param name="customName">自定义监听器标识名</param>
    /// <returns></returns>
    public async Task StartListen<T>(Func<nint?> getMemoryAddressFunc, TimeSpan delay, string customName) where T : struct
    {
        IsListening = true;
        await Task.Run(async () =>
        {
            var memoryAddress = getMemoryAddressFunc.Invoke();
            bool lastSuccess = false;
            var lastValue = default(T);
            if (memoryAddress is not null)
                try { lastSuccess = MemoryEditor.ReadMemory(ProcessHandler, memoryAddress.Value, out lastValue); } catch { }
            addressFuncDictionaryToListen.Add(customName, getMemoryAddressFunc);
            listeningAddressFuncDictionary.Add(customName, true);
        ReStart:
            try
            {
                while (listeningAddressFuncDictionary[customName] && IsListening && isCurrentProcessRunning)
                {
                    try
                    {
                        memoryAddress = getMemoryAddressFunc.Invoke();
                        if (memoryAddress == 0)
                        {
                            if (lastSuccess)
                                ValueChanged?.Invoke(memoryAddress, new(lastSuccess, false, lastValue, null, customName));
                            lastSuccess = false;
                            lastValue = default;
                            while (memoryAddress == 0 && listeningAddressFuncDictionary[customName] && IsListening && isCurrentProcessRunning)
                            {
                                memoryAddress = getMemoryAddressFunc.Invoke();
                                await Task.Delay(delay);
                            }
                        }
                        var currentSuccess = MemoryEditor.ReadMemory<T>(ProcessHandler, memoryAddress!.Value, out var currentValue);
                        if (!currentValue.Equals(lastValue) || lastSuccess != currentSuccess)
                        {
                            ValueChanged?.Invoke(memoryAddress, new(lastSuccess, currentSuccess, lastValue, currentValue, customName));
                            lastValue = currentValue;
                            lastSuccess = currentSuccess;
                        }
                        await Task.Delay(delay);
                    }
                    catch { }
                }
                listeningAddressFuncDictionary.Remove(customName);
            }
            catch
            {
                goto ReStart;
            }
        });
    }
    /// <summary>
    /// 停止所有监听
    /// </summary>
    public void StopListener()
    {
        IsListening = false;
        addressListToListen.Clear();
        addressFuncDictionaryToListen.Clear();
    }
    /// <summary>
    /// 停止指定进程的监听
    /// </summary>
    /// <param name="memoryAddress">内存地址</param>
    public void StopListener(nint memoryAddress)
    {
        listeningAddressDictionary[memoryAddress] = false;
        addressListToListen.Remove(memoryAddress);
    }
    /// <summary>
    /// 停止指定进程的监听
    /// </summary>
    /// <param name="customName">自定义名称</param>
    public void StopListener(string customName)
    {
        listeningAddressFuncDictionary[customName] = false;
        addressFuncDictionaryToListen.Remove(customName);
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

    public IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
