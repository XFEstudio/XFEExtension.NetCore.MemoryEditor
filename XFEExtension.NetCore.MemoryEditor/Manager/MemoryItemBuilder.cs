using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存条目构建器
/// </summary>
[CreateImpl]
public abstract class MemoryItemBuilder
{
    /// <summary>
    /// 内存地址标识名
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// 是否添加监听器
    /// </summary>
    public bool HasListener { get; set; }
    /// <summary>
    /// 是否立即开始监听（监听器会自动创建但是默认不会立即开始监听）
    /// </summary>
    public bool StartListen { get; set; }
    /// <summary>
    /// 内存地址类型
    /// </summary>
    public required Type MemoryAddressType { get; set; }
    /// <summary>
    /// 监听器监听频率
    /// </summary>
    public TimeSpan ListenerFrequency { get; set; }
    /// <summary>
    /// 内存地址
    /// </summary>
    public nint? StaticMemoryAddress { get; set; }
    /// <summary>
    /// 内存地址动态获取方法
    /// </summary>
    public Func<nint?>? DynamicMemoryAddress { get; set; }
    /// <summary>
    /// 可更新内存地址更新方法
    /// </summary>
    public Func<nint?>? UpdatableMemoryAddress { get; set; }
    /// <summary>
    /// 为该地址添加监听器
    /// </summary>
    /// <param name="frequency">监听频率</param>
    /// <param name="startListen">是否立即开始监听（监听器会自动创建但是默认不会立即开始监听）</param>
    /// <returns></returns>
    public MemoryItemBuilder WithListener(TimeSpan frequency, bool startListen = false)
    {
        HasListener = true;
        ListenerFrequency = frequency;
        StartListen = startListen;
        return this;
    }
    /// <summary>
    /// 为该地址添加监听器
    /// </summary>
    /// <param name="frequency">监听频率，单位为毫秒</param>
    /// <param name="startListen">是否立即开始监听（监听器会自动创建但是默认不会立即开始监听）</param>
    /// <returns></returns>
    public MemoryItemBuilder WithListener(int frequency = 1, bool startListen = false)
    {
        HasListener = true;
        ListenerFrequency = TimeSpan.FromMilliseconds(frequency);
        StartListen = startListen;
        return this;
    }
    /// <summary>
    /// 使用静态地址
    /// </summary>
    /// <param name="memoryAddress">静态地址</param>
    /// <returns></returns>
    public MemoryItemBuilder WithStaticMemoryAddress(nint memoryAddress)
    {
        StaticMemoryAddress = memoryAddress;
        return this;
    }
    /// <summary>
    /// 使用动态地址
    /// </summary>
    /// <param name="memoryAddressGetFunc">内存地址动态获取方法</param>
    /// <returns></returns>
    public MemoryItemBuilder WithDynamicMemoryAddress(Func<nint?> memoryAddressGetFunc)
    {
        DynamicMemoryAddress = memoryAddressGetFunc;
        return this;
    }
    /// <summary>
    /// 使用可更新内存地址
    /// </summary>
    /// <param name="memoryAddressUpdateFunc">内存地址更新方法</param>
    /// <returns></returns>
    public MemoryItemBuilder WithUpdatableMemoryAddress(Func<nint?> memoryAddressUpdateFunc)
    {
        UpdatableMemoryAddress = memoryAddressUpdateFunc;
        return this;
    }
    /// <summary>
    /// 构建静态地址
    /// </summary>
    /// <typeparam name="T">内存地址的数据类型</typeparam>
    /// <param name="name">内存地址自定义标识名</param>
    /// <returns></returns>
    public static MemoryItemBuilder Create<T>(string name) => new MemoryItemBuilderImpl { Name = name, MemoryAddressType = typeof(T) };
}
