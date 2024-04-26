namespace XFEExtension.NetCore.MemoryEditor.Manager;

public abstract class StaticMemoryItem(string name) : MemoryItem(name)
{
    public override void AddListener()
    {
        if (HasListener)
            throw new InvalidOperationException("已存在监听器，无法重复添加");
        HasListener = true;
        memoryListener.StartListen
    }

    public override void RemoveListener()
    {
        throw new NotImplementedException();
    }
    public void UpdateStaticAddress()
    {

    }
}