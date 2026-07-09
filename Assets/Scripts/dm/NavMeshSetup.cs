using UnityEngine;
using Unity.AI.Navigation;   // 关键：引入此命名空间

public class NavMeshSetup : MonoBehaviour
{
    public NavMeshSurface surface;

    void Start()
    {
        if (surface != null)
        {
            surface.BuildNavMesh();
            Debug.Log("NavMesh 运行时重建成功");
        }
        else
        {
            // 自动查找场景中的 NavMeshSurface
            surface = FindObjectOfType<NavMeshSurface>();
            if (surface != null)
            {
                surface.BuildNavMesh();
                Debug.Log("NavMesh 自动重建成功");
            }
            else
            {
                Debug.LogError("场景中没有 NavMeshSurface！");
            }
        }
    }
}