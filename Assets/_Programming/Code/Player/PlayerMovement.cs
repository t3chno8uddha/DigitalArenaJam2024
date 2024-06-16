using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour, IDamageable
{
    public ShiftData pData;
    public CharacterController pController;

    [HideInInspector] public ShiftData ogData;

    public bool immunized;
    public float immunization = 3f;

    float xInput;
    public Vector3 pVelocity;

    public ShiftData selectedObject;

    MeshRenderer pRenderer;

    public Camera playerCamera;
    public Vector3 cameraOffset;

    public bool isAttacking;
    
    public float cameraCursorDistance = 0.33f;

    public Transform cinemachineTarget;

    Vector3 viewDirection;
    
    Animator animator;
    public bool isMoving;

    float previousXInput;

    public bool isEntity;

    public Image sceneFader;
    public float sceneFadeSmoothness = 0.125f;

    bool isLoading;
    int nextScene;

    void Start()
    {
        pController = GetComponent<CharacterController>();
        ogData = pData;

        animator = GetComponent<Animator>();

        pRenderer = GetComponentInChildren<MeshRenderer>();

        InitializePlayer(ogData);

        previousXInput = transform.localScale.x;
    }

    public void InitializePlayer(ShiftData data)
    {
        pRenderer.material = data.entityMaterial;

        if (data is EntityData)
        {
            isEntity = true;
        }
        else
        {
            isEntity = false;
        }

        pData = data;
    }

    public void InitializePlayerEnemy(ShiftData data, Vector3 center, float radius, float height, float scale)
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        
        col.center = center;
        col.radius = radius;
        col.height = height;

        pController.center = center;
        pController.radius = radius;
        pController.height = height;

        pRenderer.transform.localScale = new Vector3(scale * -previousXInput, scale, scale);

        InitializePlayer(data);
    }

    void Update()
    {
        Move();
        Animate();

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
        if (Input.GetMouseButtonDown(0))
        {
            if (pData is EntityData)
            {
                StartCoroutine(Attack());
            }
        }
        
        if (isLoading)
        {
            Vector4 sceneFadeColor = sceneFader.color;
            sceneFadeColor.w = Mathf.Lerp(sceneFadeColor.w, 1, sceneFadeSmoothness * Time.deltaTime);
            
            if (sceneFadeColor.w > 0.99f)
            {
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                sceneFader.color = sceneFadeColor;
            }
        }

        if (isAttacking)
        {
            EntityData eData = pData as EntityData;
            
            switch (eData.attackType)
            {
                case AttackType.assassination:
                    Assassinate(transform.position + new Vector3(0,1,0) + transform.right * previousXInput, eData.attackSize);
                    break;
                case AttackType.melee:
                    Melee(transform.position + new Vector3(0,1,0) + transform.right * previousXInput, eData.attackSize);
                    break;
                case AttackType.ranged:
                    Ranged(transform.position, viewDirection, eData.projectile);
                    break;
                case AttackType.latch:
                    //ToggleLatch();
                    break;
            }
        }
    }
    
    IEnumerator Attack ()
    {
        isAttacking = true;

        EntityData eData = pData as EntityData;
        yield return new WaitForSeconds(eData.attackSpeed);

        isAttacking = false; 
    }

    void OnDrawGizmos()
    {
        if (Input.GetMouseButton(0))
        {
            if (pData is EntityData)
            {
                EntityData eData = pData as EntityData;
                
                // Draw a yellow sphere at the transform's position
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(transform.position + new Vector3(0,1,0)  + transform.right * previousXInput, eData.attackSize);
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
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        Vector3 lScale = transform.localScale;
        transform.localScale = new Vector3 (previousXInput, lScale.y, lScale.z);

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
        if (!immunized)
        {
            if (pData != ogData)
            {
                StartCoroutine(Immunize());
                InitializePlayer(ogData);
            }
            else
            {
                if (!isLoading)
                {
                    int currentScene = SceneManager.GetActiveScene().buildIndex;
                    LoadScene(currentScene);
                }
            }
        }
    }

    IEnumerator Immunize()
    {
        immunized = true;

        yield return new WaitForSeconds(immunization);

        immunized = false;
    }

    void Assassinate(Vector3 position, float size)
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
        RaycastHit[] hits = Physics.SphereCastAll(position, size, Vector3.up);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform != transform)
            {
                IDamageable damageable = hit.transform.gameObject.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.Damage();
                }
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

    public void Animate()
    {
        animator.SetBool("Is Grounded", pController.isGrounded);
        animator.SetBool("Is Moving", isMoving);
        animator.SetBool("Is Attacking", isAttacking);
    }

    public void LoadScene(int index)
    {
        isLoading = true;
        nextScene = index;
    }
}