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

    private NavMeshAgent agent;

    // Two enemy states: Idle and Chasing
    private enum State { Idle, Chasing }
    private State currentState = State.Idle;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

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
            case State.Idle:
                HandleIdle(distanceToPlayer);
                break;

            case State.Chasing:
                HandleChase(distanceToPlayer);
                break;
        }
    }

    // HandleIdle()
    // Function to implement Enemy Idle behavior, where the enemy is simply not chasing the player 
    // Patrolling will be implemented as well (later)
    void HandleIdle(float distanceToPlayer)
    {
        //Just have the enemy stand still for now
        agent.speed = patrolSpeed;
        agent.ResetPath();

        // Patrol handling...

        if (distanceToPlayer <= detectionRadius) //Once the player enters the detection radius of the enemy...
        {
            currentState = State.Chasing; //... start chasing (enter chasing state)
        }
    }

    // HandleChase()
    // Function to implement Enemy Chase behavior, where the enemy simply follows the player
    void HandleChase(float distanceToPlayer)
    {
        agent.speed = chaseSpeed;

        agent.SetDestination(player.position); //Makes the enemy move towards the player

        Vector3 direction = (player.position - transform.position).normalized; //Rotate the enemy to face the player more directly
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }

        if (distanceToPlayer > loseSightRadius) //If player is far enough...
        {
            currentState = State.Idle; //...stop chasing and enter Idle state
        }
    }
}
