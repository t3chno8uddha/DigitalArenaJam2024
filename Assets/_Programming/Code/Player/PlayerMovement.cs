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

    public Camera playerCamera;
    public Vector3 cameraOffset;
    
    public float cameraCursorDistance = 0.33f;

    public Transform cinemachineTarget;

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
        // Get the cursor position
        Vector3 viewDirection = playerCamera.ScreenToWorldPoint(new Vector3 (Input.mousePosition.x, Input.mousePosition.y, cameraCursorDistance));
        viewDirection.z = transform.position.z;
        
        // Ensure that the camera is between the cursor and the player. 0 meaning stuck at the player, 1 at the camera.
        var newCameraPosition = Vector3.Lerp(transform.position, viewDirection, cameraCursorDistance);
        newCameraPosition += cameraOffset;

        cinemachineTarget.position = newCameraPosition;


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