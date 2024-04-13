using System.Diagnostics;
using XFEExtension.NetCore.DelegateExtension;

namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 内存监听器
/// </summary>
public class MemoryListener : IDisposable
{
    private bool disposedValue;
    private readonly Dictionary<nint, bool> listeningList = [];
    private readonly Dictionary<string, bool> listeningFuncList = [];
    /// <summary>
    /// 当监听值发生改变时触发
    /// </summary>
    public event XFEEventHandler<nint?, MemoryValue>? ValueChanged;
    /// <summary>
    /// 进程句柄
    /// </summary>
    public nint ProcessHandler { get; set; }
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
            listeningList.Add(memoryAddress, true);
        ReStart:
            try
            {
                while (listeningList[memoryAddress] && IsListening)
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
                listeningList.Remove(memoryAddress);
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
            listeningList.Add(memoryAddress, true);
        ReStart:
            try
            {
                while (listeningList[memoryAddress] && IsListening)
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
                listeningList.Remove(memoryAddress);
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
            listeningFuncList.Add(customName, true);
        ReStart:
            try
            {
                while (listeningFuncList[customName] && IsListening)
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
                            while (memoryAddress == 0 && listeningFuncList[customName] && IsListening)
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
                listeningFuncList.Remove(customName);
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
    public void StopListener() => IsListening = false;
    /// <summary>
    /// 停止指定进程的监听
    /// </summary>
    /// <param name="memoryAddress">内存地址</param>
    public void StopListener(nint memoryAddress) => listeningList[memoryAddress] = false;
    /// <summary>
    /// 停止指定进程的监听
    /// </summary>
    /// <param name="customName">自定义名称</param>
    public void StopListener(string customName) => listeningFuncList[customName] = false;
    /// <summary>
    /// 内存监听器
    /// </summary>
    /// <param name="processName">进程名称</param>
    public MemoryListener(string processName) => ProcessHandler = MemoryEditor.GetProcessHandle(processName, ProcessAccessFlags.All);
    /// <summary>
    /// 内存监听器
    /// </summary>
    /// <param name="processHandler">进程句柄</param>
    public MemoryListener(nint processHandler) => ProcessHandler = processHandler;
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
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposedValue = true;
        }
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    // ~MemoryListener()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
