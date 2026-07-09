using System;
using UnityEngine;

public abstract class CreatureController : MonoBehaviour, IHittable
{
    [Header("移动设置")]
    public float moveSpeed = 6f;

    [Header("引用")]
    protected Animator _anim;
    public CapsuleCollider capsule;

    [Header("元素属性")]
    public ElementType creatureElement = ElementType.None;

    public bool isDead;
    public bool isMoving;
    public bool isInvincible;

    protected virtual void Awake()
    {
        if (_anim == null) _anim = GetComponent<Animator>();
        capsule = GetComponentInChildren<CapsuleCollider>();
    }

    public virtual void Say(string text, float duration = 2f, WordTextType type = WordTextType.Neutral)
    {
        if (isDead) return;

        Vector3 spawnPos = capsule != null  ? capsule.bounds.center + Vector3.up * capsule.bounds.extents.y
            : transform.position + Vector3.up * 2f;

        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowWordText(spawnPos, text, type, transform, duration);
        }
    }

    public virtual bool Hit(float damage, Vector3 hitPoint, bool isElementAdvantage)
    {
        if (isInvincible) return false;
        return TakeDamage(damage, hitPoint, isElementAdvantage);
    }

    public virtual bool TakeDamage(float damage, Vector3 hitPoint, bool isElementAdvantage)
    {
        return false;
    }

    public Vector3 HitPoint()
    {
        return capsule.bounds.center;
    }

    public virtual void Die()
    {
        return;
    }
}