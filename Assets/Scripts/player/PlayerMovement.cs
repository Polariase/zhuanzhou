using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("跳跃与重力")]
    [SerializeField] private float jumpHeight = 1.8f;      // 跳跃高度
    [SerializeField] private float gravity = -9.81f;        // 重力加速度（负数）
    [SerializeField] private float groundCheckOffset = 0.1f; // 地面检测微小偏移

    [Header("摄像机")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection;
    private Vector3 velocity; // 用于累加重力/跳跃速度

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // ----- 1. 获取输入并计算移动方向（相对摄像机） -----
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * vertical) + (right * horizontal);
        moveDirection.Normalize();

        // ----- 2. 动画控制（和之前一样） -----
        bool isMoving = moveDirection.magnitude > 0.1f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving;

        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isRunning", isRunning);

        // ----- 3. 速度与跳跃/重力逻辑（核心升级部分） -----
        float currentSpeed = isMoving ? (isRunning ? runSpeed : walkSpeed) : 0f;

        // 3.1 地面检测（CharacterController 自带 isGrounded）
        if (controller.isGrounded && velocity.y < 0)
        {
            // 触地时，给一个极小的下压力（确保角色一直粘着地面，适应斜坡）
            velocity.y = -1f;
        }

        // 3.2 跳跃（按空格触发）
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            // 物理公式：初速度 = sqrt(2 * 重力加速度 * 跳跃高度)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 3.3 始终累加重力（无论是否在地面，保证斜坡下落和跳跃下落）
        velocity.y += gravity * Time.deltaTime;

        // 3.4 组装最终位移向量
        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y;

        // 执行移动（CharacterController 会自动处理斜坡碰撞和滑动）
        controller.Move(finalMove * Time.deltaTime);

        // ----- 4. 角色转向（保持平滑） -----
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // （可选）后期用于外部取消移动或攻击时锁定的公共方法
    public void SetMoveLock(bool locked) { /* 预留攻击锁定接口 */ }
}