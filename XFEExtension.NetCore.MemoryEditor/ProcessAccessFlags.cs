namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 进程访问权限
/// </summary>
[Flags]
public enum ProcessAccessFlags : uint
{
    /// <summary>
    /// 允许对进程进行所有操作的权限。
    /// </summary>
    All = 0x001F0FFF,
    /// <summary>
    /// 终止进程的权限。
    /// </summary>
    Terminate = 0x00000001,
    /// <summary>
    /// 在远程进程中创建线程的权限。
    /// </summary>
    CreateThread = 0x00000002,
    /// <summary>
    /// 对进程进行虚拟内存操作的权限。
    /// </summary>
    VirtualMemoryOperation = 0x00000008,
    /// <summary>
    /// 从进程中读取虚拟内存的权限。
    /// </summary>
    VirtualMemoryRead = 0x00000010,
    /// <summary>
    /// 向进程写入虚拟内存的权限。
    /// </summary>
    VirtualMemoryWrite = 0x00000020,
    /// <summary>
    /// 复制句柄的权限。
    /// </summary>
    DuplicateHandle = 0x00000040,
    /// <summary>
    /// 创建进程的权限。
    /// </summary>
    CreateProcess = 0x000000080,
    /// <summary>
    /// 设置进程配额的权限。
    /// </summary>
    SetQuota = 0x00000100,
    /// <summary>
    /// 设置进程信息的权限。
    /// </summary>
    SetInformation = 0x00000200,
    /// <summary>
    /// 查询进程信息的权限。
    /// </summary>
    QueryInformation = 0x00000400,
    /// <summary>
    /// 有限查询进程信息的权限。
    /// </summary>
    QueryLimitedInformation = 0x00001000,
    /// <summary>
    /// 同步的权限。
    /// </summary>
    Synchronize = 0x00100000
}
