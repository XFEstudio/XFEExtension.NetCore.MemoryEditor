using System.Diagnostics;
using System.Runtime.InteropServices;

namespace XFEExtension.NetCore.MemoryEditor;

public partial class MemoryEditor
{
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
    /// <summary>
    /// 获取程序句柄
    /// </summary>
    /// <param name="processId">进程ID</param>
    /// <param name="flags">权限</param>
    /// <returns></returns>
    public static nint GetProcessHandle(int processId, ProcessAccessFlags flags)
    {
        nint processHandle = OpenProcess((int)flags, false, processId);
        return processHandle;
    }
    /// <summary>
    /// 获取程序句柄
    /// </summary>
    /// 
    /// <param name="flags">权限</param>
    /// <returns></returns>
    public static nint GetProcessHandle(string processName, ProcessAccessFlags flags)
    {
        nint processHandle = OpenProcess((int)flags, false, processId);
        return processHandle;
    }

    public static T ReadMemory<T>(nint processHandle, nint address) where T : struct
    {
        unsafe
        {
            var size = sizeof(T);
            var buffer = new byte[size];
            ReadProcessMemory(processHandle, address, buffer, (uint)size, out _);
            fixed (byte* pBuffer = buffer)
            {
                return Marshal.PtrToStructure<T>((nint)pBuffer);
            }
        }
    }

    // 写入进程内存
    public static void WriteMemory(nint processHandle, nint address, byte[] buffer)
    {
        WriteProcessMemory(processHandle, address, buffer, (uint)buffer.Length, out _);
    }

    // 解析指针地址
    public static nint ResolvePointerAddress(string processName, string moduleName, int firstAddress, int[] offsets, int size)
    {
        // 获取游戏进程
        var gameProcess = GetGameProcessByName(processName);
        if (gameProcess == null)
        {
            Console.WriteLine("游戏进程未找到！");
            return nint.Zero;
        }

        // 获取模块基址
        var moduleBaseAddress = GetModuleBaseAddress(gameProcess, moduleName);
        if (moduleBaseAddress == nint.Zero)
        {
            Console.WriteLine($"未找到模块：{moduleName}");
            return nint.Zero;
        }

        // 解析指针地址
        var resolvedAddress = nint.Add(moduleBaseAddress, firstAddress);
        foreach (int offset in offsets)
        {
            var longAddress = ReadMemory<long>(gameProcess.Handle, resolvedAddress);
            var pointerValue = new nint(longAddress);
            resolvedAddress = nint.Add(pointerValue, offset);
        }

        return resolvedAddress;
    }

    // 获取进程对象
    public static Process? GetGameProcessByName(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        return processes.Length > 0 ? processes[0] : null;
    }

    // 获取模块基址
    public static nint GetModuleBaseAddress(Process process, string moduleName)
    {
        var module = process.Modules.Cast<ProcessModule>().FirstOrDefault(m => m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
        return module != null ? module.BaseAddress : nint.Zero;
    }
}
