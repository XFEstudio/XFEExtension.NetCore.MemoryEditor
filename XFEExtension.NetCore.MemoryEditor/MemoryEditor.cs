using System.Diagnostics;
using System.Runtime.InteropServices;
using XFEExtension.NetCore.DelegateExtension;

namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 内存编辑器
/// </summary>
public partial class MemoryEditor : IDisposable
{
    private bool disposedValue;
    /// <summary>
    /// 当指定地址的内存值变化时触发
    /// </summary>
    public event XFEEventHandler<nint?, MemoryValue>? ValueChanged;
    /// <summary>
    /// 目标进程
    /// </summary>
    public Process TargetProcess { get; set; }
    /// <summary>
    /// 进程句柄
    /// </summary>
    public nint ProcessHandle { get; set; }
    /// <summary>
    /// 内存监听器
    /// </summary>
    public MemoryListenerManager Listener { get; set; }
    /// <summary>
    /// 进程位数类型（32/64）
    /// </summary>
    public ProcessType ProcessBiteType { get; set; }
    /// <summary>
    /// 读取指定地址的内存
    /// </summary>
    /// <typeparam name="T">目标类型（int,float,long等）</typeparam>
    /// <param name="address">指定地址</param>
    /// <param name="result">读取结果</param>
    /// <returns>是否读取成功</returns>
    public bool ReadMemory<T>(nint address, out T result) where T : struct => ReadMemory<T>(ProcessHandle, address, out result);
    /// <summary>
    /// 在指定内存地址中写入数据
    /// </summary>
    /// <typeparam name="T">数据类型（int,float,long等）</typeparam>
    /// <param name="address">目标地址</param>
    /// <param name="source">待写入值</param>
    /// <returns>是否写入成功</returns>
    public bool WriteMemory<T>(nint address, T source) where T : struct => WriteMemory<T>(ProcessHandle, address, source);
    /// <summary>
    /// 添加监听器
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="address">待监听的内存地址</param>
    /// <param name="customName">自定义监听器标识名</param>
    public void AddListener<T>(nint address, string customName = "") where T : struct => _ = Listener.StartListen<T>(address, customName);
    /// <summary>
    /// 添加监听器
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="address">待监听的内存地址</param>
    /// <param name="delay">检测频率，以毫秒为单位</param>
    /// <param name="customName">自定义监听器标识名</param>
    public void AddListener<T>(nint address, int delay, string customName = "") where T : struct => _ = Listener.StartListen<T>(address, delay, customName);
    /// <summary>
    /// 添加监听器
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="address">待监听的内存地址</param>
    /// <param name="delay">检测频率</param>
    /// <param name="customName">自定义监听器标识名</param>
    public void AddListener<T>(nint address, TimeSpan delay, string customName = "") where T : struct => _ = Listener.StartListen<T>(address, delay, customName);
    /// <summary>
    /// 添加监听器
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="getMemoryAddressFunc">动态获取内存地址的方法</param>
    /// <param name="delay">检测频率，以毫秒为单位</param>
    /// <param name="customName">自定义监听器标识名</param>
    public void AddListener<T>(Func<nint?> getMemoryAddressFunc, int delay, string customName = "") where T : struct => _ = Listener.StartListen<T>(getMemoryAddressFunc, delay, customName);
    /// <summary>
    /// 添加监听器
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="getMemoryAddressFunc">动态获取内存地址的方法</param>
    /// <param name="delay">检测频率</param>
    /// <param name="customName">自定义监听器标识名</param>
    public void AddListener<T>(Func<nint?> getMemoryAddressFunc, TimeSpan delay, string customName = "") where T : struct => _ = Listener.StartListen<T>(getMemoryAddressFunc, delay, customName);
    /// <summary>
    /// 移除并停止指定监听器
    /// </summary>
    /// <param name="address">监听地址</param>
    public void RemoveListener(nint address) => Listener.StopListener(address);
    /// <summary>
    /// 移除并停止指定监听器
    /// </summary>
    /// <param name="customName">自定义名称</param>
    public void RemoveListener(string customName) => Listener.StopListener(customName);
    /// <summary>
    /// 移除所有监听器
    /// </summary>
    public void RemoveListeners() => Listener.StopListener();
    /// <summary>
    /// 解析基址对应的实际地址
    /// </summary>
    /// <param name="moduleName">模块名称（基址的进程部分，可以是DLL等）</param>
    /// <param name="baseAddress">基址的地址部分</param>
    /// <param name="offsets">基址的偏移组</param>
    /// <returns>实际地址</returns>
    public nint ResolvePointerAddress(string moduleName, int baseAddress, params nint[] offsets) => ResolvePointerAddress(TargetProcess, moduleName, baseAddress, ProcessBiteType, offsets);
    /// <summary>
    /// 解析基址对应的实际地址
    /// </summary>
    /// <param name="moduleBaseAddress">模块地址（基址的进程部分，可以是DLL等）</param>
    /// <param name="baseAddress">基址的地址部分</param>
    /// <param name="offsets">基址的偏移组</param>
    /// <returns>实际地址</returns>
    public nint ResolvePointerAddress(nint moduleBaseAddress, int baseAddress, params nint[] offsets) => ResolvePointerAddress(TargetProcess, moduleBaseAddress, baseAddress, ProcessBiteType, offsets);
    /// <summary>
    /// 内存编辑器
    /// </summary>
    /// <param name="process">进程</param>
    /// <param name="processType">进程位数类型（32/64）</param>
    /// <param name="processAccessFlags">进程访问权限</param>
    public MemoryEditor(Process process, ProcessType processType = ProcessType.Bit64, ProcessAccessFlags processAccessFlags = ProcessAccessFlags.All)
    {
        TargetProcess = process;
        ProcessHandle = GetProcessHandle(TargetProcess.Id, processAccessFlags);
        ProcessBiteType = processType;
        Listener = new(ProcessHandle);
        Listener.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);
    }
    /// <summary>
    /// 内存编辑器
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="processType">进程位数类型（32/64）</param>
    /// <param name="processAccessFlags">进程访问权限</param>
    public MemoryEditor(string processName, ProcessType processType = ProcessType.Bit64, ProcessAccessFlags processAccessFlags = ProcessAccessFlags.All)
    {
        TargetProcess = Process.GetProcessesByName(processName).First();
        ProcessHandle = GetProcessHandle(TargetProcess.Id, processAccessFlags);
        ProcessBiteType = processType;
        Listener = new(ProcessHandle);
        Listener.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);
    }
    /// <summary>
    /// 内存编辑器
    /// </summary>
    /// <param name="processId">进程PID</param>
    /// <param name="processType">进程位数类型（32/64）</param>
    /// <param name="processAccessFlags">进程访问权限</param>
    public MemoryEditor(int processId, ProcessType processType = ProcessType.Bit64, ProcessAccessFlags processAccessFlags = ProcessAccessFlags.All)
    {
        TargetProcess = Process.GetProcessById(processId);
        ProcessHandle = GetProcessHandle(TargetProcess.Id, processAccessFlags);
        ProcessBiteType = processType;
        Listener = new(ProcessHandle);
        Listener.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Listener.Dispose();
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
    #region 静态方法
    /// <summary>
    /// 获取程序句柄
    /// </summary>
    /// <param name="processId">进程ID</param>
    /// <param name="flags">进程访问权限</param>
    /// <returns></returns>
    public static nint GetProcessHandle(int processId, ProcessAccessFlags flags)
    {
        nint processHandle = OpenProcess((int)flags, false, processId);
        return processHandle;
    }
    /// <summary>
    /// 获取程序句柄
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="flags">进程访问权限</param>
    /// <returns></returns>
    public static nint GetProcessHandle(string processName, ProcessAccessFlags flags)
    {
        nint processHandle = OpenProcess((int)flags, false, Process.GetProcessesByName(processName).First().Id);
        return processHandle;
    }
    /// <summary>
    /// 读取指定地址的内存
    /// </summary>
    /// <typeparam name="T">目标类型（int,float,long等）</typeparam>
    /// <param name="processHandle">进程句柄</param>
    /// <param name="address">指定地址</param>
    /// <param name="result">读取结果</param>
    /// <returns>是否读取成功</returns>
    public static bool ReadMemory<T>(nint processHandle, nint address, out T result) where T : struct
    {
        unsafe
        {
            var size = Marshal.SizeOf<T>();
            var buffer = new byte[size];
            var boolResult = ReadProcessMemory(processHandle, address, buffer, (uint)size, out _);
            fixed (byte* pBuffer = buffer)
            {
                result = Marshal.PtrToStructure<T>((nint)pBuffer);
            }
            return boolResult;
        }
    }
    /// <summary>
    /// 读取指定地址的内存<br/>
    /// 如非有意使用，请转到<seealso cref="ReadMemory{T}(nint, nint, out T)"/>
    /// </summary>
    /// <param name="processHandle">进程句柄</param>
    /// <param name="address">指定地址</param>
    /// <param name="type">目标类型（int,float,long等）</param>
    /// <returns>读取结果</returns>
    public static object? ReadMemory(nint processHandle, nint address, Type type)
    {
        unsafe
        {
            var size = Marshal.SizeOf(type);
            var buffer = new byte[size];
            if (ReadProcessMemory(processHandle, address, buffer, (uint)size, out _))
            {
                fixed (byte* pBuffer = buffer)
                {
                    return Marshal.PtrToStructure((nint)pBuffer, type);
                }
            }
            return null;
        }
    }
    /// <summary>
    /// 在指定内存中写入数据
    /// </summary>
    /// <typeparam name="T">数据类型（int,float,long等）</typeparam>
    /// <param name="processHandle">进程句柄</param>
    /// <param name="address">目标地址</param>
    /// <param name="source">待写入值</param>
    /// <returns>是否写入成功</returns>
    public static bool WriteMemory<T>(nint processHandle, nint address, T source) where T : struct
    {
        var buffer = StructureToByteArray(source);
        return WriteProcessMemory(processHandle, address, buffer, (uint)buffer.Length, out _);
    }
    /// <summary>
    /// 解析基址对应的实际地址
    /// </summary>
    /// <param name="process">主进程</param>
    /// <param name="moduleName">模块名称（基址的进程部分，可以是DLL等）</param>
    /// <param name="baseAddress">基址的地址部分</param>
    /// <param name="processType">目标进程类型</param>
    /// <param name="offsets">基址的偏移组</param>
    /// <returns>实际地址</returns>
    public static nint ResolvePointerAddress(Process process, string moduleName, int baseAddress, ProcessType processType, params nint[] offsets) => ResolvePointerAddress(process, GetModuleBaseAddress(process, moduleName), baseAddress, processType, offsets);
    /// <summary>
    /// 解析基址对应的实际地址
    /// </summary>
    /// <param name="process">主进程</param>
    /// <param name="moduleBaseAddress">模块地址（基址的进程部分，可以是DLL等）</param>
    /// <param name="baseAddress">基址的地址部分</param>
    /// <param name="processType">目标进程类型</param>
    /// <param name="offsets">基址的偏移组</param>
    /// <returns>实际地址</returns>
    public static nint ResolvePointerAddress(Process process, nint moduleBaseAddress, int baseAddress, ProcessType processType, params nint[] offsets)
    {
        var resolvedAddress = nint.Add(moduleBaseAddress, baseAddress);
        if (processType == ProcessType.Bit32)
        {
            foreach (nint offset in offsets)
            {
                var success = ReadMemory(process.Handle, resolvedAddress, out int nextAddress);
                if (!success || nextAddress == nint.Zero)
                    return nint.Zero;
                var pointerValue = new nint(nextAddress);
                resolvedAddress = pointerValue + offset;
            }
        }
        else
        {
            foreach (nint offset in offsets)
            {
                var success = ReadMemory(process.Handle, resolvedAddress, out long nextAddress);
                if (!success || nextAddress == nint.Zero)
                    return nint.Zero;
                var pointerValue = new nint(nextAddress);
                resolvedAddress = pointerValue + offset;
            }
        }
        return resolvedAddress;
    }
    /// <summary>
    /// 通过模块名称获取模块地址
    /// </summary>
    /// <param name="process">模块所在进程</param>
    /// <param name="moduleName">模块名称</param>
    /// <returns></returns>
    public static nint GetModuleBaseAddress(Process process, string moduleName)
    {
        var module = process.Modules.Cast<ProcessModule>().FirstOrDefault(m => m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
        return module != null ? module.BaseAddress : nint.Zero;
    }
    internal static Process? GetGameProcessByName(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        return processes.Length > 0 ? processes[0] : null;
    }
    private static byte[] StructureToByteArray<T>(T source) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var buffer = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(source, ptr, true);
        Marshal.Copy(ptr, buffer, 0, size);
        Marshal.FreeHGlobal(ptr);
        return buffer;
    }
    #endregion
    #region DLL引用
    [LibraryImport("kernel32.dll")]
    internal static partial nint OpenProcess(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, [Out] byte[] lpBuffer, uint dwSize, out int lpNumberOfBytesRead);
    #endregion
}
