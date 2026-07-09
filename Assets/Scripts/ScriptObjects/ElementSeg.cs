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
    private static readonly float[][] DamageMatrix = new float[][]
    {
        // ·ÀÊØ·½: None   Wind   Fire   Water  Grass  Dark
        new float[] { 1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  0.75f }, // ¹¥»÷·½: None
        new float[] { 1.0f,  1.0f,  0.67f, 1.0f,  1.5f,  0.75f }, // ¹¥»÷·½: Wind 
        new float[] { 1.0f,  1.0f,  1.0f,  0.67f, 1.5f,  0.75f }, // ¹¥»÷·½: Fire 
        new float[] { 1.0f,  1.0f,  1.5f,  1.0f,  0.67f, 0.75f }, // ¹¥»÷·½: Water 
        new float[] { 1.0f,  0.67f, 0.67f, 1.5f,  1.0f,  0.75f }, // ¹¥»÷·½: Grass 
        new float[] { 1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.5f }  // ¹¥»÷·½: Dark 
    };

    public static float GetDamageMultiplier(this ElementType attacker, ElementType defender)
    {
        int attackerIndex = (int)attacker;
        int defenderIndex = (int)defender;

        if (attackerIndex < 0 || attackerIndex >= DamageMatrix.Length ||
            defenderIndex < 0 || defenderIndex >= DamageMatrix[attackerIndex].Length)
        {
            return 1.0f;
        }

        return DamageMatrix[attackerIndex][defenderIndex];
    }
    public static string GetName(this ElementType type)
    {
        return type switch
        {
            ElementType.None => "ÎÞÊôÐÔ",
            ElementType.Wind => "·çÊôÐÔ",
            ElementType.Fire => "»ðÊôÐÔ",
            ElementType.Water => "Ë®ÊôÐÔ",
            ElementType.Grass => "²ÝÊôÐÔ",
            ElementType.Dark => "°µÊôÐÔ",
            _ => "ÎÞÊôÐÔ"
        };
    }

    public static string GetTypeName(this ElementType type)
    {
        return type switch
        {
            ElementType.None => "none",
            ElementType.Wind => "wind",
            ElementType.Fire => "fire",
            ElementType.Water => "water",
            ElementType.Grass => "grass",
            ElementType.Dark => "dark",
            _ => "none"
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
    public float distScale;
    public float rateScale;
    public int tier;
}
