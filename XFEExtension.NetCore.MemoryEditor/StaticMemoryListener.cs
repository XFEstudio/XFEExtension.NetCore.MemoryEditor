using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 内存静态监听器
/// </summary>
/// <param name="customName">监听器自定义标识名</param>
/// <param name="memoryAddress">内存地址</param>
/// <param name="memoryAddressType">内存地址数据类型</param>
[CreateImpl]
public abstract class StaticMemoryListener(string customName, nint memoryAddress, Type memoryAddressType) : MemoryListener(customName, memoryAddressType)
{
    private nint memoryAddress = memoryAddress;
    /// <summary>
    /// 内存地址
    /// </summary>
    public nint MemoryAddress
    {
        get { return memoryAddress; }
        set { memoryAddress = value; }
    }
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
                    if (lastValue is null != currentValue is null || lastValue?.Equals(currentValue) == false)
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
}
