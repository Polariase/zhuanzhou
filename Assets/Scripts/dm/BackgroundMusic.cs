using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

    void Awake()
    {
        // 如果已经存在一个实例，则销毁当前物体
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // 否则保存实例，并标记为不销毁
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}