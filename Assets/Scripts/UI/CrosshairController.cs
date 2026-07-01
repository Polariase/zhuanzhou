using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public RectTransform crosshairRoot;
    public Image dotImage;
    public Image[] linesImage;
    public RectTransform lines;
    public RectTransform[] linesTransform;
    public Camera mainCamera;
    public PlayerController pc;
    public Gradient spreadGradient;
    public float rotateSpeed = 300f;

    private float _minSpread = 2f;
    private float _maxSpread = 100f;
    private float _smoothTime = 0.1f;
    private float _currentRotationZ = 0f;
    private float _currentSpreadVelocity;
    private float _visualSpread;
    private PlayerInput _input;

    public void Initialize(PlayerController player)
    {
        pc = player;
        if (pc == null) return;

        mainCamera = Camera.main;
        _input = pc.GetComponent<PlayerInput>();
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
        linesTransform[0].anchoredPosition = new Vector2(0, spread);
        linesTransform[1].anchoredPosition = new Vector2(spread, 0);
        linesTransform[2].anchoredPosition = new Vector2(-spread, 0);
        linesTransform[3].anchoredPosition = new Vector2(0, -spread);
    }

    private void UpdateCrosshairColor(float percent)
    {

    }
}
