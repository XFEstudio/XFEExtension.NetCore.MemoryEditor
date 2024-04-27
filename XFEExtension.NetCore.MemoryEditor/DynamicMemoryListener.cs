using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 内存动态监听器
/// </summary>
/// <param name="customName">监听器自定义标识名</param>
/// <param name="getAddressFunc">获取内存的动态方法</param>
/// <param name="memoryAddressType">内存地址数据类型</param>
[CreateImpl]
public abstract class DynamicMemoryListener(string customName, Func<nint?> getAddressFunc, Type memoryAddressType) : MemoryListener(customName, memoryAddressType)
{
    private readonly Func<nint?> getAddressFunc = getAddressFunc;
    /// <summary>
    /// 获取目标内存地址方法
    /// </summary>
    public Func<nint?> GetAddressFunc { get => getAddressFunc; }
    /// <summary>
    /// 开始监听
    /// </summary>
    /// <param name="processHandler">监听的进程句柄</param>
    /// <param name="frequency">监听检测频率</param>
    /// <returns></returns>
    public override async Task StartListen(nint processHandler, TimeSpan frequency)
    {
        await base.StartListen(processHandler, frequency);
        CurrentListeningTask = Task.Run(async () =>
        {
            var memoryAddress = getAddressFunc.Invoke();
            object? lastValue = null;
            if (memoryAddress is not null)
                try { lastValue = MemoryEditor.ReadMemory(processHandler, memoryAddress.Value, memoryAddressType); } catch { }
            while (isListening)
            {
                try
                {
                    memoryAddress = getAddressFunc.Invoke();
                    if (memoryAddress == 0)
                    {
                        if (lastValue is not null)
                            valueChanged?.Invoke(this, new(lastValue is not null, false, lastValue, null, name));
                        lastValue = null;
                        while (memoryAddress == 0 && isListening)
                        {
                            memoryAddress = getAddressFunc.Invoke();
                            await Task.Delay(frequency);
                        }
                    }
                    var currentValue = MemoryEditor.ReadMemory(processHandler, memoryAddress!.Value, memoryAddressType);
                    if (lastValue is null == currentValue is null || lastValue?.Equals(currentValue) == false)
                    {
                        valueChanged?.Invoke(this, new(lastValue is not null, currentValue is not null, lastValue, currentValue, name));
                        lastValue = currentValue;
                    }
                    await Task.Delay(frequency);
                }
                catch { }
            }
        });
        await CurrentListeningTask;
    }
}
