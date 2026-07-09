using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementStatsUI : MonoBehaviour
{
    public ElementType elementType;

    public TMP_Text levelText;
    public Slider slider;
    public TMP_Text expText;
    public TMP_Text potText;

    public void RefreshUI(PlayerStats stats)
    {
        if (stats == null) return;

        if (stats.elementStats.TryGetValue(elementType, out var stat))
        {
            levelText.text = "Lv." + stat.lvl;

            float req = ElementStats.ExpReq(stat.lvl);
            float progress = Mathf.Clamp01(stat.exp / req);
            slider.value = progress;

            expText.text = $"{Mathf.RoundToInt(stat.exp)} / {Mathf.RoundToInt(req)}";
            potText.text = stat.pot.ToString("F1") + "%";
        }
    }
}