namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 内存记录值
/// </summary>
/// <param name="PreviousValue">改变前的值</param>
/// <param name="CurrentValue">当前值</param>
/// <param name="CustomName">自定义标识名</param>
public record struct MemoryValue(object PreviousValue, object CurrentValue, string? CustomName) { }