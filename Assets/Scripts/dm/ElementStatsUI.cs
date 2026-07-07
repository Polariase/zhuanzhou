using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementStatsUI : MonoBehaviour
{
    [Header("元素类型")]
    public ElementType elementType;

    [Header("UI组件")]
    public TMP_Text elementNameText;
    public TMP_Text levelText;
    public Image expFillImage;
    public TMP_Text expText;
    public TMP_Text potText;

    private PlayerStats stats;

    void Start()
    {
        // 从临时数据提供者获取数据
        if (TempStatsProvider.Instance == null)
        {
            Debug.LogError("[ElementStatsUI] 场景中没有 TempStatsProvider！请创建一个空物体挂载该脚本。");
            return;
        }

        stats = TempStatsProvider.Instance.stats;

        if (stats == null)
        {
            Debug.LogError("[ElementStatsUI] TempStatsProvider.stats 为空！");
            return;
        }

        elementNameText.text = GetElementName(elementType);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (stats == null) return;

        if (stats.elementStats.TryGetValue(elementType, out var elementStats))
        {
            levelText.text = "Lv." + elementStats.lvl;

            float req = ElementStats.ExpReq(elementStats.lvl);
            float progress = Mathf.Clamp01(elementStats.exp / req);
            expFillImage.fillAmount = progress;

            expText.text = $"{elementStats.exp.ToString("F1")} / {req}";
            potText.text = elementStats.pot.ToString("F1") + "%";
        }
    }

    private string GetElementName(ElementType type)
    {
        switch (type)
        {
            case ElementType.Wind: return "风";
            case ElementType.Water: return "水";
            case ElementType.Fire: return "火";
            case ElementType.Grass: return "木";
            case ElementType.Dark: return "恶";
            default: return type.ToString();
        }
    }
}