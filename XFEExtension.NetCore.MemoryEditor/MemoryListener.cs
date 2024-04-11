using XFEExtension.NetCore.DelegateExtension;

namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 内存监听器
/// </summary>
public class MemoryListener : IDisposable
{
    private bool disposedValue;
    private readonly Dictionary<nint, bool> listeningList = [];
    /// <summary>
    /// 当监听值发生改变时触发
    /// </summary>
    public event XFEEventHandler<nint, MemoryValue>? ValueChanged;
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
    /// <returns></returns>
    public async Task StartListen<T>(nint memoryAddress) where T : struct
    {
        IsListening = true;
        listeningList.Add(memoryAddress, true);
        await Task.Run(() =>
        {
            var lastValue = MemoryEditor.ReadMemory<T>(ProcessHandler, memoryAddress);
            while (listeningList[memoryAddress] && IsListening)
            {
                var currentValue = MemoryEditor.ReadMemory<T>(ProcessHandler, memoryAddress);
                if (!currentValue.Equals(lastValue))
                {
                    ValueChanged?.Invoke(memoryAddress, new(lastValue, currentValue));
                    lastValue = currentValue;
                }
            }
            listeningList.Remove(memoryAddress);
        });
    }
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="memoryAddress">待监听的内存地址</param>
    /// <param name="delay">检测频率，以毫秒为单位</param>
    /// <returns></returns>
    public async Task StartListen<T>(nint memoryAddress, int delay = 10) where T : struct
    {
        IsListening = true;
        listeningList.Add(memoryAddress, true);
        await Task.Run(() =>
        {
            var lastValue = MemoryEditor.ReadMemory<T>(ProcessHandler, memoryAddress);
            while (listeningList[memoryAddress] && IsListening)
            {
                var currentValue = MemoryEditor.ReadMemory<T>(ProcessHandler, memoryAddress);
                if (!currentValue.Equals(lastValue))
                {
                    ValueChanged?.Invoke(memoryAddress, new(lastValue, currentValue));
                    currentValue = lastValue;
                }
                Thread.Sleep(delay);
            }
            listeningList.Remove(memoryAddress);
        });
    }
    /// <summary>
    /// 停止所有监听
    /// </summary>
    public void StopListener() => IsListening = false;
    /// <summary>
    /// 停止指定进程的监听
    /// </summary>
    /// <param name="memoryAddress"></param>
    public void StopListener(nint memoryAddress) => listeningList[memoryAddress] = false;
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
