using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存管理器构建器
/// </summary>
[CreateImpl]
public abstract class MemoryManagerBuilder()
{
    /// <summary>
    /// 进程名称
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;
    /// <summary>
    /// 当目标进程退出后，是否自动重新获取进程
    /// </summary>
    public bool AutoReacquireProcess { get; set; }
    /// <summary>
    /// 重新获取进程的检测频率（单位毫秒）
    /// </summary>
    public int ReacquireProcessFrequency { get; set; }
    /// <summary>
    /// 是否在创建后自动开始寻找目标进程
    /// </summary>
    public bool FindProcessWhenCreate { get; set; }
    /// <summary>
    /// 创建内存地址管理器构建器
    /// </summary>
    /// <returns></returns>
    public static MemoryManagerBuilder CreateBuilder() => new MemoryManagerBuilderImpl();
    /// <summary>
    /// 使用自动重新获取进程
    /// </summary>
    /// <param name="reacquireProcessFrequency">重新获取进程的检测频率（单位毫秒）</param>
    /// <returns></returns>
    public MemoryManagerBuilder WithAutoReacquireProcess(int reacquireProcessFrequency = 500)
    {
        AutoReacquireProcess = true;
        ReacquireProcessFrequency = reacquireProcessFrequency;
        return this;
    }
    /// <summary>
    /// 在创建后自动开始寻找目标进程
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <returns></returns>
    public MemoryManagerBuilder WithFindProcessWhenCreate(string processName)
    {
        ProcessName = processName;
        FindProcessWhenCreate = true;
        return this;
    }
    /// <summary>
    /// 构建静态内存地址管理器
    /// </summary>
    /// <param name="builders">内存构建器组</param>
    /// <returns></returns>
    public StaticMemoryManager BuildStaticManager(params MemoryItemBuilder[] builders)
    {
        var manager = new StaticMemoryManagerImpl(builders)
        {
            AutoReacquireProcess = AutoReacquireProcess,
            ReacquireProcessFrequency = ReacquireProcessFrequency
        };
        if (FindProcessWhenCreate)
            _ = manager.WaitProcessEnter(ProcessName);
        return manager;
    }
    /// <summary>
    /// 构建动态内存地址管理器
    /// </summary>
    /// <param name="builders">内存构建器组</param>
    /// <returns></returns>
    public DynamicMemoryManager BuildDynamicManager(params MemoryItemBuilder[] builders)
    {
        var manager = new DynamicMemoryManagerImpl(builders)
        {
            AutoReacquireProcess = AutoReacquireProcess,
            ReacquireProcessFrequency = ReacquireProcessFrequency
        };
        if (FindProcessWhenCreate)
            _ = manager.WaitProcessEnter(ProcessName);
        return manager;
    }
    /// <summary>
    /// 构建可更新内存地址管理器
    /// </summary>
    /// <param name="builders">内存构建器组</param>
    /// <returns></returns>
    public UpdatableMemoryManager BuildUpdatableManager(params MemoryItemBuilder[] builders)
    {
        var manager = new UpdatableMemoryManager(builders)
        {
            AutoReacquireProcess = AutoReacquireProcess,
            ReacquireProcessFrequency = ReacquireProcessFrequency
        };
        if (FindProcessWhenCreate)
            _ = manager.WaitProcessEnter(ProcessName);
        return manager;
    }
}
