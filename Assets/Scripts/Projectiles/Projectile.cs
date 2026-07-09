using MyPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float detectionRadius = 0.1f;
    public LayerMask hitLayers;
    public List<ParticleSystem> trailParticles;
    public List<TrailRenderer> trailRenderers;
    private ParticleSystem _ps;

    private string _hitKey;
    private PoolItem _poolItem;
    private bool _hasCollided;
    private Vector3 _lastPosition;
    private float _maxTrailDuration;

    private float _maxDistance = 100f;
    private float _travelled = 0f;
    private float _currentSize = 1f;

    public float baseDamage = 0f;
    public ElementType projectileElement = ElementType.None;

    private void Awake()
    {
        _poolItem = GetComponent<PoolItem>();
        _maxTrailDuration = CalculateMaxTrailDuration();
        _ps = GetComponent<ParticleSystem>();
        hitLayers = ~0;
        _hitKey = _poolItem.key + "hit";
    }

    public void InitProjectile(Vector3 pos, Quaternion rot,float maxDistance, float size, float dmg, ElementType element)
    {
        foreach (var trail in trailRenderers)
        {
            if (trail != null)
            {
                trail.enabled = false;
                trail.Clear();
            }
        }

        transform.SetPositionAndRotation(pos, rot);
        _currentSize = size;
        transform.localScale = Vector3.one * _currentSize;

        _lastPosition = pos;
        _hasCollided = false;

        _maxDistance = maxDistance;
        _travelled = 0f;

        baseDamage = dmg;
        projectileElement = element;

        foreach (var trail in trailRenderers)
        {
            if (trail != null)
            {
                trail.enabled = true;
                trail.emitting = true;
            }
        }

        _ps.Clear(true);
        _ps.Play(true);
    }

    private void Update()
    {
        HandleProjMove();
    }

    private void HandleProjMove()
    {
        if (_hasCollided) return;

        Vector3 currentPos = transform.position;
        Vector3 movement = transform.forward * (speed * Time.deltaTime);
        float moveDistance = movement.magnitude;

        _travelled += moveDistance;
        if (_travelled >= _maxDistance)
        {
            transform.position = _lastPosition + transform.forward * (moveDistance - (_travelled - _maxDistance));
            OnProjectileExpiry();
            return;
        }

        if (Physics.SphereCast(_lastPosition, detectionRadius, transform.forward, out RaycastHit hit, moveDistance, hitLayers))
        {
            transform.position = hit.point;
            OnProjectileHit(hit);
        }
        else
        {
            _lastPosition = currentPos;
            transform.position += movement;
        }
    }

    private void OnProjectileExpiry()
    {
        _hasCollided = true;
        _ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

        foreach (var ps in trailParticles)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        foreach (var trail in trailRenderers)
        {
            if (trail != null)
            {
                trail.emitting = false;
            }
        }
        StartCoroutine(WaitAndRecycle());
    }

    private void OnProjectileHit(RaycastHit hit)
    {
        _hasCollided = true;

        _ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

        foreach (var ps in trailParticles)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        foreach (var trail in trailRenderers)
        {
            if (trail != null)
            {
                trail.emitting = false;
            }
        }

        IHittable hittable = hit.collider.GetComponentInParent<IHittable>();
        if (hittable == null) hittable = hit.collider.GetComponent<IHittable>();

        if (hittable != null)
        {
            float targetMultiplier = 1.0f;
            bool isElementAdvantage = false;

            CreatureController targetCreature = hit.collider.GetComponentInParent<CreatureController>();
            if (targetCreature == null) targetCreature = hit.collider.GetComponent<CreatureController>();

            if (targetCreature != null)
            {
                targetMultiplier = projectileElement.GetDamageMultiplier(targetCreature.creatureElement);
                isElementAdvantage = targetMultiplier > 1.0f;
            }

            float finalDamage = baseDamage * targetMultiplier;

            hittable.Hit(finalDamage, hit.point, isElementAdvantage);
        }

        Quaternion insertRot = Quaternion.FromToRotation(Vector3.up, -transform.forward);

        GameObject hitObj = PoolManager.Instance.proj.GetAndSetEffect(_hitKey, hit.point, insertRot);
        if (hitObj != null)
        {
            hitObj.transform.localScale = Vector3.one * _currentSize;
        }

        StartCoroutine(WaitAndRecycle());
    }

    private IEnumerator WaitAndRecycle()
    {
        yield return new WaitForSeconds(_maxTrailDuration);
            PoolManager.Instance.proj.Release(gameObject, _poolItem.key);
    }

    private float CalculateMaxTrailDuration()
    {
        float maxTime = 0f;
        foreach (var ps in trailParticles)
        {
            if (ps != null)
            {
                float t = ps.main.duration + ps.main.startLifetime.constantMax;
                if (t > maxTime) maxTime = t;
            }
        }
        return maxTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
