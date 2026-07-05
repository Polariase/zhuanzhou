using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class CircleController : MonoBehaviour
{
    private PlayerController _pc;
    private GameObject decalObject;
    private ParticleSystem circlePs;

    public float duration = 1f;
    public Ease easeType = Ease.OutQuad;

    private DecalProjector decalProjector;
    private Material runtimeMaterial;
    private float currentProgress = 1f;

    private Tweener currentTween;
    private static readonly int ProgressID = Shader.PropertyToID("_Progress");
    private static readonly int ColorFastID = Shader.PropertyToID("_Color_Fast");
    private static readonly int ColorSlowID = Shader.PropertyToID("_Color_Slow");
    private static readonly int ColorBurnID = Shader.PropertyToID("_Color_Burn");

    public readonly Color basicFire = new Color32(133, 16, 22,255);
    public readonly Color basicWater = new Color32(12, 70, 101, 255);
    public readonly Color basicWind = new Color32(75, 88, 86, 255);
    public readonly Color basicDark = new Color32(113, 7, 87, 255);
    public readonly Color basicGrass = new Color32(23, 68, 18, 255);

    void Awake()
    {
        _pc = GetComponent<PlayerController>();
        decalObject = transform.Find("MagicCircle").gameObject;
        circlePs = GetComponentInChildren<ParticleSystem>();
        decalProjector = decalObject.GetComponent<DecalProjector>();
        runtimeMaterial = decalProjector.material;

        currentProgress = 1f;
        UpdateProperty();
        decalObject.SetActive(false);
    }

    public void ChangeParticleColors(Color minColor, Color maxColor)
    {
        var mainModule = circlePs.main;
        ParticleSystem.MinMaxGradient gradient = new ParticleSystem.MinMaxGradient();
        gradient.mode = ParticleSystemGradientMode.TwoColors;
        gradient.colorMin = minColor;
        gradient.colorMax = maxColor;
        mainModule.startColor = gradient;
    }

    private Color32 GetBasicColor(ElementType type)
    {
        return type switch
        {
            ElementType.Fire => basicFire,
            ElementType.Water => basicWater,
            ElementType.Wind => basicWind,
            ElementType.Dark => basicDark,
            ElementType.Grass => basicGrass,
            _ => Color.black
        };
    }

    public void ToggleState(bool show)
    {
        List<ElementStats> sorted = _pc.stats.GetSortedElementStats();
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }

        float targetValue = show ? 0f : 1f;

        if (show)
        {
            Color fast = GetBasicColor(sorted[0].type);
            Color slow = GetBasicColor(sorted[1].type);
            Color burn = GetBasicColor(sorted[2].type);
            Color pFast = sorted[0].type.GetColor();
            Color pSlow = sorted[1].type.GetColor();
            runtimeMaterial.SetColor(ColorFastID, fast);
            runtimeMaterial.SetColor(ColorSlowID, slow);
            runtimeMaterial.SetColor(ColorBurnID, burn);
            ChangeParticleColors(pFast, pSlow);
            decalObject.SetActive(true);
            circlePs.Play();
        }
        else
        {
            var emission = circlePs.emission;
            emission.enabled = true;
        }

        currentTween = DOTween.To(() => currentProgress, x => currentProgress = x, targetValue, duration)
            .SetEase(easeType)
            .OnUpdate(() =>
            {
                UpdateProperty();
            })
            .OnComplete(() =>
            {
                if (!show)
                {
                    decalObject.SetActive(false);
                    circlePs.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            });
    }

    private void UpdateProperty()
    {
        runtimeMaterial.SetFloat(ProgressID, currentProgress);
    }

    private void OnDestroy()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }
    }
}