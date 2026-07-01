using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Image hpFill;
    public TextMeshProUGUI hpText;
    public Image overloadFill;
    public TextMeshProUGUI overloadText;
    public Gradient overloadGradient;
    public HotbarController hotbar;

    private PlayerStats _stats;

    public void Initialize(PlayerController pc)
    {
        Cleanup();
        _stats = pc.stats;
        if (_stats == null) return;
        _stats.OnHpChanged += OnHpChanged;
        _stats.OnLoadChanged += OnLoadChanged;
        OnHpChanged(_stats.hp, _stats.maxHp);
        OnLoadChanged(_stats.currentLoad, _stats.maxLoad);
        hotbar.Initialize(pc);
    }

    public void Cleanup()
    {
        if (_stats != null)
        {
            _stats.OnHpChanged -= OnHpChanged;
            _stats.OnLoadChanged -= OnLoadChanged;
            _stats = null;
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void OnHpChanged(int currentHp, int maxHp)
    {
        if (maxHp <= 0) return;

        float pct = Mathf.Clamp01(currentHp / (float)maxHp);
        hpFill.fillAmount = pct;

        hpText.SetText("{0}%", Mathf.RoundToInt(pct * 100f));
    }

    private void OnLoadChanged(float currentLoad, float maxLoad)
    {
        if (maxLoad <= 0) return;

        float pct = Mathf.Clamp01(currentLoad / maxLoad);
        overloadFill.fillAmount = pct;

        overloadText.SetText("{0}%", Mathf.RoundToInt(pct * 100f));

        Color targetColor;
        if (_stats != null && _stats.overloaded)
        {
            targetColor = overloadGradient.Evaluate(1f);
        }
        else
        {
            targetColor = overloadGradient.Evaluate(pct);
        }
        targetColor.a = 0.6f;
        overloadFill.color = targetColor;
    }
}
