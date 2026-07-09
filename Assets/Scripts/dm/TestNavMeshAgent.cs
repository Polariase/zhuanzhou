using UnityEngine;
using UnityEngine.AI;

public class TestNavMeshAgent : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform target;   // 在 Inspector 中拖入一个空物体作为目标点

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("没有 NavMeshAgent 组件！");
            return;
        }

        if (target != null)
        {
            agent.SetDestination(target.position);
            Debug.Log($"设置目标：{target.position}");
        }
        else
        {
            Debug.LogError("未设置目标点！");
        }
    }
}