using UnityEngine;

public class BackpackController : BaseInventoryController
{
    [SerializeField] private int _initialCapacity = 24;
    [SerializeField] private BackpackView _backpackView;

    private void Start()
    {
        if (inventoryData == null || inventoryData.CurrentCapacity == 0)
        {
            inventoryData = new(_initialCapacity);
        }

        if (_backpackView != null)
        {
            _backpackView.Initialize(inventoryData, this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            ArcanaSeg arcana = DataManager.Instance.GetMagicSeg<ArcanaSeg>("mofajian");
            ModifierSeg mod = DataManager.Instance.GetMagicSeg<ModifierSeg>("weixing");
            ElementSeg ele = DataManager.Instance.GetMagicSeg<ElementSeg>("huohua");
            EmitterSeg em = DataManager.Instance.GetMagicSeg<EmitterSeg>("changgui");
            MagicItemRuntime mr = new(arcana, ele, em, mod);
            inventoryData.AddItem(new InventoryItem(mr, 1));
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            ArcanaSeg arcana = DataManager.Instance.GetMagicSeg<ArcanaSeg>("molibiaoqiang");
            ModifierSeg mod = DataManager.Instance.GetMagicSeg<ModifierSeg>("juxing");
            ElementSeg ele = DataManager.Instance.GetMagicSeg<ElementSeg>("jiliu");
            EmitterSeg em = DataManager.Instance.GetMagicSeg<EmitterSeg>("changgui");
            MagicItemRuntime mr = new(arcana, ele, em, mod);
            inventoryData.AddItem(new InventoryItem(mr, 1));
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            ArcanaSeg arcana = DataManager.Instance.GetMagicSeg<ArcanaSeg>("molizhendangbo");
            ModifierSeg mod = DataManager.Instance.GetMagicSeg<ModifierSeg>("gaosu");
            ElementSeg ele = DataManager.Instance.GetMagicSeg<ElementSeg>("maosheng");
            EmitterSeg em = DataManager.Instance.GetMagicSeg<EmitterSeg>("changgui");
            MagicItemRuntime mr = new(arcana, ele, em, mod);
            inventoryData.AddItem(new InventoryItem(mr, 1));
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            ArcanaSeg arcana = DataManager.Instance.GetMagicSeg<ArcanaSeg>("mofajian");
            ElementSeg ele = DataManager.Instance.GetMagicSeg<ElementSeg>("weifeng");
            EmitterSeg em = DataManager.Instance.GetMagicSeg<EmitterSeg>("changgui");
            MagicItemRuntime mr = new(arcana, ele, em, null);
            inventoryData.AddItem(new InventoryItem(mr, 1));
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            ArcanaSeg arcana = DataManager.Instance.GetMagicSeg<ArcanaSeg>("molibiaoqiang");
            ElementSeg ele = DataManager.Instance.GetMagicSeg<ElementSeg>("zhongyan");
            EmitterSeg em = DataManager.Instance.GetMagicSeg<EmitterSeg>("lianfa");
            ModifierSeg mod = DataManager.Instance.GetMagicSeg<ModifierSeg>("weixing");
            MagicItemRuntime mr = new(arcana, ele, em, mod);
            inventoryData.AddItem(new InventoryItem(mr, 1));
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            ArcanaSeg arcana = DataManager.Instance.GetMagicSeg<ArcanaSeg>("molibiaoqiang");
            ElementSeg ele = DataManager.Instance.GetMagicSeg<ElementSeg>("zhongyan");
            EmitterSeg em = DataManager.Instance.GetMagicSeg<EmitterSeg>("lianfa");
            ModifierSeg mod = DataManager.Instance.GetMagicSeg<ModifierSeg>("juxing");
            MagicItemRuntime mr = new(arcana, ele, em, mod);
            inventoryData.AddItem(new InventoryItem(mr, 1));
        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            inventoryData.AddItem(new(DataManager.Instance.GetItemData(1001),10));
            inventoryData.AddItem(new(DataManager.Instance.GetItemData(1002), 10));
        }
    }
}