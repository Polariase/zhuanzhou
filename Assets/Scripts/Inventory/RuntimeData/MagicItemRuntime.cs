using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MagicItemRuntime : ItemRuntimeData,IItemUseHandler
{
    public override bool CanPlaceInHotbar => true;
    public override IItemUseHandler GetUseHandler() => this;

    public ArcanaSeg arcana;
    public ElementSeg element;
    public EmitterSeg emitter;
    public ModifierSeg modifier;

    public float Damage => arcana.damage * element.damageScale * (modifier != null ? modifier.damageScale : 1f);
    public float Cost => arcana.cost * element.costScale * emitter.costScale * (modifier != null ? modifier.costScale : 1f);
    public float Speed => arcana.speed * element.speedScale * (modifier != null ? modifier.speedScale : 1f);
    public float Dist => arcana.distance * element.distScale * (modifier != null ? modifier.distScale : 1f);
    public float FireRate => arcana.firerate * element.rateScale * emitter.fireRateScale * (modifier != null ? modifier.rateScale : 1f);
    public float Size => arcana.size * (modifier != null ? modifier.sizeScale : 1f);
    public float DetectSize => arcana.detectSize * (modifier != null ? modifier.sizeScale : 1f);

    public ElementType Element => element.type;
    public string ProjKey => arcana != null ? arcana.projKey : "";

    private int _currentTriFireIndex = 0;


    public MagicItemRuntime(ArcanaSeg arcana, ElementSeg element, EmitterSeg emitter, ModifierSeg modifier)
    {
        this.arcana = arcana;
        this.element = element;
        this.emitter = emitter != null ? emitter : DataManager.Instance.GetMagicSeg<EmitterSeg>("changgui");
        this.modifier = modifier;
    }

    public override int ItemID
    {
        get
        {
            if (arcana == null || element == null || emitter == null) return 1;
            int arcId = arcana.id;
            int eleId = element.id;
            int emiId = emitter.id;
            int modId = modifier != null ? modifier.id : 0;

            return arcId + eleId + emiId + modId;
        }
    }

    public override string ItemName
    {
        get
        {
            if (emitter == null || element == null || arcana == null) return "无效法术";
            string emitStr = emitter.segName;
            string eleStr = element.segName;
            string arcStr = arcana.segName;
            string modStr = modifier != null ? $"·改" : "";
            return $"{emitStr}{eleStr}{arcStr}{modStr}";
        }
    }

    public override ItemType Type => ItemType.Magic;

    public override string IconAddress => "法术道具Icon";
    public override string PrefabAddress => "法术道具";
    public override int MaxStack => 1;

    public override string Desc
    {
        get
        {
            if (arcana == null || element == null || emitter == null) return "无效法术";
            return $"秘术：{arcana.segName}\n" +
                   $"元素：{element.segName}\n" +
                   $"发射方式：{emitter.segName}\n" + (modifier != null ?
                   $"修饰符：{modifier.segName}\n" : "");
        }
    }

    public override ItemRuntimeData Clone()
    {
        return new MagicItemRuntime(arcana, element, emitter, modifier);
    }

    private float _nextFireTime = 0f;

    public void OnUseStart(ItemUseContext context)
    {
        if (arcana == null || element == null || emitter == null) return;
        context.player.isCasting = true;
    }

    public void OnUseTick(ItemUseContext context)
    {
        if (arcana == null || element == null || emitter == null) return;

        if (Time.time < _nextFireTime)
        {
            return;
        }

        if (context.player.stats.mana < Cost)
        {
            OnUseEnd(context);
            return;
        }

        context.player.isCasting = true;

        context.player.stats.Regene(-Cost);

        // 如果冷却结束，正式产生子弹并刷新冷却
        SpawnProjectile(context.player,context);

        float fireInterval = FireRate > 0 ? (1f / FireRate) : 0.5f;
        _nextFireTime = Time.time + fireInterval;

    }

    public void OnUseEnd(ItemUseContext context)
    {
        if (arcana == null || element == null || emitter == null) return;

        context.player.isCasting = false;
        _currentTriFireIndex = 0;
    }

    private void SpawnProjectile(PlayerController player,ItemUseContext context)
    {
        if (arcana == null || element == null || emitter == null) return;

        Transform fireTransform = player.mainFire;
        if (emitter != null && emitter.emitterType == EmitterType.Combo)
        {
            fireTransform = _currentTriFireIndex switch
            {
                0 => player.triFire0,
                1 => player.triFire1,
                2 => player.triFire2,
                _ => fireTransform
            };

            _currentTriFireIndex = (_currentTriFireIndex + 1) % 3;
        }

        Vector3 firePosition = fireTransform.position;

        // 2. 初始化发射朝向
        Quaternion fireRotation = Quaternion.identity;

        if (player.isAiming && player.camTarget != null)
        {
            // 【瞄准状态】：获取相机屏幕中心发出的射线命中点
            // 直接借用你在 PlayerController 里的逻辑，但保留完整的 3D 方向（不抹平 y 轴）
            Camera mainCam = context.mainCamera; // 或者通过 context 传进来，这里用 Camera.main 兜底
            if (mainCam != null)
            {
                Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                Vector3 targetPoint;

                if (Physics.Raycast(ray, out RaycastHit hit, player.aimTargetMaxDistance, player.aimLayerMask))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    targetPoint = ray.GetPoint(player.aimTargetMaxDistance);
                }

                // 计算从【发射点】指向【准星命中点】的完整 3D 向量
                Vector3 aimDirection = targetPoint - firePosition;

                if (aimDirection.sqrMagnitude > 0.001f)
                {
                    fireRotation = Quaternion.LookRotation(aimDirection.normalized);
                }
                else
                {
                    fireRotation = player.modelRoot.rotation;
                }
            }
        }
        else
        {
            // 【非瞄准状态】：直接朝向 modelRoot 的正前方（仅水平方向）
            fireRotation = player.modelRoot.rotation;
        }

        // 3. 生成并配置子弹
        string elementString = Element.GetTypeName();
        string bulletPoolKey = $"{ProjKey}{elementString}";
        string muzzlePoolKey = $"{bulletPoolKey}muzzle";

        GameObject bulletObj = PoolManager.Instance.proj.GetAndSetProj(bulletPoolKey, firePosition, fireRotation);

        if (bulletObj != null)
        {
            Projectile projScript = bulletObj.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.speed = Speed;
                projScript.detectionRadius = DetectSize;
                projScript.InitProjectile(firePosition, fireRotation, Dist, Size, Damage, Element);
            }
        }

        GameObject muzzleObj = PoolManager.Instance.proj.GetAndSetEffect(muzzlePoolKey, firePosition, fireRotation);
        if (muzzleObj != null)
        {
            muzzleObj.transform.localScale = Vector3.one * Size;
        }
    }
}