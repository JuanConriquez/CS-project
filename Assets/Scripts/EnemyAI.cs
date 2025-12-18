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

    [Header("Attack")]
    public int damagePerHit = 10;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f; // seconds between hits

    // 🔊 ADD — Footstep audio settings
    [Header("Audio - Footsteps")]
    public AudioClip[] footstepClips;
    [Range(0f, 1f)] public float footstepVolume = 0.8f;
    public float baseStepInterval = 0.5f;   // seconds between steps at patrol speed
    public float minStepInterval = 0.25f;   // faster cap (chasing)
    public float stepMoveThreshold = 0.2f;  // how fast enemy must be moving to play steps
    private AudioSource footstepSource;
    private float stepTimer = 0f;

    private float attackTimer = 0f;
    private PlayerHealth playerHealth;

    private CharacterController controller;

    // Two enemy states: Roam and Chasing

    private enum State { Roam, Chasing }
    private State currentState = State.Roam;
    private Vector3 moveDirection = Vector3.zero;

    private Animator animator;

    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();

        playerHealth = player.GetComponent<PlayerHealth>();

        // setup AudioSource for footsteps
        footstepSource = GetComponent<AudioSource>();
        if (footstepSource == null) footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.spatialBlend = 1f; // 3D footsteps

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
        attackTimer -= Time.deltaTime;

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

        // 🔊 ADD — footstep update AFTER movement is applied
        HandleFootsteps();
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

        Vector3 direction = (player.position - transform.position); //Rotate the enemy to face the player more directly
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            direction.Normalize();
            moveDirection = direction * chaseSpeed;

            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        } else
        {
            moveDirection = Vector3.zero;
        }

        if (distanceToPlayer <= attackRange)
        {
            TryAttack();
        }

        if (distanceToPlayer > loseSightRadius) //If player is far enough...
        {
            currentState = State.Roam; //...stop chasing and enter Idle state
        }
    }

    void TryAttack()
    {
        if (attackTimer > 0f) return;
        if (playerHealth == null) return;

        attackTimer = attackCooldown;

        // Optional: play attack animation
        //animator.SetTrigger("attack");

        // Actually deal damage
        playerHealth.TakeDamage(damagePerHit);
    }

    // 🔊 ADD — Footstep logic
    void HandleFootsteps()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;
        if (controller == null) return;

        // actual movement speed from CharacterController
        float speed = controller.velocity.magnitude;

        // basic "is moving" check
        bool isMoving = speed > stepMoveThreshold;

        if (!isMoving)
        {
            stepTimer = 0f;
            return;
        }

        // Step timing scales: patrol = slower, chase = faster
        float targetSpeed = (currentState == State.Chasing) ? chaseSpeed : patrolSpeed;
        float t = (targetSpeed <= 0.01f) ? 1f : Mathf.Clamp01(speed / targetSpeed);

        float interval = Mathf.Lerp(baseStepInterval, minStepInterval, t);

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            if (clip != null)
                footstepSource.PlayOneShot(clip, footstepVolume);

            stepTimer = interval;
        }
    }
}