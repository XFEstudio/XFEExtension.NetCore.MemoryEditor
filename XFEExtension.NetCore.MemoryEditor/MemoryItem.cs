namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 内存条目
/// </summary>
public abstract class MemoryItem
{
    /// <summary>
    /// 内存地址
    /// </summary>
    public nint? MemoryAddress { get; set; }
    /// <summary>
    /// 内存地址动态获取方法
    /// </summary>
    public Func<nint?>? AddressGetFunc { get; set; }
    /// <summary>
    /// 该地址是否存在监听器
    /// </summary>
    public bool HasListener { get; set; }
    public void AddListener(MemoryListener listener,)
    {
        listener.StartListen()
    }
}
