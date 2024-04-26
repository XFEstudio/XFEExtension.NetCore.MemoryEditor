namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存条目
/// </summary>
/// <typeparam name="T">地址类型</typeparam>
/// <param name="name">名称</param>
/// <param name="memoryListener">监听器</param>
public abstract class MemoryItem<T>(string name, MemoryListenerManager memoryListener)
{
    private protected MemoryListenerManager memoryListener = memoryListener;
    /// <summary>
    /// 内存地址标识名
    /// </summary>
    public string Name { get; init; } = name;
    /// <summary>
    /// 内存地址
    /// </summary>
    public nint? MemoryAddress { get; internal set; }
    /// <summary>
    /// 内存地址动态获取方法
    /// </summary>
    public Func<nint?>? AddressGetFunc { get; internal set; }
    /// <summary>
    /// 该地址是否存在监听器
    /// </summary>
    public bool HasListener { get; protected set; }
    /// <summary>
    /// 添加监听器
    /// </summary>
    public abstract void AddListener();
    /// <summary>
    /// 移除监听器
    /// </summary>
    public abstract void RemoveListener();
}