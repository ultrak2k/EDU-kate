using UnityEngine;

public interface IDamageable
{
    enum DamageTeam
    {
        NONE,
        PLAYER,
        ENEMY
    }

    DamageTeam Team { get;}
    int Health { get;}
    void TakeDamage(int inDamage);
}
