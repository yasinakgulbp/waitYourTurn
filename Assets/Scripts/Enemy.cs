using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public Transform goInsideTarget;
    public float moveSpeed = 5f;
    public float playerDetectionRadius = 10f;
    public float doorDetectionRadius = 10f;
    public float playerStopDistance = 1f;
    public float doorStopDistance = 1f;
    public LayerMask doorLayer;

    private bool doorExists = true;
    private enum EnemyState { Patrol, Chase, Attack, GoInside };
    private EnemyState currentState = EnemyState.Patrol;
    private float attackTimer = 0f;
    private float attackCooldown = 3f;
    private float rotationSpeed;

    private void Start()
    {
        Application.targetFrameRate = 200;
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.GoInside:
                GoInside();
                break;
        }
    }

    void Patrol()
    {
        if (doorExists)
        {
            MoveTowardsDoor();
        }
        else
        {
            currentState = EnemyState.Chase;
        }
    }

    void MoveTowardsDoor()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, doorDetectionRadius, doorLayer);

        if (colliders.Length > 0)
        {
            Vector3 direction = (colliders[0].transform.position - transform.position).normalized;
            float distanceToDoor = Vector3.Distance(transform.position, colliders[0].transform.position);

            if (distanceToDoor > doorStopDistance)
            {
                transform.Translate(direction * moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            doorExists = false;
            currentState = EnemyState.GoInside;
        }
    }

    void Chase()
    {
        // Calculate direction towards the player only along the x and z axes
        Vector3 direction = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position).normalized;

        // Move towards the player along the x and z axes
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        // Calculate the rotation needed to face the player only in the horizontal plane
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        // Apply the rotation only in the horizontal plane
        transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

        // Check if within attack range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= playerStopDistance)
        {
            currentState = EnemyState.Attack;
        }
    }




    void Attack()
    {
        if (attackTimer >= attackCooldown)
        {
            Debug.Log("Attack");
            attackTimer = 0f;
        }
        else
        {
            attackTimer += Time.deltaTime;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > playerStopDistance)
        {
            currentState = EnemyState.Chase;
        }
    }

    void GoInside()
    {
        Vector3 direction = (goInsideTarget.position - transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);

        float distanceToTarget = Vector3.Distance(transform.position, goInsideTarget.position);
        if (distanceToTarget < 0.1f)
        {
            currentState = EnemyState.Chase;
        }
    }
}
