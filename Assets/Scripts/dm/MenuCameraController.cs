using UnityEngine;
using System.Collections;

public class MenuCameraController : MonoBehaviour
{
    [Header("路径点")]
    public Transform[] waypoints;          // 所有路径点
    public float moveSpeed = 0.5f;         // 移动速度
    public float waitTime = 3f;            // 在每个点停留的时间

    private int currentTargetIndex = 0;
    private float journey = 0f;
    private Vector3 startPos;
    private Quaternion startRot;
    private Transform target;

    void Start()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("没有设置路径点！");
            return;
        }

        // 从第一个点开始
        transform.position = waypoints[0].position;
        transform.rotation = waypoints[0].rotation;
        currentTargetIndex = 0;
        StartCoroutine(MoveToNextPoint());
    }

    IEnumerator MoveToNextPoint()
    {
        while (true)
        {
            // 在当前位置停留
            yield return new WaitForSeconds(waitTime);

            // 选择下一个目标点（循环）
            currentTargetIndex = (currentTargetIndex + 1) % waypoints.Length;
            target = waypoints[currentTargetIndex];

            // 开始移动
            startPos = transform.position;
            startRot = transform.rotation;
            journey = 0f;

            while (journey < 1f)
            {
                journey += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, target.position, journey);
                transform.rotation = Quaternion.Slerp(startRot, target.rotation, journey);
                yield return null;
            }

            // 精确对准目标
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }
}