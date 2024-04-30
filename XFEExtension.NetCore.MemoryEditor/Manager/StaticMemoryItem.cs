using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 静态内存
/// </summary>
/// <param name="name">地址自定义标识名称</param>
/// <param name="memoryAddress">内存地址</param>
/// <param name="memoryItemType">内存地址数据类型</param>
/// <param name="memoryEditor">内存修改器</param>
[CreateImpl]
public abstract class StaticMemoryItem(string name, nint memoryAddress, Type memoryItemType, MemoryEditor memoryEditor) : MemoryItem(name, memoryItemType, memoryEditor)
{
    /// <summary>
    /// 监听器
    /// </summary>
    public StaticMemoryListener? Listener { get; protected set; }
    /// <summary>
    /// 内存地址
    /// </summary>
    public nint MemoryAddress { get; init; } = memoryAddress;
    /// <inheritdoc/>
    public override bool HasListener { get => Listener is not null; }
    /// <inheritdoc/>
    public override void AddListener(TimeSpan? frequency = null, bool startListen = false) => Listener = memoryEditor.AddStaticListener(Name, MemoryAddress, frequency, MemoryItemType, startListen);
    /// <inheritdoc/>
    public override bool Read<T>(out T result) => memoryEditor.ReadMemory(MemoryAddress, out result);
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
        IsListening = false;
        await Listener.StopListen();
    }
    /// <inheritdoc/>
    public override bool Write<T>(T value) => memoryEditor.WriteMemory(MemoryAddress, value);
    internal override async Task StartListen(nint processHandler, TimeSpan? frequency = null)
    {
        if (Listener is null)
            throw new InvalidOperationException($"{nameof(Listener)} 监听器为空");
        IsListening = true;
        memoryEditor.ListenerManager.IsListening = true;
        await Listener.StartListen(processHandler, frequency);
    }
}