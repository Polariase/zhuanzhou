using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EmitterType
{
    Normal,    
    Combo
}

[CreateAssetMenu(menuName = "Magic/Emitter")]
public class EmitterSeg : MagicSeg
{
    public EmitterType emitterType = EmitterType.Normal;
    public float fireRateScale = 1f;
    public float costScale = 1f;

    public EmitterSeg()
    {
        segStr = "Emitter";
    }
}
