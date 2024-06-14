using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Entity Data/Player")]
public class PlayerData : EntityBehaviour
{
    public float gravityModifier = 7;
    public float jumpStrength;
    public UnityEvent attackEvent;
}