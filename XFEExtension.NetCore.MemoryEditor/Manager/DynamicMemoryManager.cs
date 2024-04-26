using System.Collections;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 动态内存管理器
/// </summary>
public abstract class DynamicMemoryManager : MemoryManager
{
    internal DynamicMemoryManager(nint processHandler) : base(processHandler)
    {
    }
    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="addressName">地址名称</param>
    /// <returns></returns>
    public override nint? this[string addressName] => memoryAddressGetFuncDictionary[addressName].Invoke();
    /// <summary>
    /// 添加地址
    /// </summary>
    /// <param name="addressName">标识名称</param>
    /// <param name="addressGetFunc">地址的获取方法</param>
    public override void Add(string addressName, Func<nint?> addressGetFunc) => memoryAddressGetFuncDictionary.Add(addressName, addressGetFunc);
    /// <summary>
    /// 设置指定名称的地址的获取方法
    /// </summary>
    /// <param name="addressName">地址标识名称</param>
    /// <param name="newAddressGetFun">新的获取方法</param>
    public override void Set(string addressName, Func<nint?> newAddressGetFun) => memoryAddressGetFuncDictionary[addressName] = newAddressGetFun;
    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public override IEnumerator GetEnumerator() => memoryAddressGetFuncDictionary.Values.GetEnumerator();
    /// <summary>
    /// 移除指定名称的地址
    /// </summary>
    /// <param name="addressName">地址标识名称</param>
    public override void Remove(string addressName) => memoryAddressGetFuncDictionary.Remove(addressName);
}
