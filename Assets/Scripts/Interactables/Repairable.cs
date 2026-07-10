using UnityEngine;

public class RepairableObject : MonoBehaviour, IInteractable
{
    [Header("修复所需材料数量")]
    [SerializeField] private int requiredWoodCount = 5;  // 需要的木材数量
    [SerializeField] private int requiredStoneCount = 3; // 需要的石头数量

    [Header("关联物体状态转换")]
    [SerializeField] private GameObject targetToActivate; // 修复成功后需要激活的物体（如：修好的桥、门）

    // 定义常量 ID 避免魔法数字
    private const int WOOD_ID = 1002;
    private const int STONE_ID = 1001;

    // 实现接口的属性：显示在交互UI上的文字
    public string actionName;
    public string ActionName => actionName;

    /// <summary>
    /// 响应交互的核心方法
    /// </summary>
    public void Interact()
    {
        // 1. 获取玩家的背包组件
        // （这里假设你的玩家单例是 PlayerController.Instance，你可以根据实际架构修改获取背包的方式）
        BackpackController backpack = InventoryManager.Instance.backpack;

        if (backpack == null)
        {
            Debug.LogError("RepairableObject: 场景中未找到玩家的 BackpackController 组建！");
            return;
        }

        InventoryData inventoryData = backpack.GetData();
        if (inventoryData == null) return;

        // 2. 检查玩家背包中的材料总数是否足够
        int currentWood = GetItemTotalCount(inventoryData, WOOD_ID);
        int currentStone = GetItemTotalCount(inventoryData, STONE_ID);

        if (currentWood >= requiredWoodCount && currentStone >= requiredStoneCount)
        {
            // 3. 材料足够：扣除对应的材料
            DeductItem(inventoryData, WOOD_ID, requiredWoodCount);
            DeductItem(inventoryData, STONE_ID, requiredStoneCount);

            // 4. 执行状态转换
            ExecuteRepairSuccess();
        }
        else
        {
            // 5. 材料不够：可以在这里触发 UI 提示，或者直接打印 Log
            Debug.Log($"材料不足！修复需要：木材 {requiredWoodCount} (当前 {currentWood})，石头 {requiredStoneCount} (当前 {currentStone})");
        }
    }

    /// <summary>
    /// 计算指定 ID 的物品在背包里的总数量（遍历所有格子并累加）
    /// </summary>
    private int GetItemTotalCount(InventoryData data, int itemID)
    {
        int total = 0;
        for (int i = 0; i < data.CurrentCapacity; i++)
        {
            InventoryItem item = data.GetItem(i);
            // 检查格子不为空，且ItemID匹配
            if (item != null && item.runtimeData != null && item.runtimeData.ItemID == itemID)
            {
                total += item.count;
            }
        }
        return total;
    }

    /// <summary>
    /// 从背包中扣除指定数量的物品
    /// </summary>
    private void DeductItem(InventoryData data, int itemID, int amountToRemove)
    {
        int remainingToDeduct = amountToRemove;

        for (int i = 0; i < data.CurrentCapacity; i++)
        {
            InventoryItem item = data.GetItem(i);
            if (item != null && item.runtimeData != null && item.runtimeData.ItemID == itemID)
            {
                if (item.count >= remainingToDeduct)
                {
                    // 当前格子的数量大等于还需要扣除的数量，直接扣完并结束循环
                    data.RemoveItem(i, remainingToDeduct);
                    break;
                }
                else
                {
                    // 当前格子不够扣，把当前格子清空，并减少“还需要扣除的数量”，继续找下一个格子
                    remainingToDeduct -= item.count;
                    data.RemoveItem(i, item.count);
                }
            }
        }
    }

    /// <summary>
    /// 成功修复后的表现：切换物体显示状态
    /// </summary>
    private void ExecuteRepairSuccess()
    {
        Debug.Log("修复成功！");

        // 激活目标物体
        if (targetToActivate != null)
        {
            targetToActivate.SetActive(true);
        }

        // 隐藏当前交互物体自身
        gameObject.SetActive(false);
    }
}