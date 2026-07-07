using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotHandler : MonoBehaviour
{
    public string itemType;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn == null) btn = GetComponentInChildren<Button>();

        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnSlotClick);
            Debug.Log($"[HotbarSlotHandler] {gameObject.name} 绑定成功！");
        }
        else
        {
            Debug.LogWarning($"[HotbarSlotHandler] {gameObject.name} 未找到 Button");
        }
    }

    void OnSlotClick()
    {
        Debug.Log($"[Hotbar] 点击槽位：{itemType}");
        if (PlacementManager.Instance == null)
        {
            Debug.LogError("PlacementManager 实例不存在！");
            return;
        }

        if (!PlacementManager.Instance.IsPlacing)
            PlacementManager.Instance.StartPlacement(itemType);
        else
        {
            PlacementManager.Instance.CancelPlacement();
            PlacementManager.Instance.StartPlacement(itemType);
        }
    }
}