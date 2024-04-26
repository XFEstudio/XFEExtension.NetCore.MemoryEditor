using System.Collections;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存地址管理器
/// </summary>
/// <param name="processHandler">目标进程句柄</param>
public abstract class MemoryManager(nint processHandler) : IDisposable, IEnumerable
{
    private protected bool disposedValue;
    private protected Dictionary<string, Func<nint?>> memoryAddressGetFuncDictionary = [];
    /// <summary>
    /// 监听器
    /// </summary>
    public MemoryListener Listener { get; protected set; } = new(processHandler);
    /// <summary>
    /// 现成句柄
    /// </summary>
    public nint ProcessHandler { get; set; } = processHandler;
    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="addressName">地址标识名称</param>
    /// <returns></returns>
    public abstract nint? this[string addressName] { get; }
    /// <summary>
    /// 添加地址
    /// </summary>
    /// <param name="addressName">标识名称</param>
    /// <param name="addressGetFunc">地址的获取方法</param>
    public abstract void Add(string addressName, Func<nint?> addressGetFunc);
    /// <summary>
    /// 设置指定名称的地址的获取方法
    /// </summary>
    /// <param name="addressName">地址标识名称</param>
    /// <param name="newAddressGetFun">新的获取方法</param>
    public abstract void Set(string addressName, Func<nint?> newAddressGetFun);
    public abstract void AddWithListener(s)
    /// <summary>
    /// 移除指定名称的地址
    /// </summary>
    /// <param name="addressName">地址名称</param>
    public abstract void Remove(string addressName);
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
                Listener.Dispose();
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
    public abstract IEnumerator GetEnumerator();
}
