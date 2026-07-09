using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementStatsPanel : BasePanel
{
    [SerializeField] private List<ElementStatsUI> elements = new List<ElementStatsUI>();

    public override void Open()
    {
        base.Open();
        RefreshAllElements();
    }

    public void RefreshAllElements()
    {
        if (PlayerController.Instance == null) return;
        foreach (var elementUI in elements)
        {
            if (elementUI != null)
            {
                elementUI.RefreshUI(PlayerController.Instance.stats);
            }
        }
    }
}
