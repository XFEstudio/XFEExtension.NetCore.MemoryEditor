using System.Collections;
using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 静态内存管理器
/// </summary>
[CreateImpl]
public class StaticMemoryManager : MemoryManager
{
    /// <summary>
    /// 地址值字典
    /// </summary>
    public Dictionary<string, StaticMemoryItem> ItemDictionary { get; set; } = [];
    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="addressName">地址标识名</param>
    /// <returns></returns>
    public StaticMemoryItem this[string addressName] => ItemDictionary[addressName];
    internal override void Add(MemoryItemBuilder builder)
    {
        if (builder.StaticMemoryAddress is null)
            throw new ArgumentNullException(nameof(builder), "对于静态地址管理器，构建器的静态地址不可为空");
        var staticMemoryItem = new StaticMemoryItemImpl(builder.Name, builder.StaticMemoryAddress.Value, builder.MemoryAddressType, Editor);
        if (builder.HasListener)
            staticMemoryItem.AddListener(builder.ListenerFrequency, builder.StartListen);
        ItemDictionary.Add(builder.Name, staticMemoryItem);
    }
    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public override IEnumerator GetEnumerator() => ItemDictionary.GetEnumerator();
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="addressName">地址标识名称</param>
    public override void Remove(string addressName) => ItemDictionary.Remove(addressName);
    /// <summary>
    /// 静态内存管理器
    /// </summary>
    /// <param name="builders">内存构建器组</param>
    internal StaticMemoryManager(params MemoryItemBuilder[] builders) => Add(builders);
}