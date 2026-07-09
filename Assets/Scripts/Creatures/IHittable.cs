using UnityEngine;

public interface IHittable
{
    bool Hit(float damage, Vector3 hitPoint, bool isCrit);

    bool TakeDamage(float damage, Vector3 hitPoint, bool isCrit);

    Vector3 HitPoint();
}