using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;

public enum EnemyState {idle, chase}

public class EnemyObject : ShiftObject, IDamageable
{
    public EnemyData eData;

    public Transform min, max;
    public Transform patrol;

    public EnemyState eState;

    [HideInInspector] public float xDirection;
    public Vector3 eVelocity = new Vector3(0,-100,0);
    public CharacterController eController;

    public bool seesPlayer;
    public bool isAttacking;

    int gravityDir = -1;

    bool hasFired;

    Renderer eRenderer;

    Vector3 patrolA, patrolB, lastPlayerPosition;

    bool isMoving;
    bool startedCountdown;
    Animator animator;

    void Start()
    {
        if (sData is EnemyData)
        {
            eData = sData as EnemyData;
        }
    
        patrolA = patrol.position;
        patrolB = transform.position;

        eRenderer = GetComponentInChildren<Renderer>();

        min.parent = null;
        max.parent = null;
        
        patrol.parent = null;

        eController = GetComponent<CharacterController>();

        pMovement = FindObjectOfType<PlayerMovement>();

        InitializeObject(sRenderer);
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isAttacking && !hasFired)
        {
            CheckAttack();
        }
        else
        {
            switch (eState)
            {
                case EnemyState.idle:
                    Idle();
                    break;
                
                case EnemyState.chase:
                    Chase();
                    break;
            }
        }
        
        if (Vector3.Distance (transform.position, pMovement.transform.position) < eData.viewRange)
        {
            SearchPlayer();
        }
        else
        {
            seesPlayer = false;
        }

        Animate();
    }
    
    void CheckAttack()
    {
        Vector3 attackPosition = transform.position + new Vector3(0,eData.attackHeight,0) + transform.right * xDirection;

        switch (eData.attackType)
        {
            case AttackType.melee:
                Melee(attackPosition, eData.attackSize);
                break;
            case AttackType.ranged:
                Ranged(attackPosition, eData.projectile);
                break;
        }   
    }

    void Idle()
    {
        MoveEnemy(patrolA, false);

        if (seesPlayer)
        {
            eState = EnemyState.chase;
        }
    }

    void SearchPlayer()
    {
        Vector3 directionToTarget = pMovement.transform.position - transform.position;

        // Elevate the Raycast starting position, not to shoot from the floor.
        Vector3 height = new Vector3(0, 1f, 0);
        
        Vector3 newPosition = transform.position - transform.right * transform.localScale.x;

        float targetDirection = pMovement.transform.position.x - newPosition.x;

        Debug.DrawRay(transform.position - transform.right * transform.localScale.x, transform.up * 2, Color.white);

        if (Mathf.Sign(targetDirection) == Mathf.Sign(transform.localScale.x))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + height, directionToTarget.normalized, out hit))
            {
                if (hit.transform == pMovement.transform)
                {
                    if (pMovement.isEntity)
                    {
                        seesPlayer = true;
                        Debug.DrawRay(transform.position + height, directionToTarget.normalized, Color.white);
                    }
                    else
                    {
                        switch (eState)
                        {
                            case EnemyState.idle:
                            seesPlayer = false;
                                break;

                            case EnemyState.chase:
                            seesPlayer = true;
                                break;
                        }
                    }
                }
                else
                {
                    seesPlayer = false;
                }
            }
        }
    }

    void Chase()
    {
        if (seesPlayer)
        {
            lastPlayerPosition = pMovement.transform.position;
        }
        else
        {
            if (!startedCountdown)
            {
                StartCoroutine (WaitForPlayer());
                return;
            }
        }

        MoveEnemy(lastPlayerPosition, true);
    }

    void MoveEnemy(Vector3 location, bool attack)
    {
        float stopDistance = 0;
        
        if (attack) { stopDistance = eData.attackDistance; }
        else {stopDistance = eData.stopDistance;}

        if (Mathf.Abs(transform.position.x - location.x) < stopDistance)
        {
            if (eState == EnemyState.idle)
            {
                if (!startedCountdown) { StartCoroutine(PatrolCountdown()); }
            }

            isMoving = false;

            if (attack)
            {
                if (!isAttacking)
                {
                    StartCoroutine(AttackPlayer());
                }
            }
            return;
        }
        else
        {
            isMoving = true;
        }

        GetDirection(location);

        eVelocity.x = xDirection * eData.movementSpeed;

        if (eVelocity.y < eData.gravityModifier * gravityDir)
        {
            if (eController.isGrounded)
            {
                eVelocity.y = eData.gravityModifier * gravityDir;
            }
        }
        
        if (!eController.isGrounded)
        {
            // Apply gravity to the y velocity
            eVelocity.y += eData.gravityModifier * gravityDir * Time.deltaTime;
        }

        // Move the character controller
        eController.Move(eVelocity * Time.deltaTime);

        if (transform.position.x > max.position.x) { transform.position = new Vector3 (max.position.x, transform.position.y, transform.position.z); }
        if (transform.position.x < min.position.x) { transform.position = new Vector3 (min.position.x, transform.position.y, transform.position.z); }
        
        if (transform.position.y > max.position.y) { transform.position = new Vector3 (transform.position.x, max.position.y, transform.position.z); }
        if (transform.position.y < min.position.y) { transform.position = new Vector3 (transform.position.x, min.position.y, transform.position.z); }
    }

    IEnumerator WaitForPlayer()
    {
        startedCountdown = true;

        yield return new WaitForSeconds (eData.patience);

        startedCountdown = false;
        eState = EnemyState.idle;
    }

    IEnumerator AttackPlayer ()
    {
        isAttacking = true;

        yield return new WaitForSeconds(eData.attackSpeed);

        isAttacking = false; 
        hasFired = false;
    }

    void GetDirection(Vector3 targetDirection)
    {
        float targetX = 0;

        if (targetDirection.x > transform.position.x)
        {
            targetX = 1f;
        }
        else if (targetDirection.x < transform.position.x)
        {
            targetX = -1f;
        }

        Vector3 lScale = transform.localScale;
        transform.localScale = new Vector3 (targetX, lScale.y, lScale.z);

        xDirection = Mathf.Lerp(xDirection, targetX, eData.readjustmentSpeed);

        Debug.DrawLine(targetDirection, targetDirection + Vector3.up * 2);
    }

    IEnumerator PatrolCountdown ()
    {
        startedCountdown = true;

        yield return new WaitForSeconds (eData.patience);

        Vector3 oldPatrol = patrolA;

        patrolA = patrolB;
        patrolB = oldPatrol;

        startedCountdown = false;
    }

    public void Damage(bool shift)
    {
        if (shift)
        {
            Vector3 center = eController.center;
            float radius = eController.radius;
            float height = eController.height;

            float scale = eRenderer.transform.localScale.y;

            pMovement.InitializePlayerEnemy(eData, center, radius, height, scale);
        }
        
        Destroy(gameObject);
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
                    damageable.Damage(false);
                }
            }
        }
    }
    
    public void Ranged(Vector3 position, GameObject projectile)
    {
        if (!hasFired)
        {
            hasFired = true;

            Vector3 viewDir = pMovement.transform.position + new Vector3(0, eData.attackHeight/2, 0) - position;
            Quaternion rotation = Quaternion.LookRotation(viewDir);
            
            Projectile proj = Instantiate(projectile, position, rotation).GetComponent<Projectile>();
            proj.progenitor = gameObject;
        }
    }

    public void ToggleLatch()
    {
    }
    
    public void Animate()
    {
        if (eData.checksGround)
        {
            animator.SetBool("Is Grounded", eController.isGrounded);
            animator.SetBool("Is Moving", isMoving);
        }
        else
        {
            animator.SetBool("Is Grounded", true);
            animator.SetBool("Is Moving", true);
        }
        animator.SetBool("Is Attacking", isAttacking);
    }
}