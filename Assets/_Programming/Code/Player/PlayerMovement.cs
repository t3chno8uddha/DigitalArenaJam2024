using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;

public class PlayerMovement : MonoBehaviour, IDamageable
{
    public ShiftData pData;
    public CharacterController pController;

    [HideInInspector] public ShiftData ogData;

    float xInput;
    public Vector3 pVelocity;

    public ShiftData selectedObject;

    MeshRenderer pRenderer;

    public Camera playerCamera;
    public Vector3 cameraOffset;
    
    public float cameraCursorDistance = 0.33f;

    public Transform cinemachineTarget;

    Vector3 viewDirection;

    float previousXInput;

    void Start()
    {
        pController = GetComponent<CharacterController>();
        ogData = pData;

        pRenderer = GetComponentInChildren<MeshRenderer>();

        InitializePlayer(ogData);
    }

    public void InitializePlayer(ShiftData data)
    {
        pRenderer.material = data.entityMaterial;
        pData = data;
    }

    void Update()
    {
        Move();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (selectedObject != null)
            {
                if (selectedObject != pData)
                {
                    InitializePlayer(selectedObject);
                }
                else
                {
                    InitializePlayer(ogData);
                }
            }
            else
            {
                InitializePlayer(ogData);
            }
        }
        if (Input.GetMouseButton(0))
        {
            if (pData is EntityData)
            {
                EntityData eData = pData as EntityData;
                
                switch (eData.attackType)
                {
                    case AttackType.assassination:
                        Assassintate(transform.position + new Vector3(0,1,0) + transform.right * previousXInput, eData.attackSize);
                        break;
                    case AttackType.melee:
                        Melee(transform.position + new Vector3(0,1,0) + transform.right * previousXInput, eData.attackSize);
                        break;
                    case AttackType.ranged:
                        Vector3 directionToTarget = transform.position - transform.position;
                        Ranged(transform.position, viewDirection, eData.projectile);
                        break;
                    case AttackType.latch:

                        break;
                }
            }
        }
    }

    void Move()
    {       
        // Get the cursor position
        viewDirection = playerCamera.ScreenToWorldPoint(new Vector3 (Input.mousePosition.x, Input.mousePosition.y, cameraCursorDistance));
        viewDirection.z = transform.position.z;
        
        // Ensure that the camera is between the cursor and the player. 0 meaning stuck at the player, 1 at the camera.
        var newCameraPosition = Vector3.Lerp(transform.position, viewDirection, cameraCursorDistance);

        cinemachineTarget.position = newCameraPosition;

        xInput = Input.GetAxisRaw("Horizontal");
        if (xInput != 0)
        {
            previousXInput = xInput;
        }

        // Ensure horizontal movement is frame-rate independent
        pVelocity.x = xInput * pData.movementSpeed;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (pData is EntityData)
            {
                EntityData eData = pData as EntityData;

                if (eData.checksGround)
                {
                    if (pController.isGrounded)
                    {
                        Jump(eData);
                    }
                }
                else
                {
                    Jump(eData);
                }
            }
        }

        if (pVelocity.y < -pData.gravityModifier)
        {
            if (pController.isGrounded)
            {
                pVelocity.y = -pData.gravityModifier;
            }
        }
    
        if (!pController.isGrounded)
        {
            // Apply gravity to the y velocity
            pVelocity.y += -pData.gravityModifier * Time.deltaTime;
        }

        // Move the character controller
        pController.Move(pVelocity * Time.deltaTime);
    }

    void Jump(EntityData eData)
    {
        pVelocity.y = eData.jumpStrength;
    }

    public void Damage()
    {
        print ("DEAT");
    }

    void Assassintate(Vector3 position, float size)
    {
        RaycastHit[] hits = Physics.SphereCastAll(position, size, Vector3.up);
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.GetComponent<EnemyObject>() != null)
            {
                IDamageable damageable = hit.transform.gameObject.GetComponent<IDamageable>();
                
                if (damageable != null)
                {
                    if (hit.transform.localScale.x == previousXInput)
                    {
                        damageable.Damage();
                    }     
                }
            }
        }
    }

    public void Melee(Vector3 position, float size)
    {
        RaycastHit hit;
        if (Physics.SphereCast(position, size, Vector3.up, out hit))
        {
            IDamageable damageable = hit.transform.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.Damage();
            }
        }
    }
    public void Ranged(Vector3 position, Vector3 angle, GameObject projectile)
    {
        Instantiate(projectile, position, Quaternion.Euler(angle));
    }

    public void ToggleLatch(EntityData eData)
    {
        eData.gravityModifier = -eData.gravityModifier;
        eData.jumpStrength = -eData.jumpStrength;
    }
}