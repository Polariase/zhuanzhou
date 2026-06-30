using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;

    [Header("距离与高度")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float heightOffset = 1.5f;

    [Header("鼠标灵敏度")]
    [SerializeField] private float sensitivity = 2f;

    [Header("角度限制（防止翻转）")]
    [SerializeField] private float minYAngle = -30f;
    [SerializeField] private float maxYAngle = 60f;

    private float currentX = 0f;
    private float currentY = 20f;

    // 新增：记录当前是否锁定鼠标（方便UI调用）
    private bool isCursorLocked = true;

    void Start()
    {
        // 初始锁定并隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;

        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        // ----- 新增：ESC 键切换鼠标锁定状态（方便调出设置UI）-----
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 如果当前是锁定状态，就解锁（显示鼠标）；否则锁定（隐藏鼠标）
            if (isCursorLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isCursorLocked = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                isCursorLocked = true;
            }
        }
    }

    void LateUpdate()
    {
        // 注意：只有在鼠标锁定时，才响应鼠标移动旋转视角
        // 如果鼠标解锁（比如在设置界面），就不转动摄像机，防止干扰UI操作
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (target == null) return;

            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            currentX += mouseX;
            currentY -= mouseY;
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

            Vector3 targetPosition = target.position - rotation * Vector3.forward * distance;
            transform.position = targetPosition;

            Vector3 lookAtPoint = target.position + Vector3.up * heightOffset;
            transform.LookAt(lookAtPoint);
        }
    }

    // 公共方法：供UI按钮调用（比如“关闭设置”按钮）
    public void ToggleCursorLock(bool locked)
    {
        isCursorLocked = locked;
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}