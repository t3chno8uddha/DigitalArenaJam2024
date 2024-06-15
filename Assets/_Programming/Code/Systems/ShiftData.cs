using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Shift Data", menuName = "Data/Entity Data")]
public class ShiftData : EntityData
{
    public bool checksGround = true;

    public float jumpStrength;
    
    public UnityEvent attackEvent;
}