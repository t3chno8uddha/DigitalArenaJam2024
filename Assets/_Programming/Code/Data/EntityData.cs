using UnityEngine;

[CreateAssetMenu(fileName = "New Shift Data", menuName = "Data/Entity Data")]
public class EntityData : ShiftData
{
    public bool checksGround = true;

    public float jumpStrength;
    
    public AttackType attackType;

    public bool ignoreAngle = false;

    public float attackSpeed = 0.5f;

    public float attackSize;
    public float attackHeight;
    public GameObject projectile;
}