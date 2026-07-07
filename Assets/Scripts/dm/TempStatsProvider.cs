using UnityEngine;

/// <summary>
/// 临时数据提供者，用于测试UI，等队友集成后替换为 PlayerController.Instance.stats
/// </summary>
public class TempStatsProvider : MonoBehaviour
{
    public static TempStatsProvider Instance { get; private set; }

    public PlayerStats stats = new PlayerStats();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 通过 GainExp 增加经验，自动处理升级
        stats.elementStats[ElementType.Wind].GainExp(15f);
        stats.elementStats[ElementType.Fire].GainExp(8f);
        // 其他元素不增加经验，保持默认
    }
}