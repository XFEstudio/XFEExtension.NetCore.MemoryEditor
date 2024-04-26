﻿namespace XFEExtension.NetCore.MemoryEditor.Manager;

/// <summary>
/// 内存条目
/// </summary>
public abstract class MemoryItem(string name)
{
    /// <summary>
    /// 内存地址标识名
    /// </summary>
    public string Name { get; init; } = name;
    /// <summary>
    /// 内存地址
    /// </summary>
    public nint? MemoryAddress { get; internal set; }
    /// <summary>
    /// 内存地址动态获取方法
    /// </summary>
    public Func<nint?>? AddressGetFunc { get; internal set; }
    /// <summary>
    /// 该地址是否存在监听器
    /// </summary>
    public bool HasListener { get; set; }
    /// <summary>
    /// 添加监听器
    /// </summary>
    public abstract void AddListener();
    /// <summary>
    /// 移除监听器
    /// </summary>
    public abstract void RemoveListener();
}