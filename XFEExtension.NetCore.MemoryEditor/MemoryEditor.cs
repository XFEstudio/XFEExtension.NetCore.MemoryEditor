using System.Diagnostics;
using System.Runtime.InteropServices;
using XFEExtension.NetCore.DelegateExtension;
using XFEExtension.NetCore.MemoryEditor.Manager;

namespace XFEExtension.NetCore.MemoryEditor;

/// <summary>
/// 内存编辑器
/// </summary>
public partial class MemoryEditor : MemoryListenerManagerBase, IDisposable
{
    private bool disposedValue;
    private MemoryListenerManager listenerManager = new();
    /// <summary>
    /// 当指定地址的内存值变化时触发
    /// </summary>
    public event XFEEventHandler<MemoryListener, MemoryValue>? ValueChanged;
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
    public Process? CurrentProcess { get => listenerManager.CurrentProcess; set => listenerManager.CurrentProcess = value; }
    /// <summary>
    /// 进程句柄
    /// </summary>
    public nint ProcessHandler { get => listenerManager.ProcessHandler; set => listenerManager.ProcessHandler = value; }
    /// <summary>
    /// 内存监听器
    /// </summary>
    public MemoryListenerManager ListenerManager
    {
        get { return listenerManager; }
        set { listenerManager = value; }
    }
    /// <summary>
    /// 当目标进程退出后，是否自动重新获取进程
    /// </summary>
    public bool AutoReacquireProcess { get => listenerManager.AutoReacquireProcess; set => listenerManager.AutoReacquireProcess = value; }
    /// <summary>
    /// 自动重新获取进程的检测频率（单位毫秒）
    /// </summary>
    public int AutoReacquireProcessFrequency { get => listenerManager.ReacquireProcessFrequency; set => listenerManager.ReacquireProcessFrequency = value; }
    /// <summary>
    /// 进程句柄权限（不懂勿填）
    /// </summary>
    public ProcessAccessFlags ProcessAccessFlags { get => listenerManager.ProcessAccessFlags; set => listenerManager.ProcessAccessFlags = value; }
    /// <summary>
    /// 进程位数类型（32/64）
    /// </summary>
    public ProcessType ProcessBiteType { get; set; }
    /// <summary>
    /// 等待目标进程出现
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="frequency">检测频率</param>
    /// <returns></returns>
    public async Task WaitProcessEnter(string? processName = null, int frequency = 500) => await listenerManager.WaitProcessEnter(processName, frequency);
    /// <summary>
    /// 读取指定地址的内存
    /// </summary>
    /// <typeparam name="T">目标类型（int,float,long等）</typeparam>
    /// <param name="address">指定地址</param>
    /// <param name="result">读取结果</param>
    /// <returns>是否读取成功</returns>
    public bool ReadMemory<T>(nint address, out T result) where T : struct => ReadMemory(ProcessHandler, address, out result);
    /// <summary>
    /// 在指定内存地址中写入数据
    /// </summary>
    /// <typeparam name="T">数据类型（int,float,long等）</typeparam>
    /// <param name="address">目标地址</param>
    /// <param name="source">待写入值</param>
    /// <returns>是否写入成功</returns>
    public bool WriteMemory<T>(nint address, T source) where T : struct => WriteMemory(ProcessHandler, address, source);
    /// <summary>
    /// 解析基址对应的实际地址
    /// </summary>
    /// <param name="moduleName">模块名称（基址的进程部分，可以是DLL等）</param>
    /// <param name="baseAddress">基址的地址部分</param>
    /// <param name="offsets">基址的偏移组</param>
    /// <returns>实际地址</returns>
    public nint ResolvePointerAddress(string moduleName, nint baseAddress, params nint[] offsets) => CurrentProcess is null ? default : ResolvePointerAddress(CurrentProcess, moduleName, baseAddress, ProcessBiteType, offsets);
    /// <summary>
    /// 解析基址对应的实际地址
    /// </summary>
    /// <param name="moduleBaseAddress">模块地址（基址的进程部分，可以是DLL等）</param>
    /// <param name="baseAddress">基址的地址部分</param>
    /// <param name="offsets">基址的偏移组</param>
    /// <returns>实际地址</returns>
    public nint ResolvePointerAddress(nint moduleBaseAddress, int baseAddress, params nint[] offsets) => CurrentProcess is null ? default : ResolvePointerAddress(CurrentProcess, moduleBaseAddress, baseAddress, ProcessBiteType, offsets);
    internal override StaticMemoryListener AddStaticListener(string customName, nint memoryAddress, TimeSpan? frequency, Type type, bool startListen) => ListenerManager.AddStaticListener(customName, memoryAddress, frequency, type, startListen);
    internal override DynamicMemoryListener AddDynamicListener(string customName, Func<nint?> memoryAddressGetFunc, TimeSpan? frequency, Type type, bool startListen) => ListenerManager.AddDynamicListener(customName, memoryAddressGetFunc, frequency, type, startListen);
    internal override UpdatableMemoryListener AddUpdatableListener(string customName, Func<nint?> memoryAddressUpdateFunc, TimeSpan? frequency, Type type, bool startListen) => ListenerManager.AddUpdatableListener(customName, memoryAddressUpdateFunc, frequency, type, startListen);
    /// <inheritdoc/>
    public override void ClearListeners() => ListenerManager.ClearListeners();
    /// <inheritdoc/>
    public override void RemoveListener(string customName) => ListenerManager.RemoveListener(customName);
    /// <inheritdoc/>
    public override Task StopListener(string customName) => ListenerManager.StopListener(customName);
    /// <inheritdoc/>
    public override Task StopListeners() => ListenerManager.StopListeners();
    /// <summary>
    /// 内存编辑器
    /// </summary>
    /// <param name="processBiteType">进程位数类型（32/64）</param>
    public MemoryEditor(ProcessType processBiteType = ProcessType.Bit64)
    {
        ProcessBiteType = processBiteType;
        ListenerManager.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);
        ListenerManager.CurrentProcessExit += (sender, e) => CurrentProcessExit?.Invoke(sender, e);
        ListenerManager.CurrentProcessEntered += (sender, e) => CurrentProcessEntered?.Invoke(sender, e);
    }
    /// <summary>
    /// 内存编辑器
    /// </summary>
    /// <param name="process">进程</param>
    /// <param name="processType">进程位数类型（32/64）</param>
    public MemoryEditor(Process process, ProcessType processType = ProcessType.Bit64)
    {
        CurrentProcess = process;
        ProcessBiteType = processType;
        ListenerManager.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);
        ListenerManager.CurrentProcessExit += (sender, e) => CurrentProcessExit?.Invoke(sender, e);
        ListenerManager.CurrentProcessEntered += (sender, e) => CurrentProcessEntered?.Invoke(sender, e);
    }
    /// <summary>
    /// 内存编辑器
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="processType">进程位数类型（32/64）</param>
    public MemoryEditor(string processName, ProcessType processType = ProcessType.Bit64)
    {
        CurrentProcess = Process.GetProcessesByName(processName).First();
        ProcessBiteType = processType;
        ListenerManager.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);
        ListenerManager.CurrentProcessExit += (sender, e) => CurrentProcessExit?.Invoke(sender, e);
        ListenerManager.CurrentProcessEntered += (sender, e) => CurrentProcessEntered?.Invoke(sender, e);
    }
    /// <summary>
    /// 内存编辑器
    /// </summary>
    /// <param name="processId">进程PID</param>
    /// <param name="processType">进程位数类型（32/64）</param>
    public MemoryEditor(int processId, ProcessType processType = ProcessType.Bit64)
    {
        CurrentProcess = Process.GetProcessById(processId);
        ProcessBiteType = processType;
        ListenerManager.ValueChanged += (sender, e) => ValueChanged?.Invoke(sender, e);
        ListenerManager.CurrentProcessExit += (sender, e) => CurrentProcessExit?.Invoke(sender, e);
        ListenerManager.CurrentProcessEntered += (sender, e) => CurrentProcessEntered?.Invoke(sender, e);
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
                ListenerManager.Dispose();
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
    public static nint ResolvePointerAddress(Process process, string moduleName, nint baseAddress, ProcessType processType, params nint[] offsets) => ResolvePointerAddress(process, GetModuleBaseAddress(process, moduleName), baseAddress, processType, offsets);
    /// <summary>
    /// 解析基址对应的实际地址
    /// </summary>
    /// <param name="process">主进程</param>
    /// <param name="moduleBaseAddress">模块地址（基址的进程部分，可以是DLL等）</param>
    /// <param name="baseAddress">基址的地址部分</param>
    /// <param name="processType">目标进程类型</param>
    /// <param name="offsets">基址的偏移组</param>
    /// <returns>实际地址</returns>
    public static nint ResolvePointerAddress(Process process, nint moduleBaseAddress, nint baseAddress, ProcessType processType, params nint[] offsets)
    {
        var resolvedAddress = moduleBaseAddress + baseAddress;
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
    internal static partial bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, [In] byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, [Out] byte[] lpBuffer, uint dwSize, out int lpNumberOfBytesRead);
    #endregion
}
