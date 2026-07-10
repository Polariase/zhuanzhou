using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInjecter : MonoBehaviour
{
    public int id;
    public int n;
    private void Start()
    {
        GetComponent<ItemObject>().item = new(DataManager.Instance.GetItemData(id), n);
    }
}
