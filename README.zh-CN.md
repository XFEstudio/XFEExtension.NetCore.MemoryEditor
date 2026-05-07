# XFEExtension.NetCore.MemoryEditor

[![NuGet 版本](https://img.shields.io/nuget/v/XFEExtension.NetCore.MemoryEditor.svg)](https://www.nuget.org/packages/XFEExtension.NetCore.MemoryEditor/)
[![NuGet 下载量](https://img.shields.io/nuget/dt/XFEExtension.NetCore.MemoryEditor.svg)](https://www.nuget.org/packages/XFEExtension.NetCore.MemoryEditor/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)

[English](./README.md) | 简体中文

一个 .NET 内存读写工具库，支持基址指针解析，内置内存管理器。

## 安装

```bash
dotnet add package XFEExtension.NetCore.MemoryEditor
```

---

## 内存管理器

### 内存管理器类型说明

内存管理器是本开源包中力推的类，使用内存管理器可以方便快捷地开发出基于指定偏移地址的内存修改工具。

### 内存管理器类型

- **静态内存管理器** `StaticMemoryManager`：存放静态地址，该地址不会随着进程实例的不同而变动。
- **动态内存管理器** `DynamicMemoryManager`：存放地址的获取方法，每次监听器检测地址时都会调用该方法来动态获取地址。
- **可更新内存管理器** `UpdatableMemoryManager`：存放静态地址和该地址对应的更新方法，管理器默认会在目标进程更改时调用一次该方法，用户也可手动调用来更新地址。

### 内存管理器结构

内存管理器内部包含一个 `MemoryEditor`，而 `MemoryEditor` 内部则内置了一个 `MemoryListenerManager`。

### 创建内存管理器（推荐使用动态内存管理器或可更新内存管理器）

```csharp
public static class Program
{
    public static DynamicMemoryManager Manager { get; } = MemoryManager.CreateBuilder() // 创建内存管理器构建器
            .WithAutoReacquireProcess("ExampleGame") // 当目标进程退出后，自动重新获取目标名称的进程
            .WithFindProcessWhenCreate()              // 创建管理器时开始寻找目标进程
            .BuildDynamicManager(
            MemoryItemBuilder.Create<int>("Level")           // 为每个地址添加一个名称
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x14A0, 0x0, 0x80, 0xE4, 0x0, 0x1EC) // 模块名称、基址和偏移
                             .WithListener(),                // 添加监听器
            MemoryItemBuilder.Create<float>("HealthPoint")
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x12E8, 0x0, 0x80, 0xE4, 0x0, 0x1E0)
                             .WithListener(),
            MemoryItemBuilder.Create<float>("Stamina")
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x14E0, 0x48, 0x10, 0x20, 0x50, 0x20, 0x1B0)
                             .WithListener());
}
```

### 使用内存管理器

```csharp
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        Program.Manager.ValueChanged += Manager_ValueChanged; // 订阅内存值改变事件
    }

    private void Manager_ValueChanged(XFEExtension.NetCore.MemoryEditor.Manager.MemoryItem sender, MemoryValue e)
    {
        Trace.WriteLine($"名称：{e.CustomName} 地址：{sender:X}\t是否读取到值  上次：{e.PreviousValueGetSuccessful}  这次：{e.CurrentValueGetSuccessful}  值从：{e.PreviousValue}  变更为：{e.CurrentValue}");
        switch (e.CustomName)
        {
            case "Level":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(99)) // 替换为实际要写入的值
                        Trace.WriteLine("Level：写入失败");
                }
                break;
            case "HealthPoint":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(100.0f)) // 替换为实际要写入的值
                        Trace.WriteLine("HealthPoint：写入失败");
                }
                break;
            case "Stamina":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(100.0f)) // 替换为实际要写入的值
                        Trace.WriteLine("Stamina：写入失败");
                }
                break;
            default:
                break;
        }
    }
}
```

### 直接读写内存条目

```csharp
// 读取值
int level = Program.Manager["Level"].Read<int>();

// 写入值
bool success = Program.Manager["Level"].Write<int>(99);
```

---

## 内存修改器

### 内存修改器类型说明

内存修改器可以通过静态方法直接调用，也可以创建实例来针对某个特定进程进行内存编辑。

### 内存修改器层次结构

`MemoryEditor` 的实例内部内置一个 `MemoryListenerManager`。

### 创建内存修改器

```csharp
// 默认创建，稍后设置进程
var memoryEditor = new MemoryEditor();
memoryEditor.CurrentProcess = Process.GetProcessesByName("YourProcessName").First();

// 直接用进程名创建
var memoryEditor = new MemoryEditor("YourProcessName");

// 直接用进程对象创建
var memoryEditor = new MemoryEditor(process);

// 直接用进程PID创建
var memoryEditor = new MemoryEditor(processId);
```

### 实例方式读写内存

```csharp
bool writeSuccess = memoryEditor.WriteMemory(0x007621B5, 1234);
bool readSuccess = memoryEditor.ReadMemory<int>(0x007621B5, out var result);
Console.WriteLine(result);
```

### 使用内存修改器（不创建实例）

```csharp
nint processHandle = 0x00000a8c;
bool writeSuccess = MemoryEditor.WriteMemory(processHandle, 0x007621B5, 1234); // 通过进程句柄向指定地址写入值
bool readSuccess = MemoryEditor.ReadMemory<int>(processHandle, 0x007621B5, out var result); // 通过进程句柄读取指定地址的值
```

### 解析基址指针地址

```csharp
// 使用模块名称（实例方式）
nint address = memoryEditor.ResolvePointerAddress("game.dll", 0x0072A200, 0x14A0, 0x0, 0x1EC);

// 使用模块名称（静态方式）
nint address = MemoryEditor.ResolvePointerAddress(process, "game.dll", 0x0072A200, ProcessType.Bit64, 0x14A0, 0x0, 0x1EC);
```

---

## 内存监听器管理器

### 内存监听器管理器说明

推荐优先使用内存管理器；内存监听器管理器主要为内存管理器提供服务，但也可以单独使用。

### 内存监听器管理器层次结构

内部维护一个 `Dictionary<string, MemoryListener>` 对象，用于管理所有已创建的监听器。

### 创建内存监听器管理器

```csharp
// 通过进程名创建
var listenerManager = new MemoryListenerManager("YourProcessName");

// 空构建后手动设置进程
var listenerManager = new MemoryListenerManager();
listenerManager.CurrentProcess = Process.GetProcessesByName("YourProcessName").First();
```

### 添加监听器

```csharp
// 添加静态监听器
var listener = listenerManager.AddStaticListener("HP", 0x007621B5, TimeSpan.FromMilliseconds(100), typeof(float), startListen: true);

// 添加动态监听器
var listener = listenerManager.AddDynamicListener("Level", () => ResolveAddress(), TimeSpan.FromMilliseconds(100), typeof(int), startListen: true);
```

### 处理事件

```csharp
listenerManager.ValueChanged += (sender, e) =>
{
    Console.WriteLine($"{e.CustomName}：{e.PreviousValue} -> {e.CurrentValue}");
};
```

### 停止监听

```csharp
// 停止所有监听器
await listenerManager.StopListeners();

// 停止指定监听器
await listenerManager.StopListener("Level");
```

---

## 内存监听器

内存监听器用于监控单个内存地址，当值发生变化时触发事件。

### 监听器类型

- `StaticMemoryListener`：监听固定地址
- `DynamicMemoryListener`：每次检测时调用函数获取地址
- `UpdatableMemoryListener`：地址可按需或在进程变更时自动更新

### 直接创建监听器

```csharp
var staticListener   = MemoryListener.CreateStaticListener("HP", 0x007621B5, typeof(float));
var dynamicListener  = MemoryListener.CreateDynamicListener("Level", () => ResolveAddress(), typeof(int));
var updatableListener = MemoryListener.CreateUpdatableListener("Stamina", () => ResolveAddress(), typeof(float));
```

### 启动与停止

```csharp
await listener.StartListen(processHandle, TimeSpan.FromMilliseconds(100));
await listener.StopListen();
```

---

## MemoryValue

`MemoryValue` 是传递给 `ValueChanged` 事件处理程序的 record struct。

| 属性 | 类型 | 说明 |
|---|---|---|
| `CustomName` | `string` | 创建监听器时指定的标识名 |
| `PreviousValue` | `object?` | 上一次的值 |
| `CurrentValue` | `object?` | 当前值 |
| `PreviousValueGetSuccessful` | `bool` | 上一次读取是否成功 |
| `CurrentValueGetSuccessful` | `bool` | 本次读取是否成功 |

---

## ProcessType

| 值 | 说明 |
|---|---|
| `ProcessType.Bit32` | 32 位进程 |
| `ProcessType.Bit64` | 64 位进程（默认） |
