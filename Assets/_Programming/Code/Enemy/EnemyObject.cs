using System.Collections;
using UnityEngine;

public enum EnemyState {idle, chase}

public class EnemyObject : ShiftObject, IDamageable
{
    EnemyData eData;

    public Transform min, max;
    public Transform patrol;

    public EnemyState eState;

    [HideInInspector] public float xDirection;
    public Vector3 eVelocity;
    public CharacterController eController;

    public bool seesPlayer;

    Vector3 patrolA, patrolB, lastPlayerPosition;

    bool startedCountdown;

    void Start()
    {
        if (sData is EnemyData)
        {
            eData = sData as EnemyData;
        }
    
        patrolA = patrol.position;
        patrolB = transform.position;

        min.parent = null;
        max.parent = null;
        
        patrol.parent = null;

        eController = GetComponent<CharacterController>();

        pMovement = FindObjectOfType<PlayerMovement>();

        InitializeObject(sRenderer);
    }

    void Update()
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
        
        if (Vector3.Distance (transform.position, pMovement.transform.position) < eData.viewRange)
        {
            SearchPlayer();
        }
        else
        {
            seesPlayer = false;
        }
    }

    void Idle()
    {
        if (transform.position != patrolA)
        {
            MoveEnemy(patrolA, false);
        }
        
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
                    seesPlayer = true;
                    Debug.DrawRay(transform.position + height, directionToTarget.normalized, Color.white);
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
            return;
        }

        GetDirection(location);

        eVelocity.x = xDirection * eData.movementSpeed;

        if (eVelocity.y < -eData.gravityModifier)
        {
            if (eController.isGrounded)
            {
                eVelocity.y = -eData.gravityModifier;
            }
        }
        
        if (!eController.isGrounded)
        {
            // Apply gravity to the y velocity
            eVelocity.y += -eData.gravityModifier * Time.deltaTime;
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

    public void Damage()
    {
        if (pMovement.pData == pMovement.ogData)
        {
            pMovement.InitializePlayer(eData);
            print ("HUUUURTS");  

            Destroy(gameObject);
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
    
    public void Ranged(Vector3 position, Transform angle, GameObject projectile)
    {
        Instantiate(projectile, position, angle.rotation);
    }

    public void ToggleLatch(EntityData eData)
    {
        eData.gravityModifier = -eData.gravityModifier;
        eData.jumpStrength = -eData.jumpStrength;
    }

    public void Ranged(Vector3 position, Vector3 angle, GameObject projectile)
    {
        Instantiate(projectile, position, Quaternion.Euler(angle));
    }
}