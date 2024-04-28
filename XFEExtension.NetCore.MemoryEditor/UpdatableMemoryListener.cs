using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 可更新的内存监听器
/// </summary>
/// <param name="customName"></param>
/// <param name="updateAddressFunc"></param>
/// <param name="memoryAddressType"></param>
[CreateImpl]
public abstract class UpdatableMemoryListener(string customName, Func<nint?> updateAddressFunc, Type memoryAddressType) : MemoryListener(customName, memoryAddressType)
{
    private nint memoryAddress = updateAddressFunc.Invoke() ?? default;
    /// <summary>
    /// 内存地址
    /// </summary>
    public nint MemoryAddress
    {
        get { return memoryAddress; }
        set { memoryAddress = value; }
    }
    private Func<nint?> updateAddressFunc = updateAddressFunc;
    /// <summary>
    /// 获取目标内存地址方法
    /// </summary>
    public Func<nint?> UpdateAddressFunc { get => updateAddressFunc; set => updateAddressFunc = value; }
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <param name="processHandler">监听的进程句柄</param>
    /// <param name="frequency">监听检测频率</param>
    /// <returns></returns>
    public override async Task StartListen(nint processHandler, TimeSpan? frequency)
    {
        await base.StartListen(processHandler, frequency);
        CurrentListeningTask = Task.Run(async () =>
        {
            object? lastValue = null;
            try { lastValue = MemoryEditor.ReadMemory(this.processHandler, memoryAddress, memoryAddressType); } catch { }
            while (isListening)
            {
                try
                {
                    var currentValue = MemoryEditor.ReadMemory(this.processHandler, memoryAddress, memoryAddressType);
                    if (lastValue is null == currentValue is null || lastValue?.Equals(currentValue) == false)
                    {
                        valueChanged?.Invoke(this, new(lastValue is not null, currentValue is not null, lastValue, currentValue, name));
                        lastValue = currentValue;
                    }
                    await Task.Delay(this.frequency);
                }
                catch { }
            }
        });
        await CurrentListeningTask;
    }
    /// <summary>
    /// 更新地址值
    /// </summary>
    /// <param name="processHandler">进程句柄</param>
    public void UpdateAddress(nint processHandler = default)
    {
        if (processHandler != default)
        {
            ProcessHandler = processHandler;
        }
        memoryAddress = updateAddressFunc.Invoke() ?? memoryAddress;
    }
}