using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Shift Data", menuName = "Data/Shift Data")]
public class ShiftData : ScriptableObject
{
    public Material entityMaterial;
    
    public float shiftDistance = 4f;

    public float movementSpeed;

    public float gravityModifier = 7;
}