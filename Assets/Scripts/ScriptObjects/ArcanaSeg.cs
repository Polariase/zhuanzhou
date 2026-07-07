using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Magic/Arcana")]
public class ArcanaSeg : MagicSeg
{
    public float damage;
    public float cost;
    public float speed;
    public string iconAddress;
    public string prefabAddress;
    public GameObject projPrefab;
}
