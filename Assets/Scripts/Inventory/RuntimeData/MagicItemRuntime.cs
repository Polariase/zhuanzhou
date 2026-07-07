using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MagicItemRuntime : ItemRuntimeData
{
    public ArcanaSeg arcana;
    public ElementSeg element;
    public EmitterSeg emitter;
    public ModifierSeg modifier;

    public float Damage => arcana != null ? arcana.damage * (element != null ? element.damageScale : 1f) * (modifier != null ? modifier.damageScale : 1f) : 0f;
    public float Cost => arcana != null ? arcana.cost * (element != null ? element.costScale : 1f) * (modifier != null ? modifier.costScale : 1f) : 0f;
    public float Speed => arcana != null ? arcana.speed * (element != null ? element.speedScale : 1f) * (modifier != null ? modifier.speedScale : 1f) : 0f;

    public ElementType Element => element != null ? element.type : ElementType.None;
    public GameObject ProjectilePrefab => arcana != null ? arcana.projPrefab : null;


    public MagicItemRuntime(ArcanaSeg arcana, ElementSeg element, EmitterSeg emitter, ModifierSeg modifier)
    {
        this.arcana = arcana;
        this.element = element;
        this.emitter = emitter;
        this.modifier = modifier;
    }

    public override int ItemID
    {
        get
        {
            if (arcana == null) return 0;
            int arcId = arcana.GetInstanceID();
            int eleId = element != null ? element.GetInstanceID() : 0;
            int emiId = emitter != null ? emitter.GetInstanceID() : 0;
            int modId = modifier != null ? modifier.GetInstanceID() : 0;

            return HashCode.Combine(arcId, eleId, emiId, modId);
        }
    }

    public override string ItemName
    {
        get
        {
            string emitStr = emitter != null? emitter.segName : "";
            string eleStr = element != null ? $"[{element.segName}]" : "无属性";
            string arcStr = arcana != null ? arcana.segName : "无效法术";
            string modStr = modifier != null ? $"·改" : "";
            return $"{emitStr}{eleStr}{arcStr}{modStr}";
        }
    }

    public override ItemType Type => ItemType.Magic;

    public override string IconAddress => arcana != null ? arcana.iconAddress : "";
    public override string PrefabAddress => arcana != null ? arcana.prefabAddress : "";
    public override int MaxStack => 1;

    public override string Desc
    {
        get
        {
            return $"秘术：{arcana.segName}\n" +
                   $"元素：{element.segName}\n" +
                   $"发射方式：{emitter.segName}\n" +
                   $"修饰符：{modifier.segName}\n";
        }
    }

    public override ItemRuntimeData Clone()
    {
        return new MagicItemRuntime(arcana, element, emitter, modifier);
    }
}