using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

public enum EnemyState {idle, chase, lounge}

public class EnemyObject : ShiftObject
{
    EnemyData eData;

    public Transform transformA, transformB;

    public EnemyState eState;

    float xDirection;
    public Vector3 eVelocity;
    public CharacterController eController;

    public bool seesPlayer;

    Vector3 post, lastPlayerPosition;
    Vector3 minPosX, maxPosX;

    bool startedCountdown;

    void Start()
    {
        eData = sData as EnemyData;
    
        post = transform.position;

        minPosX = transformA.position;
        maxPosX = transformB.position;

        Destroy(transformA.gameObject);
        Destroy(transformB.gameObject);

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
            
            case EnemyState.lounge:
                Lounge();
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
        if (transform.position != post)
        {
            MoveEnemy(post, false);
        }
        
        if (seesPlayer)
        {
            eState = EnemyState.chase;
        }
    }

    void SearchPlayer()
    {
        Vector3 directionToTarget = pMovement.transform.position - transform.position;

        float distanceToTarget = Vector3.Distance(transform.position, pMovement.transform.position);

        // Elevate the Raycast starting position, not to shoot from the floor.
        Vector3 height = new Vector3(0, 1f, 0);

        // Limit the angles in which the character may be looking.
        if (Vector3.Angle(transform.right, directionToTarget) <= eData.sightAngle / 2)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + height, directionToTarget, out hit))
            {
                if (hit.transform == pMovement.transform)
                {                         
                    Debug.DrawRay(transform.position + height, directionToTarget.normalized * distanceToTarget, Color.white);

                    seesPlayer = true;
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
            if (!startedCountdown) { StartCoroutine (WaitForPlayer()); }
        }

        MoveEnemy(lastPlayerPosition, true);
    }

    void MoveEnemy(Vector3 location, bool attack)
    {
        float stopDistance = 0;
        
        if (attack) { stopDistance = eData.attackDistance; }
        else {stopDistance = eData.stopDistance;}

        if (Vector3.Distance(transform.position, location) < stopDistance) { return; }

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
        transform.localScale = new Vector3 (-targetX, lScale.y, lScale.z);

        xDirection = Mathf.Lerp(xDirection, targetX, eData.readjustmentSpeed);

        Debug.DrawLine(targetDirection, targetDirection + Vector3.up * 2);
    }

    void Lounge()
    {

    }
}
