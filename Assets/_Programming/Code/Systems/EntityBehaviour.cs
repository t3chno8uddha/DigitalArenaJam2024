using UnityEngine;

public class EntityBehaviour : ScriptableObject
{
    public string entityName;
    public float movementSpeed;
    public bool checksGround = true;
    public Sprite entitySprite;
}