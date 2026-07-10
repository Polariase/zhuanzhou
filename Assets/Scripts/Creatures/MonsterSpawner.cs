using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    [Header("怪物预制体池")]
    [SerializeField] private List<GameObject> monsterPrefabs = new List<GameObject>();

    [Header("生成区域参数")]
    [SerializeField] private float spawnRadius = 500f; // 以当前Spawner为中心寻找NavMesh点的半径

    [Header("数量控制参数")]
    [SerializeField] private int initialSpawnCount = 12; // 初始生成数量
    [SerializeField] private int softMaxLimit = 20;     // 触发减速的数量阈值

    [Header("生成间隔参数 (秒)")]
    [SerializeField] private float normalSpawnInterval = 15f; // 正常生成间隔
    [SerializeField] private float slowSpawnInterval = 60f;   // 数量超标后的生成间隔

    // 内部维护的当前存活怪物列表
    private List<GameObject> activeMonsters = new List<GameObject>();
    private float nextSpawnTime;

    void Start()
    {
        if (monsterPrefabs == null || monsterPrefabs.Count == 0)
        {
            Debug.LogError("MonsterSpawner: 请先在 Inspector 中挂载至少一个怪物预制体！");
            return;
        }

        // 1. 刚开始时直接生成 12 个怪物
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnSingleMonster();
        }

        // 2. 设置下一次定时生成的触发时间
        UpdateNextSpawnTime();
    }

    void Update()
    {
        // 定期清理列表中已经被销毁或隐藏（SetActive(false)）的怪物
        CleanActiveMonstersList();

        // 3. 检查时间，执行定时生成
        if (Time.time >= nextSpawnTime)
        {
            SpawnSingleMonster();
            UpdateNextSpawnTime(); // 根据当前怪物数量重新计算下一次生成的间隔
        }
    }

    /// <summary>
    /// 生成单个随机怪物的核心逻辑
    /// </summary>
    private void SpawnSingleMonster()
    {
        Vector3 spawnPosition;
        if (TryGetRandomNavMeshPosition(out spawnPosition))
        {
            // 随机选择一个预制体
            int randomIndex = Random.Range(0, monsterPrefabs.Count);
            GameObject prefab = monsterPrefabs[randomIndex];

            // 生成怪物
            GameObject spawnedMonster = Instantiate(prefab, spawnPosition, Quaternion.identity);

            // 将生成的怪物记录到列表中
            activeMonsters.Add(spawnedMonster);
        }
        else
        {
            Debug.LogWarning("MonsterSpawner: 未能在指定的 NavMesh 范围内找到有效的生成点。");
        }
    }

    /// <summary>
    /// 根据当前存活的怪物数量，动态更新下一次生成的时间
    /// </summary>
    private void UpdateNextSpawnTime()
    {
        // 动态判断：如果数量大于 20，使用 60 秒间隔；否则使用 15 秒间隔
        float currentInterval = activeMonsters.Count > softMaxLimit ? slowSpawnInterval : normalSpawnInterval;
        nextSpawnTime = Time.time + currentInterval;
    }

    /// <summary>
    /// 在 NavMesh 网格上随机寻找一个有效的点
    /// </summary>
    private bool TryGetRandomNavMeshPosition(out Vector3 result)
    {
        // 在球体范围内随机选择一个方向和距离
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection += transform.position; // 以当前 Spawner 物体的位置为基准
        randomDirection.y = transform.position.y; // 保持在相似的高度平面上

        NavMeshHit hit;
        // 在 NavMesh 上对随机点进行采样，寻找最近的有效网格点
        if (NavMesh.SamplePosition(randomDirection, out hit, spawnRadius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    /// <summary>
    /// 清理已经被销毁或灭活的怪物对象，确保计数准确
    /// </summary>
    private void CleanActiveMonstersList()
    {
        for (int i = activeMonsters.Count - 1; i >= 0; i--)
        {
            // 如果怪物被销毁了（比如 Die() 里的逻辑）或者被隐藏进了对象池（SetActive(false)）
            if (activeMonsters[i] == null || !activeMonsters[i].activeInHierarchy)
            {
                activeMonsters.RemoveAt(i);
            }
        }
    }

    // 辅助可视化：在 Scene 视图中画出生成检测的范围半径
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
