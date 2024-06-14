using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows.WebCam;

public class PlayerMovement : MonoBehaviour
{
    public PlayerData pData;
    public CharacterController pController;

    float xInput;
    public Vector3 pVelocity;

    void Start()
    {
        pController = GetComponent<CharacterController>();

        InitializePlayer();
    }

    void InitializePlayer()
    {
        //
    }
    void Update()
    {
        Move();
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