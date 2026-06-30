using Cinemachine;

using UnityEngine;

using UnityEngine.EventSystems;

using UnityEngine.InputSystem;



public class PlayerController : MonoBehaviour
{
    private PlayerInput _input;
    private InputAction _look;
    private InputAction _move;
    private Vector2 _inputDir;
    private CharacterController _cc;
    public Transform modelRoot;

    public bool isGrounded;
    public bool isMoving;
    public bool isJumping;

    [Header("Cinemachine")]
    public GameObject camTarget;
    public float topClamp = 45f;
    public float botClamp = -30f;
    public float camAngleOverride = 0.0f;
    private const float _threshold = 0.01f;
    public float xSens = 16f;
    public float ySens = 8f;
    public bool lockCam = false;

    private float _targetPitch;
    private float _targetYaw;

    private Camera _mainCamera;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;
    public float gravity = -15f;
    
    private Vector3 _velocity;



    private void Awake()
    {
        _input = GetComponent<PlayerInput>();

        _look = _input.actions["Look"];

        _move = _input.actions["Move"];

        _cc = GetComponent<CharacterController>();

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
    }

    private void FixedUpdate()
    {
        isGrounded = _cc.isGrounded;
        HandleMovement();
        
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
            modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }

        _cc.Move(moveTargetDir * (moveSpeed * Time.fixedDeltaTime));

        _velocity.y += gravity * Time.fixedDeltaTime;
        _cc.Move(_velocity * Time.fixedDeltaTime);
    }

    private void HandleRotation()
    {
        Vector2 lookValue = _look.ReadValue<Vector2>();
        if (lookValue.sqrMagnitude >= _threshold && !lockCam)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.fixedDeltaTime;

            _targetYaw += lookValue.x * deltaTimeMultiplier * xSens;
            _targetPitch -= lookValue.y * deltaTimeMultiplier * ySens;
        }

        _targetPitch = ClampAngle(_targetPitch, botClamp, topClamp);
        _targetYaw = ClampAngle(_targetYaw, -360f, 360f);

        camTarget.transform.localRotation = Quaternion.Euler(_targetPitch + camAngleOverride, _targetYaw, 0.0f);
    }

}