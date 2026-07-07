using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public RectTransform crosshairRoot;
    private PlayerController _pc;
    private CanvasGroup _canvasGroup;


    public void Initialize(PlayerController player)
    {
        _pc = player;
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void LateUpdate()
    {
        UpdateCrosshair();
    }

    public void UpdateCrosshair()
    {
        if (_pc == null || _canvasGroup == null) return;

        bool shouldShow = _pc.isAiming;
        _canvasGroup.alpha = shouldShow ? 1f : 0f;
    }
}
