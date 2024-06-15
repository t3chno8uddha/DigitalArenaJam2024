using UnityEngine;

[CreateAssetMenu(fileName = "New Shift Data", menuName = "Data/Enemy Data")]
public class EnemyData : ShiftData
{
    public float viewRange = 4f;
    public float patience = 5f;

    public float stopDistance = 1f, attackDistance = 2f;
    [Range (0,1)]
    public float readjustmentSpeed = 0.125f; 
    public float sightAngle = 90f;
}