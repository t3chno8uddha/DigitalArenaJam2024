using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public ShiftData pData;
    public CharacterController pController;

    ShiftData ogData;

    float xInput;
    public Vector3 pVelocity;

    public ShiftData selectedObject;

    MeshRenderer pRenderer;

    void Start()
    {
        pController = GetComponent<CharacterController>();
        ogData = pData;

        pRenderer = GetComponentInChildren<MeshRenderer>();

        InitializePlayer(ogData);
    }

    void InitializePlayer(ShiftData data)
    {
        pRenderer.material = data.entityMaterial;
        pData = data;
    }

    void Update()
    {
        Move();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedObject != null)
            {
                InitializePlayer(selectedObject);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            InitializePlayer(ogData);
        }
    }

    void Move()
    {       
        xInput = Input.GetAxisRaw("Horizontal");

        // Ensure horizontal movement is frame-rate independent
        pVelocity.x = xInput * pData.movementSpeed;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (pData.checksGround)
            {
                if (pController.isGrounded)
                {
                    Jump();
                }
            }
            else
            {
                Jump();
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

    void Jump()
    {
        pVelocity.y = pData.jumpStrength;
    }

}