using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

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
    private InputAction _jump;
    private InputAction _chant;
    private InputAction _cancel;
    private InputAction _complete;
    private InputAction _backspace;

    private Vector2 _inputDir;
    private CharacterController _cc;
    public Transform modelRoot;
    private CircleController _circle;
    private Animator _anim;

    public bool isGrounded;
    public bool isMoving;
    public bool isAiming;
    public bool isRunning;
    public bool isChanting;

    public bool chantStart;
    public bool chantRecovery;

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

    private Camera _cam;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float runSpeedScale = 2f;
    public float airSpeedScale = 2f;
    public float rotationSpeed = 15f;
    public float gravity = -15f;
    public float jumpHeight = 3f;
    private bool _jumpRequested;
    public float animDampTime = 0.1f;

    public float groundedCheckDelay = 0.75f;
    private float _groundedTimer;
    private bool _animLandedState = true;

    private Vector3 _velocity;

    public float jumpCost = 25f;
    public float runCost = 10f;

    public bool CanAct => !chantStart && !chantRecovery;
    public bool CanJump => isGrounded && stats.stamina >= jumpCost && !isChanting && CanAct;
    public bool CanChant => isGrounded && !isChanting && CanAct;
    public bool CanMove => !isChanting && CanAct;
    public bool CanRun => stats.stamina > 0f && isGrounded;

    public bool CanRecHp => true;
    public bool CanRecMana=> true;
    public bool CanRecSt => isGrounded;


    public float SpeedScale()
    {
        float scale = 1f;
        if (isRunning && isGrounded)
            scale *= runSpeedScale;
        else if (!isGrounded)
            scale *= airSpeedScale;

        return scale;
    }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _groundedTimer = groundedCheckDelay;

        _input = GetComponent<PlayerInput>();
        _cc = GetComponent<CharacterController>();
        _anim = GetComponent<Animator>();
        _circle = GetComponent<CircleController>();

        _look = _input.actions["Look"];
        _move = _input.actions["Move"];
        _run = _input.actions["Run"];
        _jump = _input.actions["Jump"];
        _chant = _input.actions["Chant"];
        _cancel = _input.actions["Cancel"];
        _complete = _input.actions["Complete"];
        _backspace = _input.actions["Backspace"];

        modelRoot = transform.Find("ModelRoot");
    }

    void Update()
    {
        Recovery();

        _inputDir = _move.ReadValue<Vector2>();
        isMoving = (_inputDir.sqrMagnitude >= _threshold) && CanMove;

        isGrounded = _cc.isGrounded;
        if (isGrounded)
        {
            _groundedTimer = groundedCheckDelay;
            _animLandedState = true;
        }
        else
        {
            _groundedTimer -= Time.deltaTime;
            if (_groundedTimer <= 0)
            {
                _animLandedState = false;
            }
        }

        HandleMovement();
        HandleAnimation();
    }

    private void LateUpdate()
    {
        HandleRotation();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        Cleanup();
    }

    private void BindActions()
    {
        _jump.performed += OnJumpAction;
        _chant.performed += OnChantAction;
        _cancel.performed += OnAbortAction;
    }

    private void UnbindActions()
    {
        _jump.performed -= OnJumpAction;
        _chant.performed -= OnChantAction;
        _cancel.performed -= OnAbortAction;
    }

    public void Initialize(PlayerStats st, CinemachineVirtualCamera cam)
    {
        Cleanup();
        stats = st;

        if (cam != null)
        {
            cam.Follow = camTarget.transform;
        }

        _cam = Camera.main;

        if (_input != null)
        {
            foreach (var item in _input.actions.actionMaps)
            {
                item.Disable();
            }
            _input.SwitchCurrentActionMap("Player");
            _input.currentActionMap.Enable();

            BindActions();
        }
    }

    public void Cleanup()
    {
        if (_input != null)
        {
            UnbindActions();
            _input.currentActionMap?.Disable();
        }
        _cam = null;
    }

    private void Recovery()
    {
        if (CanRecHp)
            stats.RecoverHp(Time.deltaTime);

        if (CanRecMana)
            stats.RecoverMana(Time.deltaTime);

        if (CanRecSt)
            stats.RecoverStamina(Time.deltaTime);
    }

    private void OnChantAction(InputAction.CallbackContext context)
    {
        if (!CanChant || !CanAct) return;
        isChanting = true;
        _circle.ToggleState(true);
        _anim.SetBool("Chanting", true);
    }

    private void OnAbortAction(InputAction.CallbackContext context)
    {
        if (!isChanting || !CanAct) return;
        isChanting = false;
        _circle.ToggleState(false);
        _anim.SetBool("Chanting", false);
    }

    private void OnJumpAction(InputAction.CallbackContext context)
    {
        if (!CanJump) return;
        _jumpRequested = true;
        stats.Rest(-jumpCost);
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

        float currentSpeed = 0f;
        Vector3 moveTargetDir = Vector3.zero;

        if (CanMove)
        {
            if (CanRun)
            {
                isRunning = _run.IsPressed();
                if (isRunning)
                    stats.Rest(-Time.deltaTime * runCost);
            }
            else
                isRunning = false;

            currentSpeed = moveSpeed * SpeedScale();

            if (isMoving && _cam != null)
            {
                Vector3 camForward = _cam.transform.forward;
                Vector3 camRight = _cam.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                moveTargetDir = camForward * _inputDir.y + camRight * _inputDir.x;
                moveTargetDir.Normalize();

                Quaternion targetRotation = Quaternion.LookRotation(moveTargetDir);
                modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }

        _cc.Move(moveTargetDir * (currentSpeed * Time.deltaTime));

        if (_jumpRequested)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _jumpRequested = false;
            _animLandedState = false;

            if (_anim != null)
            {
                _anim.SetTrigger("Jump");
            }
        }

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
            targetAnimY = 0.5f * SpeedScale();
        }

        _anim.SetFloat("X", targetAnimX, animDampTime, Time.deltaTime);
        _anim.SetFloat("Y", targetAnimY, animDampTime, Time.deltaTime);
        _anim.SetBool("Landed", _animLandedState);
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