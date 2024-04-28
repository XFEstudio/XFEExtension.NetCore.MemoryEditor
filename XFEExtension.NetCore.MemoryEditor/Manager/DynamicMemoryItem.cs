using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 动态内存
/// </summary>
/// <param name="name">自定义标识名</param>
/// <param name="memoryAddressGetFunc">内存地址动态获取方法</param>
/// <param name="memoryItemType">内存地址数据类型</param>
/// <param name="memoryEditor">内存修改器</param>
[CreateImpl]
public abstract class DynamicMemoryItem(string name, Func<nint?> memoryAddressGetFunc, Type memoryItemType, MemoryEditor memoryEditor) : MemoryItem(name, memoryItemType, memoryEditor)
{
    /// <summary>
    /// 内存地址动态获取方法
    /// </summary>
    public Func<nint?> MemoryAddressGetFunc { get; init; } = memoryAddressGetFunc;
    /// <summary>
    /// 监听器
    /// </summary>
    public DynamicMemoryListener? Listener { get; protected set; }
    /// <inheritdoc/>
    public override bool HasListener { get => Listener is not null; }
    /// <inheritdoc/>
    public override void AddListener(TimeSpan? frequency = null, bool startListen = false) => Listener = memoryEditor.AddDynamicListener(Name, MemoryAddressGetFunc, frequency, MemoryItemType, startListen);
    /// <inheritdoc/>
    public override bool Read<T>(out T result) => memoryEditor.ReadMemory(MemoryAddressGetFunc.Invoke()!.Value, out result);
    /// <inheritdoc/>
    public override void RemoveListener()
    {
        memoryEditor.RemoveListener(Name);
        Listener = null;
    }
    /// <inheritdoc/>
    public override async Task StopListen()
    {
        if (Listener is null)
            throw new InvalidOperationException($"{nameof(Listener)} 监听器为空");
        await Listener.StopListen();
    }
    /// <inheritdoc/>
    public override bool Write<T>(T value) => memoryEditor.WriteMemory(MemoryAddressGetFunc.Invoke()!.Value, value);
    internal override async Task StartListen(nint processHandler, TimeSpan? frequency = null)
    {
        if (Listener is null)
            throw new InvalidOperationException($"{nameof(Listener)} 监听器为空");
        await Listener.StartListen(processHandler, frequency);
    }
}
