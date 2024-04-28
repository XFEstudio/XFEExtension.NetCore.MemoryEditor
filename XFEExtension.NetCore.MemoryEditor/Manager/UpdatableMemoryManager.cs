﻿using System.Collections;

namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 可更新内存管理器
/// </summary>
public abstract class UpdatableMemoryManager : MemoryManager
{
    /// <summary>
    /// 地址值字典
    /// </summary>
    public Dictionary<string, UpdatableMemoryItem> ItemDictionary { get; set; } = [];
    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="addressName">地址标识名</param>
    /// <returns></returns>
    public UpdatableMemoryItem this[string addressName] => ItemDictionary[addressName];
    internal override void Add(MemoryItemBuilder builder)
    {
        if (builder.UpdatableMemoryAddress is null)
            throw new ArgumentNullException(nameof(builder), "对于可更新内存管理器，构建器更新地址的方法不可为空");
        var updatableMemoryItem = new UpdatableMemoryItemImpl(builder.Name, builder.UpdatableMemoryAddress, builder.MemoryAddressType, Editor);
        if (builder.HasListener)
            updatableMemoryItem.AddListener(builder.ListenerFrequency, builder.StartListen);
        ItemDictionary.Add(builder.Name, updatableMemoryItem);
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
    /// 可更新内存管理器
    /// </summary>
    /// <param name="builders">内存构建器组</param>
    protected UpdatableMemoryManager(params MemoryItemBuilder[] builders) => Add(builders);
}