using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerController : CreatureController
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
    private InputAction _abort;
    private InputAction _aim;
    private InputAction _complete;
    private InputAction _use;

    private Vector2 _inputDir;
    private CharacterController _cc;
    public Transform modelRoot;
    public Transform mainFire;
    public Transform triFire0;
    public Transform triFire1;
    public Transform triFire2;
    private CircleController _circle;

    public bool isGrounded;
    public bool isAiming;
    public bool isRunning;
    public bool isChanting;
    public bool isCasting;

    public bool chantStart;
    public bool chantRecovery;

    private bool _isUsingItem = false;

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

    public float normalFOV = 45f;
    public float aimFOV = 28f;
    public float fovSmoothSpeed = 13f;
    private CinemachineVirtualCamera _vCam;

    public float aimTargetMaxDistance = 100f;
    public LayerMask aimLayerMask = 0;

    [Header("Movement Settings")]
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
    public bool CanJump => isGrounded && stats.stamina >= jumpCost && !isChanting && CanAct && !isCasting;
    public bool CanChant => isGrounded && !isChanting && CanAct && !isCasting;
    public bool CanMove => !isChanting && CanAct;
    public bool CanRun => stats.stamina > 0f && isGrounded && !isAiming && !isCasting;
    public bool CanUse => CanAct && isGrounded && !isChanting;
    public bool CanRecHp => true;
    public bool CanRecMana=> true;
    public bool CanRecSt => isGrounded;
    public bool CanInteract => CanAct && !isChanting && !isCasting;

    public float SpeedScale()
    {
        float scale = 1f;
        if (isRunning && isGrounded)
            scale *= runSpeedScale;
        else if (!isGrounded)
            scale *= airSpeedScale;

        int windLv = stats.elementStats[ElementType.Wind].lvl;
        if (windLv > 1)
        {
            scale += scale * (windLv - 1) * 0.1f;
        }

        return scale;
    }


    protected override void Awake()
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
        _abort = _input.actions["Abort"];
        _complete = _input.actions["Complete"];
        _aim = _input.actions["Aim"];
        _use = _input.actions["Use"];

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

        HandleItemUseTick();

        HandleMovement();
        HandleAnimation();
    }

    private void LateUpdate()
    {
        HandleRotation();
        HandleAim();
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
        _abort.performed += OnAbortAction;
        _complete.performed += OnCompleteCast;
        _aim.performed += OnAimAction;
        _aim.canceled += OnCancelAimAction;
        _use.started += OnUseActionStarted;
        _use.canceled += OnUseActionEnded;
    }

    private void UnbindActions()
    {
        _jump.performed -= OnJumpAction;
        _chant.performed -= OnChantAction;
        _abort.performed -= OnAbortAction;
        _complete.performed -= OnCompleteCast;
        _aim.performed -= OnAimAction;
        _aim.canceled -= OnCancelAimAction;
        _use.started -= OnUseActionStarted;
        _use.canceled -= OnUseActionEnded;
    }

    private void OnUseActionStarted(InputAction.CallbackContext context)
    {
        _isUsingItem = true;

        if (CanUse)
        {
            GetUseHandler()?.OnUseStart(CreateUseContext());
        }
    }

    private void OnUseActionEnded(InputAction.CallbackContext context)
    {
        if (_isUsingItem)
        {
            _isUsingItem = false;
            GetUseHandler()?.OnUseEnd(CreateUseContext());
        }
    }

    private void HandleItemUseTick()
    {
        if (!_use.IsPressed())
        {
            if (_isUsingItem)
            {
                _isUsingItem = false;
                GetUseHandler()?.OnUseEnd(CreateUseContext());
            }
            return;
        }

        if (!CanUse)
        {
            if (isCasting)
            {
                GetUseHandler()?.OnUseEnd(CreateUseContext());
            }
            return;
        }

        GetUseHandler()?.OnUseTick(CreateUseContext());
    }

    private IItemUseHandler GetUseHandler()
    {
        if (stats == null || stats.currentSelectedItem == null) return null;
        if (stats.currentSelectedItem.runtimeData == null) return null;
        return stats.currentSelectedItem.runtimeData.GetUseHandler();
    }

    private ItemUseContext CreateUseContext()
    {
        return new ItemUseContext
        {
            player = this,
            mainCamera = _cam,
        };
    }

    public void Initialize(PlayerStats st, CinemachineVirtualCamera cam)
    {
        Cleanup();
        stats = st;
        _vCam = cam;
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

    private void OnAimAction(InputAction.CallbackContext context) => isAiming = true;
    private void OnCancelAimAction(InputAction.CallbackContext context) => isAiming = false;

    private void OnChantAction(InputAction.CallbackContext context)
    {
        if (!CanChant || !CanAct) return;
        isChanting = true;
        _circle.ToggleState(true);
        _anim.SetBool("Chanting", true);

        UIManager.Instance.StartWordOrbit(stats.unlockedMagicKeywords, transform, new Vector3(0f, 0.5f, 0f));
    }

    private void OnAbortAction(InputAction.CallbackContext context)
    {
        if (!isChanting || !CanAct) return;
        isChanting = false;
        _circle.ToggleState(false);
        _anim.SetBool("Chanting", false);

        UIManager.Instance.wordOrbit.AbortChant();
    }

    private void OnCompleteCast(InputAction.CallbackContext context)
    {
        if (!isChanting || !CanAct) return;

        isChanting = false;
        _circle.ToggleState(false);
        _anim.SetBool("Chanting", false);

        UIManager.Instance.wordOrbit.CompleteChant();
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

            if (_cam != null)
            {
                Vector3 camForward = _cam.transform.forward;
                Vector3 camRight = _cam.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                moveTargetDir = camForward * _inputDir.y + camRight * _inputDir.x;
                if (moveTargetDir.sqrMagnitude > 0.001f)
                {
                    moveTargetDir.Normalize();
                }
            }

            if (_cam != null)
            {
                if (isAiming && isGrounded)
                {
                    Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                    Vector3 targetPoint;

                    if (Physics.Raycast(ray, out RaycastHit hit, aimTargetMaxDistance, aimLayerMask))
                    {
                        targetPoint = hit.point;
                    }
                    else
                    {
                        targetPoint = ray.GetPoint(aimTargetMaxDistance);
                    }

                    Vector3 lookDir = targetPoint - transform.position;
                    lookDir.y = 0f;

                    if (lookDir.sqrMagnitude > 0.001f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                        modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetRotation, Time.deltaTime * 2f *rotationSpeed);
                    }
                }
                else if (isMoving)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveTargetDir);
                    modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
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
            if (isAiming)
            {
                Vector3 camForward = _cam.transform.forward;
                Vector3 camRight = _cam.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 worldMoveDir = camForward * _inputDir.y + camRight * _inputDir.x;

                Vector3 localMoveDir = modelRoot.InverseTransformDirection(worldMoveDir);

                float speedMultiplier = 0.5f * SpeedScale();
                targetAnimX = localMoveDir.x * speedMultiplier;
                targetAnimY = localMoveDir.z * speedMultiplier;
            }
            else
            {
                targetAnimY = 0.5f * SpeedScale();
            }
        }

        _anim.SetFloat("X", targetAnimX, animDampTime, Time.deltaTime);
        _anim.SetFloat("Y", targetAnimY, animDampTime, Time.deltaTime);
        _anim.SetBool("Landed", _animLandedState);
        _anim.SetBool("Casting", isCasting);
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

    private void HandleAim()
    {
        if (_vCam != null)
        {
            float targetFOV = isAiming ? aimFOV : normalFOV;
            _vCam.m_Lens.FieldOfView = Mathf.Lerp(_vCam.m_Lens.FieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
        }
    }
}