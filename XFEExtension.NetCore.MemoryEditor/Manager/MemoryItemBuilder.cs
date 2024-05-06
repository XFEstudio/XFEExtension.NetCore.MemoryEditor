using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存条目构建器
/// </summary>
[CreateImpl]
public abstract class MemoryItemBuilder
{
    internal string name = string.Empty;
    internal bool hasListener;
    internal bool startListen;
    internal bool useResolvePointer;
    internal Type memoryAddressType = typeof(int);
    internal TimeSpan listenerFrequency;
    internal nint? staticMemoryAddress;
    internal Func<nint?>? dynamicMemoryAddress;
    internal Func<nint?>? updatableMemoryAddress;
    internal string? moduleName;
    internal nint baseAddress;
    internal nint[] offsets = [];
    /// <summary>
    /// 为该地址添加监听器
    /// </summary>
    /// <param name="frequency">监听频率</param>
    /// <param name="startListen">是否立即开始监听（监听器会自动创建但是默认不会立即开始监听）</param>
    /// <returns></returns>
    public MemoryItemBuilder WithListener(TimeSpan frequency, bool startListen = false)
    {
        hasListener = true;
        ListenerFrequency = frequency;
        this.startListen = startListen;
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
        hasListener = true;
        ListenerFrequency = TimeSpan.FromMilliseconds(frequency);
        this.startListen = startListen;
        return this;
    }
    /// <summary>
    /// 使用静态地址
    /// </summary>
    /// <param name="memoryAddress">静态地址</param>
    /// <returns></returns>
    public MemoryItemBuilder WithStaticMemoryAddress(nint memoryAddress)
    {
        staticMemoryAddress = memoryAddress;
        return this;
    }
    /// <summary>
    /// 使用动态地址
    /// </summary>
    /// <param name="memoryAddressGetFunc">内存地址动态获取方法</param>
    /// <returns></returns>
    public MemoryItemBuilder WithDynamicMemoryAddress(Func<nint?> memoryAddressGetFunc)
    {
        dynamicMemoryAddress = memoryAddressGetFunc;
        return this;
    }
    /// <summary>
    /// 使用基址指针动态表达式
    /// </summary>
    /// <param name="moduleName">模块名称</param>
    /// <param name="baseAddress">模块的地址部分</param>
    /// <param name="offsets">基址指针偏移部分</param>
    /// <returns></returns>
    public MemoryItemBuilder WithResolvePointer(string moduleName, nint baseAddress, params nint[] offsets)
    {
        useResolvePointer = true;
        this.moduleName = moduleName;
        this.baseAddress = baseAddress;
        this.offsets = offsets;
        return this;
    }
    /// <summary>
    /// 使用可更新内存地址
    /// </summary>
    /// <param name="memoryAddressUpdateFunc">内存地址更新方法</param>
    /// <returns></returns>
    public MemoryItemBuilder WithUpdatableMemoryAddress(Func<nint?> memoryAddressUpdateFunc)
    {
        updatableMemoryAddress = memoryAddressUpdateFunc;
        return this;
    }
    /// <summary>
    /// 构建内存地址
    /// </summary>
    /// <typeparam name="T">内存地址的数据类型</typeparam>
    /// <param name="name">内存地址自定义标识名</param>
    /// <returns></returns>
    public static MemoryItemBuilder Create<T>(string name) => new MemoryItemBuilderImpl { name = name, memoryAddressType = typeof(T) };
}
