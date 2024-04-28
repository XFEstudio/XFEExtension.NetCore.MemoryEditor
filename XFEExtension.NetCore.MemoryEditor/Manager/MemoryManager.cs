using System.Collections;
using System.Diagnostics;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存地址管理器
/// </summary>
public abstract class MemoryManager : IMemoryManager
{
    private protected bool disposedValue;
    /// <summary>
    /// 编辑器
    /// </summary>
    public MemoryEditor Editor { get; protected set; } = new();
    /// <summary>
    /// 现成句柄
    /// </summary>
    public nint ProcessHandler { get => Editor.ProcessHandle; set => Editor.ProcessHandle = value; }
    /// <summary>
    /// 当前目标进程
    /// </summary>
    public Process? CurrentProcess { get => Editor.CurrentProcess; set => Editor.CurrentProcess = value; }
    /// <summary>
    /// 移除指定名称的地址
    /// </summary>
    /// <param name="addressName">地址名称</param>
    public abstract void Remove(string addressName);
    internal abstract void Add(MemoryItemBuilder builder);
    /// <summary>
    /// 添加地址
    /// </summary>
    /// <param name="builders">内存地址构建器组</param>
    public void Add(params MemoryItemBuilder[] builders)
    {
        foreach (var builder in builders)
            Add(builder);
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    private protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Editor.Dispose();
            }
            disposedValue = true;
        }
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator GetEnumerator();
}
