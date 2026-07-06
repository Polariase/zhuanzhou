using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public RectTransform crosshairRoot;
    public Camera mainCamera;
    private PlayerController _pc;

    private PlayerInput _input;

    public void Initialize(PlayerController player)
    {
        _pc = player;
        if (_pc == null) return;

        mainCamera = Camera.main;
        _input = _pc.GetComponent<PlayerInput>();
    }

    private void LateUpdate()
    {
        UpdateCrosshair();
    }

    public void UpdateCrosshair()
    {

    }

    private void ApplySpreadToLines(float spread)
    {

    }
}
