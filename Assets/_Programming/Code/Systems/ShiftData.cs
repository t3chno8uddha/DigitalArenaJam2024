using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Data")]
public class ShiftData : ScriptableObject
{
    public Material entityMaterial;
    
    public float shiftDistance = 4f;

    public float movementSpeed;
    public bool checksGround = true;

    public float gravityModifier = 7;
    public float jumpStrength;
    
    public UnityEvent attackEvent;
}