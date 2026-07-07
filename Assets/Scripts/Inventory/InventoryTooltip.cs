using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using UnityEditor.UIElements;

public class InventoryTooltip : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI infoText;
    public int xOffset = 15;
    public int yOffset = -15;
    private RectTransform _rectTransform;


    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Display(InventoryItem item, Vector2 position)
    {
        titleText.text = item.runtimeData.ItemName;
        descriptionText.text = item.runtimeData.Desc;
        string extraInfo = $"物品类型：{item.runtimeData.Type.ToDisplayName()}";
        if (item.runtimeData is FoodItemRuntime food)
        {
            extraInfo += "\n\n食物效能\n" +
                         $"火元素潜能: {food.FirePot:F1}\n" +
                         $"水元素潜能: {food.WaterPot:F1}\n" +
                         $"草元素潜能: {food.GrassPot:F1}\n" +
                         $"风元素潜能: {food.WindPot:F1}\n" +
                         $"暗元素潜能: {food.DarkPot:F1}\n";
        }
        else if (item.runtimeData is MagicItemRuntime magic)
        {
            extraInfo += "\n\n法术属性\n" +
                         $"威力: <color=#FF5555>{magic.Damage:F1}</color>\n" +
                         $"魔耗: <color=#55FFFF>{magic.Cost:F1}</color>\n" +
                         $"弹速: <color=#FFFF55>{magic.Speed:F1}</color>\n" +
                         $"属性: {magic.Element.GetName()}";
        }

        infoText.text = extraInfo;

        gameObject.SetActive(true);

        UpdatePosition(position);
    }

    private void UpdatePosition(Vector2 mousePosition)
    {
        Vector2 finalPos = mousePosition + new Vector2(xOffset, -yOffset);

        float width = _rectTransform.rect.width;
        float height = _rectTransform.rect.height;

        float screenW = Screen.width;

        if (finalPos.x + width > screenW)
        {
            finalPos.x = mousePosition.x - width - xOffset; // 翻转到鼠标左侧
        }

        if (finalPos.y - height < 0)
        {
            finalPos.y = mousePosition.y + height - yOffset; // 翻转到鼠标上方
        }

        transform.position = finalPos;
    }

    public void Hide() => gameObject.SetActive(false);
}