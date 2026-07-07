using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ElementType
{
    None,
    Wind,
    Fire,
    Water,
    Grass,
    Dark
}

public static class ElementExtensions
{
    public static string GetName(this ElementType type)
    {
        return type switch
        {
            ElementType.None => "无属性",
            ElementType.Wind => "风属性",
            ElementType.Fire => "火属性",
            ElementType.Water => "水属性",
            ElementType.Grass => "草属性",
            ElementType.Dark => "暗属性",
            _ => "无属性"
        };
    }

    public static Color32 GetColor(this ElementType type)
    {
        return type switch
        {
            ElementType.None => new Color32(0, 0, 0, 255),
            ElementType.Wind => new Color32(170, 204, 198, 255),
            ElementType.Fire => new Color32(217, 55, 47, 255),
            ElementType.Water => new Color32(59, 162, 218, 255),
            ElementType.Grass => new Color32(66, 155, 55, 255),
            ElementType.Dark => new Color32(197, 28, 156, 255),
            _ => new Color32(0, 0, 0, 255),
        };
    }

    public static ElementType FromString(string name, bool ignoreCase = true)
    {
        if (Enum.TryParse(name, ignoreCase, out ElementType result))
        {
            return result;
        }

        return ElementType.None;
    }
}

[CreateAssetMenu(menuName = "Magic/ElementSeg")]
public class ElementSeg : MagicSeg
{
    public ElementType type;
    public float damageScale;
    public float costScale;
    public float speedScale;
}
