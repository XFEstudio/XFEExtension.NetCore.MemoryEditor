using System.Collections;
using XFEExtension.NetCore.ImplExtension;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 动态内存管理器
/// </summary>
[CreateImpl]
public abstract class DynamicMemoryManager : MemoryManager
{
    /// <summary>
    /// 地址值字典
    /// </summary>
    public Dictionary<string, DynamicMemoryItem> ItemDictionary { get; set; } = [];
    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="addressName">地址标识名</param>
    /// <returns></returns>
    public DynamicMemoryItem this[string addressName] => ItemDictionary[addressName];
    internal override void Add(MemoryItemBuilder builder)
    {
        if (builder.useResolvePointer)
        {
            if (builder.moduleName is null)
                throw new ArgumentNullException(nameof(builder), "对于使用基址指针表达式的构建器，模块名称是必须的");
            builder.dynamicMemoryAddress = new(() =>
            {
                try { return Editor.ResolvePointerAddress(builder.moduleName, builder.baseAddress, builder.offsets); } catch { return null; }
            });
        }
        else
        {
            if (builder.dynamicMemoryAddress is null)
                throw new ArgumentNullException(nameof(builder), "对于动态地址管理器，构建器的动态地址不可为空");
        }
        var dynamicMemoryItem = new DynamicMemoryItemImpl(builder.name, builder.dynamicMemoryAddress, builder.memoryAddressType, Editor);
        if (builder.hasListener)
            dynamicMemoryItem.AddListener(builder.ListenerFrequency, builder.startListen);
        ItemDictionary.Add(builder.name, dynamicMemoryItem);
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
    /// 动态内存管理器
    /// </summary>
    /// <param name="builders">内存构建器组</param>
    internal DynamicMemoryManager(params MemoryItemBuilder[] builders)
    {
        Add(builders);
        Editor.ValueChanged += (sender, e) => valueChanged?.Invoke(ItemDictionary[sender.Name], e);
    }
}
