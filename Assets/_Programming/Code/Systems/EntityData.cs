using UnityEngine;

[CreateAssetMenu(fileName = "New Shift Data", menuName = "Data/Entity Data")]
public class EntityData : ShiftData
{
    public bool checksGround = true;

    public float jumpStrength;
    
    public Vector3 center = new Vector3(0, 1, 0);
    public Vector2 radiusAndHeight = new Vector2(0.5f, 2f);

    public AttackType attackType;

    public float attackSize;
    public GameObject projectile;
}