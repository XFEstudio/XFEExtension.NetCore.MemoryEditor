using System.Collections;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 静态内存管理器
/// </summary>
public abstract class StaticMemoryManager : MemoryManager
{
    internal StaticMemoryManager(nint processHandler) : base(processHandler)
    {
    }

    /// <summary>
    /// 地址值字典
    /// </summary>
    public Dictionary<string, nint?> AddressDictionary { get; set; } = [];
    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="addressName">地址标识名</param>
    /// <returns></returns>
    public override nint? this[string addressName] => AddressDictionary[addressName];
    /// <summary>
    /// 添加地址（使用动态获取）
    /// </summary>
    /// <param name="addressName">标识名称</param>
    /// <param name="addressGetFunc">地址的获取方法</param>
    public override void Add(string addressName, Func<nint?> addressGetFunc)
    {
        memoryAddressGetFuncDictionary.Add(addressName, addressGetFunc);
        AddressDictionary.Add(addressName, addressGetFunc.Invoke());
    }
    /// <summary>
    /// 添加地址
    /// </summary>
    /// <param name="addressName">标识名称</param>
    /// <param name="address">地址</param>
    public void Add(string addressName, nint? address) => AddressDictionary.Add(addressName, address);
    /// <summary>
    /// 设置指定名称的地址的获取方法
    /// </summary>
    /// <param name="addressName">地址标识名称</param>
    /// <param name="newAddressGetFun">新的获取方法</param>
    public override void Set(string addressName, Func<nint?> newAddressGetFun)
    {
        memoryAddressGetFuncDictionary[addressName] = newAddressGetFun;
        AddressDictionary[addressName] = newAddressGetFun.Invoke();
    }
    /// <summary>
    /// 设置指定名称的地址的获取方法
    /// </summary>
    /// <param name="addressName">地址标识名称</param>
    /// <param name="newAddress">新的获取方法</param>
    public void Set(string addressName, nint? newAddress) => AddressDictionary[addressName] = newAddress;
    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public override IEnumerator GetEnumerator() => AddressDictionary.GetEnumerator();
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="addressName">地址标识名称</param>
    public override void Remove(string addressName)
    {
        AddressDictionary.Remove(addressName);
        memoryAddressGetFuncDictionary.Remove(addressName);
    }
}
