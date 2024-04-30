using System.Diagnostics;
using XFEExtension.NetCore.DelegateExtension;
using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 动态内存
/// </summary>
/// <param name="name">自定义标识名</param>
/// <param name="memoryAddressUpdateFunc">内存地址更新方法</param>
/// <param name="memoryItemType">内存地址数据类型</param>
/// <param name="memoryEditor">内存修改器</param>
[CreateImpl]
public abstract class UpdatableMemoryItem(string name, Func<nint?> memoryAddressUpdateFunc, Type memoryItemType, MemoryEditor memoryEditor) : MemoryItem(name, memoryItemType, memoryEditor)
{
    private UpdatableMemoryListener? listener;
    /// <summary>
    /// 监听器
    /// </summary>
    public UpdatableMemoryListener? Listener
    {
        get => listener;
        protected set
        {
            listener = value;
            if (listener is not null)
            {
                listener.MemoryAddressUpdated += Listener_MemoryAddressUpdated;
                MemoryAddress = listener.MemoryAddress;
            }
        }
    }
    /// <summary>
    /// 地址更新时触发
    /// </summary>
    public event XFEEventHandler<UpdatableMemoryListener, nint>? MemoryAddressUpdated;
    private void Listener_MemoryAddressUpdated(UpdatableMemoryListener sender, nint e)
    {
        if (MemoryAddress != e)
            MemoryAddress = e;
        MemoryAddressUpdated?.Invoke(sender, e);
    }
    /// <summary>
    /// 内存地址更新方法
    /// </summary>
    public Func<nint?> MemoryAddressUpdateFunc { get; init; } = memoryAddressUpdateFunc;
    /// <inheritdoc/>
    public override bool HasListener { get => Listener is not null; }
    /// <summary>
    /// 内存地址
    /// </summary>
    public nint MemoryAddress { get; set; } = memoryAddressUpdateFunc.Invoke() ?? default;
    /// <inheritdoc/>
    public override bool Read<T>(out T result) => memoryEditor.ReadMemory(MemoryAddress, out result);
    /// <inheritdoc/>
    public override async Task StopListen()
    {
        if (Listener is null)
            throw new InvalidOperationException($"{nameof(Listener)} 监听器为空");
        IsListening = false;
        await Listener.StopListen();
    }
    /// <inheritdoc/>
    public override void AddListener(TimeSpan? frequency = null, bool startListen = false) => Listener = memoryEditor.AddUpdatableListener(Name, MemoryAddressUpdateFunc, frequency, MemoryItemType, startListen);
    /// <inheritdoc/>
    public override void RemoveListener()
    {
        memoryEditor.RemoveListener(Name);
        Listener = null;
    }
    /// <inheritdoc/>
    public override bool Write<T>(T value) => memoryEditor.WriteMemory(MemoryAddress, value);
    /// <summary>
    /// 更新内存地址及当前目标进程句柄
    /// </summary>
    /// <param name="processHandler"></param>
    public void UpdateMemoryAddress(nint processHandler = default)
    {
        MemoryAddress = MemoryAddressUpdateFunc?.Invoke() ?? MemoryAddress;
        Listener?.UpdateAddress(processHandler);
    }
    internal override async Task StartListen(nint processHandler, TimeSpan? frequency = null)
    {
        if (Listener is null)
            throw new InvalidOperationException($"{nameof(Listener)} 监听器为空");
        IsListening = true;
        memoryEditor.ListenerManager.IsListening = true;
        await Listener.StartListen(processHandler, frequency);
    }
}
