using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // 引入 DoTween 命名空间
using UnityEngine.Rendering.Universal;

public class CircleController : MonoBehaviour
{
    private GameObject decalObject;

    public float duration = 1f;
    public Ease easeType = Ease.OutQuad;

    private DecalProjector decalProjector;
    private Material runtimeMaterial;
    private float currentProgress = 1f;

    private Tweener currentTween;
    private static readonly int ProgressID = Shader.PropertyToID("_Progress");

    void Awake()
    {
        decalObject = transform.Find("MagicCircle").gameObject;
        decalProjector = decalObject.GetComponent<DecalProjector>();
        runtimeMaterial = decalProjector.material;

        currentProgress = 1f;
        UpdatePropertyBlock();
        decalObject.SetActive(false);
    }

    public void ToggleState(bool show)
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }

        float targetValue = show ? 0f : 1f;

        if (show)
        {
            decalObject.SetActive(true);
        }

        currentTween = DOTween.To(() => currentProgress, x => currentProgress = x, targetValue, duration)
            .SetEase(easeType)
            .OnUpdate(() =>
            {
                UpdatePropertyBlock();
            })
            .OnComplete(() =>
            {
                if (!show)
                {
                    decalObject.SetActive(false);
                }
            });
    }

    private void UpdatePropertyBlock()
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