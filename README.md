# XFEExtension.NetCore.MemoryEditor

## 内存管理器

### 内存管理器类型说明

内存管理器是本开源包中力推的类，使用内存管理器可以方便快捷的开发出基于指定偏移地址的内存修改工具

### 内存管理器类型

- 静态内存管理器`StaticMemoryManager`：在这个内存管理器中，存放静态地址，该地址不会随着进程的不同而变动，是始终如一的
- 动态内存管理器`DynamicMemoryManager`：在这个内存管理器中，存放的是地址的获取方法，也就是动态的，每次监听器检测地址的时候都会调用一遍该方法来获取地址
- 可更新内存管理器`UpdatableMemoryManager`：如其名，在此内存管理器中，存放静态地址和该地址对应的更新方法，管理器默认会在目标进程更改时调用一次该方法，当然用户也可以手动调用该方法来更新

### 内存管理器结构

内存管理器里面包含了一个内存编辑器，而内存编辑器里面则是内置了一个内存监听器管理器

### 创建内存管理器（推荐使用动态内存管理器或可更新内存管理器）

```csharp
public static class Program
{
    public static DynamicMemoryManager Manager { get; } = MemoryManager.CreateBuilder() //创建内存管理器的构建器
            .WithAutoReacquireProcess("ExampleGame") //当目标进程退出后，自动重新获取目标名称的进程
            .WithFindProcessWhenCreate() //创建管理器时，开始寻找目标名称的进程（如果前面设置了目标进程名称此处可以不用设置）
            .BuildDynamicManager(
            MemoryItemBuilder.Create<int>("Level") //为每个地址添加一个名称
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x14A0, 0x0, 0x80, 0xE4, 0x0, 0x1EC) //内存地址的模块名称、基址和偏移部分
                             .WithListener(), //添加监听器
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
        Program.Manager.ValueChanged += Manager_ValueChanged;//订阅内存值改变事件
    }

    private void Manager_ValueChanged(XFEExtension.NetCore.MemoryEditor.Manager.MemoryItem sender, MemoryValue e)
    {
        Trace.WriteLine($"名称：{e.CustomName} 地址：{sender:X}\t是否读取到值  上次：{e.PreviousValueGetSuccessful}  这次：{e.CurrentValueGetSuccessful}  值从：{e.PreviousValue}  变更为：{e.CurrentValue}");
        switch (e.CustomName)
        {
            case "Level":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(待写入值))
                        Trace.WriteLine($"Level：写入失败");
                }
                break;
            case "HealthPoint":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(待写入值))
                        Trace.WriteLine("HealthPoint：写入失败");
                }
                break;
            case "Stamina":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(待写入值))
                        Trace.WriteLine("Stamina：写入失败");
                }
                break;
            default:
                break;
        }
    }
}
```

---

## 内存修改器

### 内存修改器类型说明

内存管理器可以通过类名.来调用静态方法，也可以创建对象，来针对某个进程进行内存编辑

### 内存修改器层次结构

内存管理器的实例里面内置一个内存监听器管理器

### 创建内存修改器

```csharp
var memoryEditor = new MemoryEditor();
memoryEditor.CurrentProcess = Process.GetProcessesFromName("Your Process Name");
var writeSuccess = memoryEditor.WriteMemory(0x007621B5, 1234);
var readSuccess = memoryEditor.ReadMemory<int>(0x007621B5, out var result);
Console.WriteLine(resule);
```

### 使用内存修改器（不创建实例）

```csharp
var writeSuccess = MemoryEditor.WriteMemory(0x00000a8c, 0x007621B5, 1234); //通过进程句柄0x00000a8c向地址0x007621B5中写入值1234
var readSuccess = MemoryEditor.ReadMemory<int>(0x00000a8c, 0x007621B5, out var result); //通过进程句柄0x00000a8c读取指定数据类型为int的地址0x007621B5中的值
```

---

## 内存监听器管理器

### 内存监听器管理器说明

推荐使用内存管理器，内存监听器管理器实际是为内存管理器服务的，当然也可以单独使用

### 内存监听器管理器层次结构

主要有一个Dictionary对象，该对象是对所有已创建监听器的管理