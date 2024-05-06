using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存管理器构建器
/// </summary>
[CreateImpl]
public abstract class MemoryManagerBuilder()
{
    internal string processName = string.Empty;
    internal bool autoReacquireProcess;
    internal int reacquireProcessFrequency;
    internal bool findProcessWhenCreate;
    /// <summary>
    /// 创建内存地址管理器构建器
    /// </summary>
    /// <returns></returns>
    public static MemoryManagerBuilder CreateBuilder() => new MemoryManagerBuilderImpl();
    /// <summary>
    /// 使用自动重新获取进程
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="reacquireProcessFrequency">重新获取进程的检测频率（单位毫秒）</param>
    /// <returns></returns>
    public MemoryManagerBuilder WithAutoReacquireProcess(string processName = "", int reacquireProcessFrequency = 500)
    {
        if (processName != string.Empty)
            this.processName = processName;
        autoReacquireProcess = true;
        this.reacquireProcessFrequency = reacquireProcessFrequency;
        return this;
    }
    /// <summary>
    /// 在创建后自动开始寻找目标进程
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <returns></returns>
    public MemoryManagerBuilder WithFindProcessWhenCreate(string processName = "")
    {
        if (processName != string.Empty)
            this.processName = processName;
        findProcessWhenCreate = true;
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
            AutoReacquireProcess = autoReacquireProcess,
            ReacquireProcessFrequency = reacquireProcessFrequency,
            ProcessName = processName
        };
        if (findProcessWhenCreate)
            _ = manager.WaitProcessEnter(manager.ProcessName);
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
            AutoReacquireProcess = autoReacquireProcess,
            ReacquireProcessFrequency = reacquireProcessFrequency,
            ProcessName = processName
        };
        if (findProcessWhenCreate)
            _ = manager.WaitProcessEnter(manager.ProcessName);
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
            AutoReacquireProcess = autoReacquireProcess,
            ReacquireProcessFrequency = reacquireProcessFrequency,
            ProcessName = processName
        };
        if (findProcessWhenCreate)
            _ = manager.WaitProcessEnter(manager.ProcessName);
        return manager;
    }
}
