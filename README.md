# XFEExtension.NetCore.MemoryEditor

[![NuGet Version](https://img.shields.io/nuget/v/XFEExtension.NetCore.MemoryEditor.svg)](https://www.nuget.org/packages/XFEExtension.NetCore.MemoryEditor/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/XFEExtension.NetCore.MemoryEditor.svg)](https://www.nuget.org/packages/XFEExtension.NetCore.MemoryEditor/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

English | [简体中文](./README.zh-CN.md)

A .NET memory read/write library with support for pointer-based address resolution and a built-in memory manager.

## Installation

```bash
dotnet add package XFEExtension.NetCore.MemoryEditor
```

---

## Memory Manager

The Memory Manager is the recommended way to use this library. It wraps the Memory Editor and Memory Listener Manager into a convenient builder-pattern API.

### Manager Types

- **Static Memory Manager** (`StaticMemoryManager`): Stores fixed addresses that do not change between process instances.
- **Dynamic Memory Manager** (`DynamicMemoryManager`): Stores address-resolution functions that are called each time the listener checks the value.
- **Updatable Memory Manager** (`UpdatableMemoryManager`): Stores a static address along with an update function that is called automatically when the target process changes, or manually by the user.

### Architecture

Each Memory Manager contains one `MemoryEditor`, which in turn contains a `MemoryListenerManager`.

### Creating a Memory Manager (Dynamic Manager recommended)

```csharp
public static class Program
{
    public static DynamicMemoryManager Manager { get; } = MemoryManager.CreateBuilder()
            .WithAutoReacquireProcess("ExampleGame")   // Automatically reacquire the process when it exits
            .WithFindProcessWhenCreate()                // Start searching for the process on creation
            .BuildDynamicManager(
            MemoryItemBuilder.Create<int>("Level")
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x14A0, 0x0, 0x80, 0xE4, 0x0, 0x1EC)
                             .WithListener(),
            MemoryItemBuilder.Create<float>("HealthPoint")
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x12E8, 0x0, 0x80, 0xE4, 0x0, 0x1E0)
                             .WithListener(),
            MemoryItemBuilder.Create<float>("Stamina")
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x14E0, 0x48, 0x10, 0x20, 0x50, 0x20, 0x1B0)
                             .WithListener());
}
```

### Using the Memory Manager

```csharp
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        Program.Manager.ValueChanged += Manager_ValueChanged;
    }

    private void Manager_ValueChanged(XFEExtension.NetCore.MemoryEditor.Manager.MemoryItem sender, MemoryValue e)
    {
        Trace.WriteLine($"Name: {e.CustomName}  Address: {sender:X}  Previous: {e.PreviousValue}  Current: {e.CurrentValue}");
        switch (e.CustomName)
        {
            case "Level":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(valueToWrite))
                        Trace.WriteLine("Level: write failed");
                }
                break;
            case "HealthPoint":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(valueToWrite))
                        Trace.WriteLine("HealthPoint: write failed");
                }
                break;
            case "Stamina":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(valueToWrite))
                        Trace.WriteLine("Stamina: write failed");
                }
                break;
            default:
                break;
        }
    }
}
```

### Reading and Writing Memory Items Directly

```csharp
// Read a value
int level = Program.Manager["Level"].Read<int>();

// Write a value
bool success = Program.Manager["Level"].Write<int>(99);
```

---

## Memory Editor

The Memory Editor provides low-level memory read/write operations. It can be used standalone or is used internally by the Memory Manager.

### Creating a Memory Editor

```csharp
// Create without a process
var memoryEditor = new MemoryEditor();
memoryEditor.CurrentProcess = Process.GetProcessesByName("YourProcessName").First();

// Create directly with a process name
var memoryEditor = new MemoryEditor("YourProcessName");

// Create with a Process object
var memoryEditor = new MemoryEditor(process);

// Create with a process ID
var memoryEditor = new MemoryEditor(processId);
```

### Instance Read/Write

```csharp
bool writeSuccess = memoryEditor.WriteMemory(0x007621B5, 1234);
bool readSuccess = memoryEditor.ReadMemory<int>(0x007621B5, out var result);
Console.WriteLine(result);
```

### Static Read/Write (without an instance)

```csharp
nint processHandle = 0x00000a8c;
bool writeSuccess = MemoryEditor.WriteMemory(processHandle, 0x007621B5, 1234);
bool readSuccess = MemoryEditor.ReadMemory<int>(processHandle, 0x007621B5, out var result);
```

### Resolving Pointer Addresses

```csharp
// Instance method (using module name)
nint address = memoryEditor.ResolvePointerAddress("game.dll", 0x0072A200, 0x14A0, 0x0, 0x1EC);

// Static method
nint address = MemoryEditor.ResolvePointerAddress(process, "game.dll", 0x0072A200, ProcessType.Bit64, 0x14A0, 0x0, 0x1EC);
```

---

## Memory Listener Manager

The Memory Listener Manager manages collections of memory listeners. It is used internally by the Memory Editor but can also be used standalone.

### Architecture

Contains a `Dictionary<string, MemoryListener>` that manages all registered listeners.

### Creating a Memory Listener Manager

```csharp
// Create with a process name
var listenerManager = new MemoryListenerManager("YourProcessName");

// Create empty and set process later
var listenerManager = new MemoryListenerManager();
listenerManager.CurrentProcess = Process.GetProcessesByName("YourProcessName").First();
```

### Adding Listeners

```csharp
// Add a static listener
var listener = listenerManager.AddStaticListener("HP", 0x007621B5, TimeSpan.FromMilliseconds(100), typeof(float), startListen: true);

// Add a dynamic listener
var listener = listenerManager.AddDynamicListener("Level", () => ResolveAddress(), TimeSpan.FromMilliseconds(100), typeof(int), startListen: true);
```

### Handling Events

```csharp
listenerManager.ValueChanged += (sender, e) =>
{
    Console.WriteLine($"{e.CustomName}: {e.PreviousValue} -> {e.CurrentValue}");
};
```

### Stopping Listeners

```csharp
// Stop all listeners
await listenerManager.StopListeners();

// Stop a specific listener
await listenerManager.StopListener("Level");
```

---

## Memory Listener

Individual memory listeners monitor a memory address and fire an event when the value changes.

### Listener Types

- `StaticMemoryListener` – monitors a fixed address
- `DynamicMemoryListener` – calls a function each poll to get the address
- `UpdatableMemoryListener` – address is updated on demand or when the process changes

### Creating Listeners Directly

```csharp
var staticListener    = MemoryListener.CreateStaticListener("HP", 0x007621B5, typeof(float));
var dynamicListener   = MemoryListener.CreateDynamicListener("Level", () => ResolveAddress(), typeof(int));
var updatableListener = MemoryListener.CreateUpdatableListener("Stamina", () => ResolveAddress(), typeof(float));
```

### Starting and Stopping

```csharp
await listener.StartListen(processHandle, TimeSpan.FromMilliseconds(100));
await listener.StopListen();
```

---

## MemoryValue

`MemoryValue` is a record struct passed to `ValueChanged` event handlers.

| Property | Type | Description |
|---|---|---|
| `CustomName` | `string` | The identifier name given when the listener was created |
| `PreviousValue` | `object?` | The previous value |
| `CurrentValue` | `object?` | The current value |
| `PreviousValueGetSuccessful` | `bool` | Whether the previous read succeeded |
| `CurrentValueGetSuccessful` | `bool` | Whether the current read succeeded |

---

## ProcessType

| Value | Description |
|---|---|
| `ProcessType.Bit32` | 32-bit process |
| `ProcessType.Bit64` | 64-bit process (default) |