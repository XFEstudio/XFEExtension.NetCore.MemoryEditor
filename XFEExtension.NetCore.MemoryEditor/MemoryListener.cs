using XFEExtension.NetCore.DelegateExtension;

namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 内存监听器
/// </summary>
/// <param name="customName">监听器自定义标识名</param>
/// <param name="memoryAddressType">内存地址类型</param>
public abstract class MemoryListener(string customName, Type memoryAddressType) : IDisposable
{
    private protected bool disposedValue;
    private protected bool isListening;
    private protected XFEEventHandler<MemoryListener, MemoryValue>? valueChanged;
    /// <summary>
    /// 当监听值发生改变时触发
    /// </summary>
    public event XFEEventHandler<MemoryListener, MemoryValue>? ValueChanged { add => valueChanged += value; remove => valueChanged -= value; }
    private protected XFEEventHandler<MemoryListener, bool>? listeningStateChanged;
    /// <summary>
    /// 当监听状态改变时触发
    /// </summary>
    public event XFEEventHandler<MemoryListener, bool>? ListeningStateChanged
    {
        add => listeningStateChanged += value;
        remove => listeningStateChanged -= value;
    }
    private protected string name = customName;
    /// <summary>
    /// 监听器自定义标识名
    /// </summary>
    public string Name { get => name; }
    private protected TimeSpan frequency = TimeSpan.FromMilliseconds(1);
    /// <summary>
    /// 监听检测频率
    /// </summary>
    public TimeSpan Frequency { get => frequency; }
    private protected readonly Type memoryAddressType = memoryAddressType;
    /// <summary>
    /// 内存地址类型
    /// </summary>
    public Type MemoryAddressType { get => memoryAddressType; }
    /// <summary>
    /// 是否正在监听
    /// </summary>
    public bool IsListening
    {
        get { return isListening; }
        set { listeningStateChanged?.Invoke(this, value); isListening = value; }
    }
    private protected nint processHandler;
    /// <summary>
    /// 当前进程句柄
    /// </summary>
    public nint ProcessHandler { get => processHandler; set => processHandler = value; }
    /// <summary>
    /// 当前的监听线程
    /// </summary>
    public Task? CurrentListeningTask { get; set; }
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <param name="processHandler">进程句柄</param>
    /// <param name="frequency">监听检测频率</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">监听已启动，无法重复启动监听</exception>
    public virtual async Task StartListen(nint processHandler, TimeSpan? frequency)
    {
        if (IsListening)
            throw new InvalidOperationException("监听已启动，无法重复启动监听");
        IsListening = true;
        this.frequency = frequency ?? TimeSpan.FromMilliseconds(1);
        this.processHandler = processHandler;
        await Task.CompletedTask;
    }
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <param name="processHandler">进程句柄</param>
    /// <param name="frequency">监听检测频率</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">监听已启动，无法重复启动监听</exception>
    public async Task StartListen(nint processHandler, int frequency = 1) => await StartListen(processHandler, TimeSpan.FromMilliseconds(frequency));
    /// <summary>
    /// 停止监听
    /// </summary>
    /// <returns></returns>
    public async Task StopListen()
    {
        IsListening = false;
        if (CurrentListeningTask is not null)
            await CurrentListeningTask;
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    protected void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                IsListening = false;
                CurrentListeningTask = null;
            }

            disposedValue = true;
        }
    }
    /// <summary>
    /// 释放监听器资源
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// 创建内存动态地址监听器
    /// </summary>
    /// <param name="customName">监听器自定义标识名</param>
    /// <param name="getAddressFunc">内存地址动态获取方法</param>
    /// <param name="memoryAddressType">内存地址数据类型</param>
    /// <returns></returns>
    public static DynamicMemoryListener CreateDynamicListener(string customName, Func<nint?> getAddressFunc, Type memoryAddressType) => new DynamicMemoryListenerImpl(customName, getAddressFunc, memoryAddressType);
    /// <summary>
    /// 创建内存静态地址监听器
    /// </summary>
    /// <param name="customName">监听器自定义标识名</param>
    /// <param name="memoryAddress">内存地址</param>
    /// <param name="memoryAddressType">内存地址数据类型</param>
    /// <returns></returns>
    public static StaticMemoryListener CreateStaticListener(string customName, nint memoryAddress, Type memoryAddressType) => new StaticMemoryListenerImpl(customName, memoryAddress, memoryAddressType);
    /// <summary>
    /// 创建可更新的内存地址监听器
    /// </summary>
    /// <param name="customName">监听器自定义标识名</param>
    /// <param name="memoryAddressUpdateFunc">存地址动态更新方法</param>
    /// <param name="memoryAddressType">内存地址数据类型</param>
    /// <returns></returns>
    public static UpdatableMemoryListener CreateUpdatableListener(string customName, Func<nint?> memoryAddressUpdateFunc, Type memoryAddressType) => new UpdatableMemoryListenerImpl(customName, memoryAddressUpdateFunc, memoryAddressType);
}
