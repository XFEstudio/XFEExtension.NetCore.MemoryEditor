
namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 监听器管理器基类
/// </summary>
public abstract class MemoryListenerManagerBase
{
    /// <summary>
    /// 开始静态监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="customName">自定义监听器标识名</param>
    /// <param name="memoryAddress">待监听的内存地址</param>
    /// <param name="frequency">检测频率，以毫秒为单位</param>
    /// <param name="startListen">启用监听</param>
    /// <returns></returns>
    public StaticMemoryListener AddStaticListener<T>(string customName, nint memoryAddress, int frequency = 1, bool startListen = true) where T : struct => AddStaticListener<T>(customName
        , memoryAddress, TimeSpan.FromMilliseconds(frequency), startListen);
    /// <summary>
    /// 开始静态监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="customName">自定义监听器标识名</param>
    /// <param name="memoryAddress">待监听的内存地址</param>
    /// <param name="frequency">检测频率</param>
    /// <param name="startListen">启用监听</param>
    /// <returns></returns>
    public StaticMemoryListener AddStaticListener<T>(string customName, nint memoryAddress, TimeSpan frequency, bool startListen = true) where T : struct => AddStaticListener(customName, memoryAddress, frequency, typeof(T), startListen);
    internal abstract StaticMemoryListener AddStaticListener(string customName, nint memoryAddress, TimeSpan? frequency, Type type, bool startListen);
    /// <summary>
    /// 开始动态监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="customName">自定义监听器标识名</param>
    /// <param name="getAddressFunc">内存地址动态获取方法</param>
    /// <param name="frequency">检测频率，以毫秒为单位</param>
    /// <param name="startListen">启用监听</param>
    /// <returns></returns>
    public DynamicMemoryListener AddDynamicListener<T>(string customName, Func<nint?> getAddressFunc, int frequency = 1, bool startListen = true) where T : struct => AddDynamicListener<T>(customName, getAddressFunc, TimeSpan.FromMilliseconds(frequency), startListen);
    /// <summary>
    /// 开始动态监听
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="customName">自定义监听器标识名</param>
    /// <param name="getAddressFunc">内存地址动态获取方法</param>
    /// <param name="frequency">检测频率</param>
    /// <param name="startListen">启用监听</param>
    /// <returns></returns>
    public DynamicMemoryListener AddDynamicListener<T>(string customName, Func<nint?> getAddressFunc, TimeSpan frequency, bool startListen = true) where T : struct => AddDynamicListener(customName, getAddressFunc, frequency, typeof(T), startListen);
    internal abstract DynamicMemoryListener AddDynamicListener(string customName, Func<nint?> memoryAddressGetFunc, TimeSpan? frequency, Type type, bool startListen);
    /// <summary>
    /// 开始可更新监听（在目标程序集变化时更新）
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="customName">自定义监听器标识名</param>
    /// <param name="memoryAddressUpdateFunc">内存地址动态更新方法</param>
    /// <param name="frequency">检测频率，以毫秒为单位</param>
    /// <param name="startListen">启用监听</param>
    /// <returns></returns>
    public UpdatableMemoryListener AddUpdatableListener<T>(string customName, Func<nint?> memoryAddressUpdateFunc, int frequency = 1, bool startListen = true) where T : struct => AddUpdatableListener<T>(customName, memoryAddressUpdateFunc, TimeSpan.FromMilliseconds(frequency), startListen);
    /// <summary>
    /// 开始可更新监听（在目标程序集变化时更新）
    /// </summary>
    /// <typeparam name="T">待监听的内存的数据类型（int,float,long等）</typeparam>
    /// <param name="customName">自定义监听器标识名</param>
    /// <param name="memoryAddressUpdateFunc">内存地址动态更新方法</param>
    /// <param name="frequency">检测频率</param>
    /// <param name="startListen">启用监听</param>
    /// <returns></returns>
    public UpdatableMemoryListener AddUpdatableListener<T>(string customName, Func<nint?> memoryAddressUpdateFunc, TimeSpan frequency, bool startListen = true) where T : struct => AddUpdatableListener(customName, memoryAddressUpdateFunc, frequency, typeof(T), startListen);
    internal abstract UpdatableMemoryListener AddUpdatableListener(string customName, Func<nint?> memoryAddressUpdateFunc, TimeSpan? frequency, Type type, bool startListen);
    /// <summary>
    /// 清除所有监听器
    /// </summary>
    public abstract void ClearListeners();
    /// <summary>
    /// 移除指定监听器
    /// </summary>
    /// <param name="customName">监听器自定义表示名称</param>
    public abstract void RemoveListener(string customName);
    /// <summary>
    /// 停止指定的监听器
    /// </summary>
    /// <param name="customName">监听器自定义表示名称</param>
    public abstract Task StopListener(string customName);
    /// <summary>
    /// 停止所有监听
    /// </summary>
    public abstract Task StopListeners();
}