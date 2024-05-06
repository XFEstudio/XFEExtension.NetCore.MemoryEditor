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
        if (builder.staticMemoryAddress is null)
            throw new ArgumentNullException(nameof(builder), "对于静态地址管理器，构建器的静态地址不可为空");
        var staticMemoryItem = new StaticMemoryItemImpl(builder.name, builder.staticMemoryAddress.Value, builder.memoryAddressType, Editor);
        if (builder.hasListener)
            staticMemoryItem.AddListener(builder.ListenerFrequency, builder.startListen);
        ItemDictionary.Add(builder.name, staticMemoryItem);
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
    internal StaticMemoryManager(params MemoryItemBuilder[] builders)
    {
        Add(builders);
        Editor.ValueChanged += (sender, e) => valueChanged?.Invoke(ItemDictionary[sender.Name], e);
    }
}