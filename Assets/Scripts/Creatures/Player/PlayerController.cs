using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance
    {
        get; private set;
    }

    public PlayerStats stats;

    private PlayerInput _input;
    private InputAction _look;
    private InputAction _move;
    private InputAction _run;

    private Vector2 _inputDir;
    private CharacterController _cc;
    public Transform modelRoot;
    private Animator _anim;

    public bool isGrounded;
    public bool isMoving;
    public bool isJumping;
    public bool isAiming;
    public bool isRunning;

    [Header("Cinemachine")]
    public GameObject camTarget;
    public float topClamp = 45f;
    public float botClamp = -30f;
    public float camAngleOverride = 0.0f;
    private const float _threshold = 0.01f;
    public float xSens = 16f;
    public float ySens = 5f;
    public bool lockCam = false;

    private float _targetPitch;
    private float _targetYaw;

    private Camera _mainCamera;

    [Header("Movement Settings")]
    public float moveSpeed = 2.5f;
    public float runSpeedScale = 2f;
    public float rotationSpeed = 15f;
    public float gravity = -15f;

    public float animDampTime = 0.1f;

    private Vector3 _velocity;



    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _input = GetComponent<PlayerInput>();

        _look = _input.actions["Look"];

        _move = _input.actions["Move"];

        _cc = GetComponent<CharacterController>();
        _run = _input.actions["Run"];

        _cc = GetComponent<CharacterController>();
        _anim = GetComponent<Animator>();

        modelRoot = transform.Find("ModelRoot");
    }



    void Start()
    {
        if (Camera.main != null)
        {
            _mainCamera = Camera.main;
        }
    }



    void Update()
    {
        _inputDir = _move.ReadValue<Vector2>();
        isMoving = _inputDir.sqrMagnitude >= _threshold;
        isRunning = _run.IsPressed();
        isGrounded = _cc.isGrounded;

        HandleMovement();
        HandleAnimation();
    }

    private void LateUpdate()
    {
        HandleRotation();
    }



    private bool IsCurrentDeviceMouse => _input.currentControlScheme == "KeyboardMouse";



    private float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void HandleMovement()
    {
        if (isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        float currentSpeed = moveSpeed * (isRunning ? runSpeedScale : 1f);
        Vector3 moveTargetDir = Vector3.zero;

        if (isMoving && _mainCamera != null)
        {
            Vector3 camForward = _mainCamera.transform.forward;
            Vector3 camRight = _mainCamera.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveTargetDir = camForward * _inputDir.y + camRight * _inputDir.x;
            moveTargetDir.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(moveTargetDir);
            modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        _cc.Move(moveTargetDir * (currentSpeed * Time.deltaTime));

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    private void HandleAnimation()
    {
        if (_anim == null) return;

        float targetAnimX = 0f;
        float targetAnimY = 0f;
        if (isMoving)
        {
            targetAnimY = 0.5f * (isRunning ? 2f : 1f);
        }

        _anim.SetFloat("X", targetAnimX, animDampTime, Time.deltaTime);
        _anim.SetFloat("Y", targetAnimY, animDampTime, Time.deltaTime);

        // 以后你可以在这里轻松扩展其他动画：
        // _anim.SetBool("IsGrounded", isGrounded);
        // _anim.SetBool("IsJumping", isJumping);
    }

    private void HandleRotation()
    {
        Vector2 lookValue = _look.ReadValue<Vector2>();
        if (lookValue.sqrMagnitude >= _threshold && !lockCam)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _targetYaw += lookValue.x * deltaTimeMultiplier * xSens;
            _targetPitch -= lookValue.y * deltaTimeMultiplier * ySens;
        }

        _targetPitch = ClampAngle(_targetPitch, botClamp, topClamp);
        _targetYaw = ClampAngle(_targetYaw, -360f, 360f);

        camTarget.transform.localRotation = Quaternion.Euler(_targetPitch + camAngleOverride, _targetYaw, 0.0f);
    }

}