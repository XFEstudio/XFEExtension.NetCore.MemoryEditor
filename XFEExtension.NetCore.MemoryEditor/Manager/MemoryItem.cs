namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存条目
/// </summary>
/// <param name="name">名称</param>
/// <param name="memoryItemType">内存地址数据类型</param>
/// <param name="memoryEditor">修改器</param>
public abstract class MemoryItem(string name, Type memoryItemType, MemoryEditor memoryEditor)
{
    private protected MemoryEditor memoryEditor = memoryEditor;
    /// <summary>
    /// 内存地址数据类型
    /// </summary>
    public Type MemoryItemType { get; set; } = memoryItemType;
    /// <summary>
    /// 内存地址标识名
    /// </summary>
    public string Name { get; init; } = name;
    /// <summary>
    /// 该地址是否被监听
    /// </summary>
    public bool IsListening { get; protected set; }
    /// <summary>
    /// 是否有监听器
    /// </summary>
    public abstract bool HasListener { get; }
    /// <summary>
    /// 开始监听该地址
    /// </summary>
    /// <param name="frequency">监听频率，单位为毫秒</param>
    public async Task StartListen(int frequency = 1) => await StartListen(TimeSpan.FromMilliseconds(frequency));
    /// <summary>
    /// 开始监听该地址
    /// </summary>
    /// <param name="frequency">监听频率</param>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task StartListen(TimeSpan? frequency = null)
    {
        if (IsListening)
            throw new InvalidOperationException("正在监听中，无法重复监听");
        IsListening = true;
        await StartListen(memoryEditor.ProcessHandler, frequency);
    }
    internal abstract Task StartListen(nint processHandler, TimeSpan? frequency);
    /// <summary>
    /// 为该地址添加监听器
    /// </summary>
    /// <param name="frequency">监听频率</param>
    /// <param name="startListen">是否在添加后启用监听</param>
    public abstract void AddListener(TimeSpan? frequency = null, bool startListen = false);
    /// <summary>
    /// 移除监听器
    /// </summary>
    public abstract void RemoveListener();
    /// <summary>
    /// 停止监听
    /// </summary>
    public abstract Task StopListen();
    /// <summary>
    /// 写入该内存
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="value">写入值</param>
    /// <returns>是否写入成功</returns>
    public abstract bool Write<T>(T value) where T : struct;
    /// <summary>
    /// 读取该内存的值
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>返回结果</returns>
    public T Read<T>() where T : struct
    {
        Read<T>(out T result);
        return result;
    }
    /// <summary>
    /// 读取该内存的值
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="result">读取的值</param>
    /// <returns>是否读取成功</returns>
    public abstract bool Read<T>(out T result) where T : struct;
}