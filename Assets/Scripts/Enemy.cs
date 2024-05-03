using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player; // Player'ý hedef olarak belirleyeceðimiz transform
    public float moveSpeed = 5f; // Enemy'nin hareket hýzý
    public float playerDetectionRadius = 10f; // Oyuncu algýlama yarýçapý
    public float doorDetectionRadius = 10f; // Kapý algýlama yarýçapý
    public float playerStopDistance = 1f; // Oyuncuya yaklaþma mesafesi
    public float doorStopDistance = 1f; // Kapýya yaklaþma mesafesi
    public LayerMask doorLayer; // Door objesinin layer'ý

    private bool doorExists = true; // Door'un varlýðýný saklamak için bir deðiþken

    enum EnemyState { Patrol, Chase }; // Enemy'nin durumlarý
    private EnemyState currentState = EnemyState.Patrol; // Baþlangýçta Patrol durumunda baþlayacak

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
        }
    }

    void Patrol()
    {
        // Eðer kapý hala varsa, kapýya doðru ilerle
        if (doorExists)
        {
            MoveTowardsDoor();
        }
        else
        {
            // Kapý yoksa, oyuncuya doðru ilerle
            currentState = EnemyState.Chase;
        }
    }

    void MoveTowardsDoor()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, doorDetectionRadius, doorLayer); // Kapýlarý algýla

        // Eðer kapý varsa, kapýya doðru hareket et
        if (colliders.Length > 0)
        {
            Vector3 direction = (colliders[0].transform.position - transform.position).normalized;
            float distanceToDoor = Vector3.Distance(transform.position, colliders[0].transform.position);

            // Kapýya yaklaþma mesafesine geldiðinde dur
            if (distanceToDoor > doorStopDistance)
            {
                transform.Translate(direction * moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Kapý yoksa, kapýnýn varlýðýný güncelle ve Patrol durumuna geç
            doorExists = false;
            currentState = EnemyState.Patrol;
        }
    }

    void Chase()
    {
        // Oyuncuya doðru hareket et
        Vector3 direction = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Oyuncuya yaklaþma mesafesine geldiðinde dur
        if (distanceToPlayer > playerStopDistance)
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }
}
