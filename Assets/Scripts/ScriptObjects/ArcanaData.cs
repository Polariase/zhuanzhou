using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Magic/Arcana")]
public class ArcanaData : MagicSeg
{
    public float damage;
    public float cost;
    public float speed;
    public Image icon;
    public GameObject prefab;
}
