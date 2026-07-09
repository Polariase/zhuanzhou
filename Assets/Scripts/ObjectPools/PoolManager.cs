using MyPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    public ItemObjectPool item;
    public PopupPool popup;
    public ProjPool proj;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            item = GetComponentInChildren<ItemObjectPool>();
            popup = GetComponentInChildren<PopupPool>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DeactiveAll()
    {
        item.DeactivateAllPoolObjects();
        popup.DeactivateAllPoolObjects();
    }
}