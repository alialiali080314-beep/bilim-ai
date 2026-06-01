using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage, Vector3 direction);
    bool IsDead { get; }
}
