using UnityEngine;

public class Chest : BaseContainer
{
    public int chestCapacity = 24;

    private InventoryData _chestData;

    public override string ActionName => "´ò¿ªÏä×Ó";

    private void Awake()
    {
        _chestData = new InventoryData(chestCapacity);
        _chestData.filter = null;
    }

    protected override void OpenContainer()
    {
        if (_chestData == null) return;
        isOpen = true;
        InventoryManager.Instance.container.OpenContainer(_chestData, this);
    }
}