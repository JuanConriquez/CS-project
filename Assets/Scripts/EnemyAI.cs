using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player; //Reference to player

    [Header("Detection")]
    public float detectionRadius = 10f; //Detection radius, how close the player must be to the enemy before chasing
    public float loseSightRadius = 15f; //Distance from player where enemy stops chasing the player

    [Header("Movement")]
    public float patrolSpeed = 2f; //Speed of enemy when not chaseing (just patrolling)
    public float chaseSpeed = 3.5f; //Enemy speed when chasing
    public float rotationSpeed = 5f; //How quickly the enemy turns to face the player

    [Header("Roaming")]
    public Transform[] patrolPoints; //Points the enemy will patrol between when not chasing the player
    public bool randomPatrol = false; //Whether the enemy patrols randomly or in order
    public float waitTimeAtWaypoint = 2f; //Time the enemy waits at each waypoint before moving to the next one
    public float waypointTolerance = 0.5f; //Distance to waypoint before considered "arrived"

    private CharacterController controller;

    // Two enemy states: Roam and Chasing

    private enum State { Roam, Chasing }
    private State currentState = State.Roam;

    [Header("Door Breaking")]
    public float doorCheckDistance = 1.5f;
    public float doorDamagePerSecond = 25f;

    private Vector3 moveDirection = Vector3.zero;

    private Animator animator;

    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("EnemeyAI: No player assigned and no object with tag 'Player' found.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Roam:
                HandleRoam(distanceToPlayer);
                break;

            case State.Chasing:
                HandleChase(distanceToPlayer);
                break;
        }

        controller.SimpleMove(moveDirection);
    }

    // HandleIdle()
    // Function to implement Enemy Idle behavior, where the enemy is simply not chasing the player 
    // Patrolling will be implemented as well (later)
    void HandleRoam(float distanceToPlayer)
    {
        animator.SetBool("isChasing", false);

        // Patrol logic only if patrol points exist
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Patrol();
        }


        else
        {
            moveDirection = Vector3.zero;
            animator.SetBool("isRoaming", false);
            animator.SetBool("isLooking", true);
        }

        

        if (distanceToPlayer <= detectionRadius) //Once the player enters the detection radius of the enemy...
        {
            currentState = State.Chasing; //... start chasing (enter chasing state)
        }
    }

    void Patrol()
    {
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector3 targetPos = targetPoint.position;
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;

        float distanceToWaypoint = toTarget.magnitude;

        // Arrived at waypoint
        if (distanceToWaypoint <= waypointTolerance)
        {
            moveDirection = Vector3.zero;
            animator.SetBool("isRoaming", false);
            animator.SetBool("isLooking", true);

            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                ChooseNextPatrolPoint();
                waitTimer = waitTimeAtWaypoint;
            }
            return;
        }

        // Move toward next waypoint
        Vector3 dir = toTarget.normalized;
        moveDirection = dir * patrolSpeed;

        animator.SetBool("isRoaming", true);
        animator.SetBool("isLooking", false);

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    void ChooseNextPatrolPoint()
    {
        if (patrolPoints.Length <= 1) return;

        if (randomPatrol)
        {
            int next = currentPatrolIndex;
            while (next == currentPatrolIndex && patrolPoints.Length > 1)
            {
                next = Random.Range(0, patrolPoints.Length);
            }
            currentPatrolIndex = next;
        }
        else
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    // HandleChase()
    // Function to implement Enemy Chase behavior, where the enemy simply follows the player
    void HandleChase(float distanceToPlayer)
    {
        animator.SetBool("isRoaming", false);
        animator.SetBool("isChasing", true);
        animator.SetBool("isIdle", false);

        Vector3 direction = (player.position - transform.position); //Rotate the enemy to face the player more directly
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            direction.Normalize();

            if (CheckAndDamageDoor(direction)) //Check if enemy is in front of door
            {
                moveDirection = Vector3.zero; //Don't move when at door
            } else
            {
                moveDirection = direction * chaseSpeed;
            }

            //moveDirection = direction * chaseSpeed;

            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        if (distanceToPlayer > loseSightRadius) //If player is far enough...
        {
            currentState = State.Roam; //...stop chasing and enter Idle state
        }
    }

    private bool CheckAndDamageDoor(Vector3 forwardDir)
    {
        float doorCheckDistance = 1.5f;
        float doorDamagePerSecond = 25f;

        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;

        if(Physics.Raycast(rayOrigin, forwardDir, out RaycastHit hit, doorCheckDistance))
        {
            Door door = hit.collider.GetComponent<Door>();
            if (door != null && !door.isBroken)
            {
                door.TakeDamage(doorDamagePerSecond * Time.deltaTime);
                return true;
            }
        }
        return false;

    }
}

