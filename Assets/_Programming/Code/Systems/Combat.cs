using UnityEngine;

public interface IDamageable 
{
    void Damage (bool shift);
    void Melee (Vector3 position, float size);
    void Ranged (Vector3 position, GameObject projectile);
    void ToggleLatch ();

    void Animate();
}

public enum AttackType { assassination, melee, ranged, latch }