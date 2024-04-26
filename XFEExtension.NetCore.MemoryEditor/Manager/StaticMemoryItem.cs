namespace XFEExtension.NetCore.MemoryEditor.Manager;

public abstract class StaticMemoryItem(string name) : MemoryItem(name)
{
    public override void AddListener()
    {

    }

    public override void RemoveListener()
    {
        throw new NotImplementedException();
    }
}