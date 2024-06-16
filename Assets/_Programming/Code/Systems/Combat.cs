using UnityEngine;

public interface IDamageable 
{
    void Damage ();
    void Melee (Vector3 position, float size);
    void Ranged (Vector3 position, Vector3 angle, GameObject projectile);
    void ToggleLatch (EntityData eData);

    void Animate();
}

public enum AttackType { assassination, melee, ranged, latch }