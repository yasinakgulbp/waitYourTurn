//using UnityEngine;

//public class Enemy : MonoBehaviour
//{
//    public Transform player;
//    public Transform goInsideTarget;
//    public float moveSpeed = 5f;
//    public float playerDetectionRadius = 10f;
//    public float doorDetectionRadius = 10f;
//    public float playerStopDistance = 1f;
//    public float doorStopDistance = 1f;
//    public LayerMask doorLayer;
//    public string Hedefismi = "Target2";

//    private bool doorExists = true;
//    private enum EnemyState { Patrol, Chase, Attack, GoInside };
//    private EnemyState currentState = EnemyState.Patrol;
//    private float attackTimer = 0f;
//    private float attackCooldown = 3f;
//    private float rotationSpeed;

//    private void Start()
//    {
//        Application.targetFrameRate = 200;

//        // Player ve goInsideTarget objelerini bul ve referanslarýný al
//        player = GameObject.FindGameObjectWithTag("Player").transform;
//        goInsideTarget = GameObject.FindGameObjectWithTag(Hedefismi).transform;

//        if (player == null)
//        {
//            Debug.LogError("Player object not found!");
//        }

//        if (goInsideTarget == null)
//        {
//            Debug.LogError("GoInsideTarget object not found!");
//        }
//    }


//    void Update()
//    {
//        switch (currentState)
//        {
//            case EnemyState.Patrol:
//                Patrol();
//                break;
//            case EnemyState.Chase:
//                Chase();
//                break;
//            case EnemyState.Attack:
//                Attack();
//                break;
//            case EnemyState.GoInside:
//                GoInside();
//                break;
//        }
//    }

//    void Patrol()
//    {
//        if (doorExists)
//        {
//            //MoveTowardsDoor();
//            GoInside();
//        }
//        else
//        {
//            currentState = EnemyState.Chase;
//        }
//    }

//    void MoveTowardsDoor()
//    {
//        Collider[] colliders = Physics.OverlapSphere(transform.position, doorDetectionRadius, doorLayer);

//        if (colliders.Length > 0)
//        {
//            Vector3 direction = (colliders[0].transform.position - transform.position).normalized;
//            float distanceToDoor = Vector3.Distance(transform.position, colliders[0].transform.position);

//            if (distanceToDoor > doorStopDistance)
//            {
//                transform.Translate(direction * moveSpeed * Time.deltaTime);
//            }
//        }
//        else
//        {
//            doorExists = false;
//            currentState = EnemyState.GoInside;
//        }
//    }

//    void Chase()
//    {
//        // Calculate direction towards the player only along the x and z axes
//        Vector3 direction = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position).normalized;

//        // Move towards the player along the x and z axes
//        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

//        // Calculate the rotation needed to face the player only in the horizontal plane
//        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

//        // Apply the rotation only in the horizontal plane
//        transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

//        // Check if within attack range
//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
//        if (distanceToPlayer <= playerStopDistance)
//        {
//            currentState = EnemyState.Attack;
//        }
//    }




//    void Attack()
//    {
//        if (attackTimer >= attackCooldown)
//        {
//            Debug.Log("Attack");
//            attackTimer = 0f;
//        }
//        else
//        {
//            attackTimer += Time.deltaTime;
//        }

//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

//        if (distanceToPlayer > playerStopDistance)
//        {
//            currentState = EnemyState.Chase;
//        }
//    }

//    void GoInside()
//    {
//        Vector3 direction = (goInsideTarget.position - transform.position).normalized;

//        // Karakterin ileri doðru yürümesi için global uzayda ilerletme
//        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

//        // Düþmanýn yönünü yürüdüðü yöne doðru bakacak þekilde ayarla
//        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
//        transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

//        float distanceToTarget = Vector3.Distance(transform.position, goInsideTarget.position);
//        if (distanceToTarget < 0.1f)
//        {
//            currentState = EnemyState.Chase;
//        }
//    }

//}





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
    public string Hedefismi = "Target2";

    [Header("Ses Ayarlarý")]
    public AudioSource audioSource;      // Sesleri çalmak için kullanýlacak AudioSource
    public AudioClip[] spawnSounds;      // Doðduðunda rastgele çalýnacak seslerin listesi

    private bool doorExists = true;
    private enum EnemyState { Patrol, Chase, Attack, GoInside };
    private EnemyState currentState = EnemyState.Patrol;
    private float attackTimer = 0f;
    private float attackCooldown = 3f;

    private void Start()
    {
        Application.targetFrameRate = 200;

        // Player ve goInsideTarget objelerini bul
        player = GameObject.FindGameObjectWithTag("Player").transform;
        goInsideTarget = GameObject.FindGameObjectWithTag(Hedefismi).transform;

        if (player == null)
        {
            Debug.LogError("Player object not found!");
        }

        if (goInsideTarget == null)
        {
            Debug.LogError("GoInsideTarget object not found!");
        }

        // Rastgele bir doðuþ sesi çal
        PlayRandomSpawnSound();
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
            GoInside();
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
                transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
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
        Vector3 direction = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position).normalized;

        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

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
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

        float distanceToTarget = Vector3.Distance(transform.position, goInsideTarget.position);
        if (distanceToTarget < 0.1f)
        {
            currentState = EnemyState.Chase;
        }
    }

    void PlayRandomSpawnSound()
    {
        if (spawnSounds != null && spawnSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, spawnSounds.Length);
            audioSource.PlayOneShot(spawnSounds[randomIndex]);
        }
    }
}
