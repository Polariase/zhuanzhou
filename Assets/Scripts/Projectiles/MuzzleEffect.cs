using MyPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleEffect : MonoBehaviour
{
    private PoolItem _poolItem;
    private ParticleSystem[] _particleSystems;
    private float _maxDuration;

    private void Awake()
    {
        _poolItem = GetComponent<PoolItem>();
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
        _maxDuration = CalculateMaxDuration();
    }

    private void OnEnable()
    {
        foreach (var ps in _particleSystems)
        {
            ps.Clear();
            ps.Play();
        }
        StartCoroutine(WaitAndRecycle());
    }

    private IEnumerator WaitAndRecycle()
    {
        yield return new WaitForSeconds(_maxDuration);
        PoolManager.Instance.proj.Release(gameObject, _poolItem.key);
    }

    private float CalculateMaxDuration()
    {
        float maxTime = 0f;
        foreach (var ps in _particleSystems)
        {
            float t = ps.main.duration + ps.main.startLifetime.constantMax;
            if (t > maxTime) maxTime = t;
        }
        return maxTime;
    }
}
