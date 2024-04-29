# XFEExtension.NetCore.MemoryEditor

## �����ڴ������
```csharp
public static class Program
{
    public static UpdatableMemoryManager Manager { get; } = MemoryManager.CreateBuilder() //�����ڴ�������Ĺ�����
            .WithAutoReacquireProcess("ExampleGame") //��Ŀ������˳����Զ����»�ȡĿ�����ƵĽ���
            .WithFindProcessWhenCreate() //����������ʱ����ʼѰ��Ŀ�����ƵĽ��̣����ǰ��������Ŀ��������ƴ˴����Բ������ã�
            .BuildUpdatableManager(
            MemoryItemBuilder.Create<int>("Level") //Ϊÿ����ַ���һ������
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x14A0, 0x0, 0x80, 0xE4, 0x0, 0x1EC) //�ڴ��ַ��ģ�����ơ���ַ��ƫ�Ʋ���
                             .WithListener(), //��Ӽ�����
            MemoryItemBuilder.Create<float>("HealthPoint")
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x12E8, 0x0, 0x80, 0xE4, 0x0, 0x1E0)
                             .WithListener(),
            MemoryItemBuilder.Create<float>("Stamina")
                             .WithResolvePointer("xxx-xxx-xx.dll", 0x0072A200, 0x14E0, 0x48, 0x10, 0x20, 0x50, 0x20, 0x1B0)
                             .WithListener());
    //�Ƽ�ʹ�ÿɸ����ڴ������
}

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        Program.Manager.ValueChanged += Manager_ValueChanged;//�����ڴ�ֵ�ı��¼�
    }

    private void Manager_ValueChanged(XFEExtension.NetCore.MemoryEditor.Manager.MemoryItem sender, MemoryValue e)
    {
        Trace.WriteLine($"���ƣ�{e.CustomName} ��ַ��{sender:X}\t�Ƿ��ȡ��ֵ  �ϴΣ�{e.PreviousValueGetSuccessful}  ��Σ�{e.CurrentValueGetSuccessful}  ֵ�ӣ�{e.PreviousValue}  ���Ϊ��{e.CurrentValue}");
        switch (e.CustomName)
        {
            case "Level":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(��д��ֵ))
                        Trace.WriteLine($"Level��д��ʧ��");
                }
                break;
            case "HealthPoint":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(��д��ֵ))
                        Trace.WriteLine("HealthPoint��д��ʧ��");
                }
                break;
            case "Stamina":
                if (e.CurrentValueGetSuccessful)
                {
                    if (!sender.Write(��д��ֵ))
                        Trace.WriteLine("Stamina��д��ʧ��");
                }
                break;
            default:
                break;
        }
    }
}
```