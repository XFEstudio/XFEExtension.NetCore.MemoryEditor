using System.Collections;
using System.Diagnostics;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存管理器
/// </summary>
public interface IMemoryManager : IEnumerable, IDisposable
{
    /// <summary>
    /// 当前目标进程
    /// </summary>
    Process? CurrentProcess { get; set; }
    /// <summary>
    /// 内存编辑器
    /// </summary>
    MemoryEditor Editor { get; }
    /// <summary>
    /// 进程句柄
    /// </summary>
    nint ProcessHandler { get; set; }
    /// <summary>
    /// 移除指定标识名的地址
    /// </summary>
    /// <param name="addressName"></param>
    void Remove(string addressName);
}