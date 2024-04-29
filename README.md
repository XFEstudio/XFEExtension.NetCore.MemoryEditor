# XFEExtension.NetCore.MemoryEditor

## 内存管理器

### 创建内存管理器（推荐使用可更新内存管理器）

```csharp
public static class Program
{
    public static UpdatableMemoryManager Manager { get; } = MemoryManager.CreateBuilder() //创建内存管理器的构建器
            .WithAutoReacquireProcess("ExampleGame") //当目标进程退出后，自动重新获取目标名称的进程
            .WithFindProcessWhenCreate() //创建管理器时，开始寻找目标名称的进程（如果前面设置了目标进程名称此处可以不用设置）
            .BuildUpdatableManager(
            MemoryItemBuilder.Create<int>("Level") //为每个地址添加一个名称
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x14A0, 0x0, 0x80, 0xE4, 0x0, 0x1EC) //内存地址的模块名称、基址和偏移部分
                             .WithListener(), //添加监听器
            MemoryItemBuilder.Create<float>("HealthPoint")
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x12E8, 0x0, 0x80, 0xE4, 0x0, 0x1E0)
                             .WithListener(),
            MemoryItemBuilder.Create<float>("Stamina")
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x14E0, 0x48, 0x10, 0x20, 0x50, 0x20, 0x1B0)
                             .WithListener());
    //推荐使用可更新内存管理器
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