using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Shift Data", menuName = "Data/Entity Data")]
public class EntityData : ShiftData
{
    public bool checksGround = true;

    public float jumpStrength;
    
    public UnityEvent attackEvent;
}