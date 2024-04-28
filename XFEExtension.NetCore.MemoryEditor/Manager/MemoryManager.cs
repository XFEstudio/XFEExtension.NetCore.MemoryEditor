using System.Collections;
using System.Diagnostics;
using XFEExtension.NetCore.DelegateExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存地址管理器
/// </summary>
public abstract class MemoryManager : IMemoryManager
{
    private protected bool disposedValue;
    private protected XFEEventHandler<MemoryItem, MemoryValue>? valueChanged;
    /// <summary>
    /// 当指定地址的内存值变化时触发
    /// </summary>
    public event XFEEventHandler<MemoryItem, MemoryValue>? ValueChanged { add => valueChanged += value; remove => valueChanged -= value; }
    /// <summary>
    /// 当前进程结束时触发
    /// </summary>
    public event EventHandler? CurrentProcessExit;
    /// <summary>
    /// 目标进程启动时触发
    /// </summary>
    public event EventHandler? CurrentProcessEntered;
    /// <summary>
    /// 当前目标进程
    /// </summary>
    public Process? CurrentProcess { get => Editor.CurrentProcess; set => Editor.CurrentProcess = value; }
    /// <summary>
    /// 进程句柄
    /// </summary>
    public nint ProcessHandler { get => Editor.ProcessHandler; set => Editor.ProcessHandler = value; }
    /// <summary>
    /// 当目标进程退出后，是否自动重新获取进程
    /// </summary>
    public bool AutoReacquireProcess { get => Editor.AutoReacquireProcess; set => Editor.AutoReacquireProcess = value; }
    /// <summary>
    /// 重新获取进程的检测频率（单位毫秒）
    /// </summary>
    public int ReacquireProcessFrequency { get => Editor.AutoReacquireProcessFrequency; set => Editor.AutoReacquireProcessFrequency = value; }
    /// <summary>
    /// 进程句柄权限（不懂勿填）
    /// </summary>
    public ProcessAccessFlags ProcessAccessFlags { get => Editor.ProcessAccessFlags; set => Editor.ProcessAccessFlags = value; }
    /// <summary>
    /// 进程位数类型（32/64）
    /// </summary>
    public ProcessType ProcessBiteType { get => Editor.ProcessBiteType; set => Editor.ProcessBiteType = value; }
    /// <summary>
    /// 编辑器
    /// </summary>
    public MemoryEditor Editor { get; protected set; } = new();
    /// <summary>
    /// 等待目标进程出现
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="frequency">检测频率</param>
    /// <returns></returns>
    public async Task WaitProcessEnter(string? processName = null, int frequency = 500) => await Editor.WaitProcessEnter(processName, frequency);
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
    /// <summary>
    /// 内存管理器
    /// </summary>
    protected MemoryManager()
    {
        Editor.CurrentProcessEntered += (sender, e) => CurrentProcessEntered?.Invoke(sender, e);
        Editor.CurrentProcessExit += (sender, e) => CurrentProcessExit?.Invoke(sender, e);
    }
    /// <summary>
    /// 创建内存管理器构建器
    /// </summary>
    /// <returns></returns>
    public static MemoryManagerBuilder CreateBuilder() => MemoryManagerBuilder.CreateBuilder();
}
