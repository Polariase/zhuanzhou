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
    public float distance;
    public float firerate;
    public float detectSize;
    public float size;
    public string projKey;

    public ArcanaSeg()
    {
        segStr = "Arcana";
    }
}
