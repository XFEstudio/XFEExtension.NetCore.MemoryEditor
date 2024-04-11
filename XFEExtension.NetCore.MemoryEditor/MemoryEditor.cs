using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using XFEExtension.NetCore.DelegateExtension;

namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 内存编辑器
/// </summary>
public partial class MemoryEditor
{
    /// <summary>
    /// 当指定地址的内存值变化时触发
    /// </summary>
    public event XFEEventHandler<nint, object>? ValueChanged;
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
    public MemoryListener Listener { get; set; }
    /// <summary>
    /// 进程位数类型（32/64）
    /// </summary>
    public ProcessType ProcessBiteType { get; set; }
    /// <summary>
    /// 读取指定地址的内存
    /// </summary>
    /// <typeparam name="T">目标类型（int,float,long等）</typeparam>
    /// <param name="address">指定地址</param>
    /// <returns></returns>
    public T ReadMemory<T>(nint address) where T : struct => ReadMemory<T>(ProcessHandle, address);
    /// <summary>
    /// 在指定内存地址中写入数据
    /// </summary>
    /// <typeparam name="T">数据类型（int,float,long等）</typeparam>
    /// <param name="address">目标地址</param>
    /// <param name="source">待写入值</param>
    public void WriteMemory<T>(nint address, T source) where T : struct => WriteMemory<T>(ProcessHandle, address, source);
    /// <summary>
    /// 添加监听器
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="address">待监听的内存地址</param>
    public void AddListener<T>(nint address) where T : struct => _ = Listener.StartListen<T>(address);
    /// <summary>
    /// 移除并停止指定监听器
    /// </summary>
    /// <param name="address">监听地址</param>
    public void RemoveListener(nint address) => Listener.StopListener(address);
    /// <summary>
    /// 移除所有监听器
    /// </summary>
    public void RemoveListeners()=> Listener.StopListener();
    /// <summary>
    /// 解析基址对应的实际地址
    /// </summary>
    /// <param name="moduleName">模块名称（基址的进程部分，可以是DLL等）</param>
    /// <param name="baseAddress">基址的地址部分</param>
    /// <param name="offsets">基址的偏移组</param>
    /// <returns>实际地址</returns>
    public nint ResolvePointerAddress(string moduleName, int baseAddress, params int[] offsets) => ResolvePointerAddress(TargetProcess.ProcessName, moduleName, baseAddress, ProcessBiteType, offsets);
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
    /// <returns></returns>
    public static T ReadMemory<T>(nint processHandle, nint address) where T : struct
    {
        unsafe
        {
            var size = Marshal.SizeOf<T>();
            var buffer = new byte[size];
            ReadProcessMemory(processHandle, address, buffer, (uint)size, out _);
            fixed (byte* pBuffer = buffer)
            {
                return Marshal.PtrToStructure<T>((nint)pBuffer);
            }
        }
    }
    /// <summary>
    /// 在指定内存中写入数据
    /// </summary>
    /// <typeparam name="T">数据类型（int,float,long等）</typeparam>
    /// <param name="processHandle">进程句柄</param>
    /// <param name="address">目标地址</param>
    /// <param name="source">待写入值</param>
    public static void WriteMemory<T>(nint processHandle, nint address, T source) where T : struct
    {
        var buffer = StructureToByteArray(source);
        WriteProcessMemory(processHandle, address, buffer, (uint)buffer.Length, out _);
    }
    /// <summary>
    /// 解析基址对应的实际地址
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="moduleName">模块名称（基址的进程部分，可以是DLL等）</param>
    /// <param name="baseAddress">基址的地址部分</param>
    /// <param name="processType">目标进程类型</param>
    /// <param name="offsets">基址的偏移组</param>
    /// <returns>实际地址</returns>
    public static nint ResolvePointerAddress(string processName, string moduleName, int baseAddress, ProcessType processType, params int[] offsets)
    {
        var gameProcess = GetGameProcessByName(processName);
        if (gameProcess == null)
        {
            Trace.WriteLine("游戏进程未找到！");
            return nint.Zero;
        }
        var moduleBaseAddress = GetModuleBaseAddress(gameProcess, moduleName);
        if (moduleBaseAddress == nint.Zero)
        {
            Trace.WriteLine($"未找到模块：{moduleName}");
            return nint.Zero;
        }
        var resolvedAddress = nint.Add(moduleBaseAddress, baseAddress);
        foreach (int offset in offsets)
        {
            if (processType == ProcessType.Bit32)
            {
                var nextAddress = ReadMemory<int>(gameProcess.Handle, resolvedAddress);
                var pointerValue = new nint(nextAddress);
                Trace.WriteLine(pointerValue.ToString("X"));
                resolvedAddress = nint.Add(pointerValue, offset);
            }
            else
            {
                var nextAddress = ReadMemory<long>(gameProcess.Handle, resolvedAddress);
                var pointerValue = new nint(nextAddress);
                Trace.WriteLine(pointerValue.ToString("X"));
                resolvedAddress = nint.Add(pointerValue, offset);
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
