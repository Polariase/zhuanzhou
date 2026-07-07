using UnityEngine;

public class TestSpawn : MonoBehaviour
{
    public GameObject prefab; // 在 Inspector 中拖入你的预制体

    void Start()
    {
        if (prefab == null)
        {
            Debug.LogError("请拖入预制体！");
            return;
        }

        // 在摄像机前方 3 米处生成
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 3;
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        Debug.Log($"生成物体：{obj.name}，位置：{obj.transform.position}");
    }
}