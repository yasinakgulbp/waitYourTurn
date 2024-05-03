using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player; // Player'� hedef olarak belirleyece�imiz transform
    public float moveSpeed = 5f; // Enemy'nin hareket h�z�
    public float playerDetectionRadius = 10f; // Oyuncu alg�lama yar��ap�
    public float doorDetectionRadius = 10f; // Kap� alg�lama yar��ap�
    public float playerStopDistance = 1f; // Oyuncuya yakla�ma mesafesi
    public float doorStopDistance = 1f; // Kap�ya yakla�ma mesafesi
    public LayerMask doorLayer; // Door objesinin layer'�

    private bool doorExists = true; // Door'un varl���n� saklamak i�in bir de�i�ken

    enum EnemyState { Patrol, Chase }; // Enemy'nin durumlar�
    private EnemyState currentState = EnemyState.Patrol; // Ba�lang��ta Patrol durumunda ba�layacak

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
        // E�er kap� hala varsa, kap�ya do�ru ilerle
        if (doorExists)
        {
            MoveTowardsDoor();
        }
        else
        {
            // Kap� yoksa, oyuncuya do�ru ilerle
            currentState = EnemyState.Chase;
        }
    }

    void MoveTowardsDoor()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, doorDetectionRadius, doorLayer); // Kap�lar� alg�la

        // E�er kap� varsa, kap�ya do�ru hareket et
        if (colliders.Length > 0)
        {
            Vector3 direction = (colliders[0].transform.position - transform.position).normalized;
            float distanceToDoor = Vector3.Distance(transform.position, colliders[0].transform.position);

            // Kap�ya yakla�ma mesafesine geldi�inde dur
            if (distanceToDoor > doorStopDistance)
            {
                transform.Translate(direction * moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Kap� yoksa, kap�n�n varl���n� g�ncelle ve Patrol durumuna ge�
            doorExists = false;
            currentState = EnemyState.Patrol;
        }
    }

    void Chase()
    {
        // Oyuncuya do�ru hareket et
        Vector3 direction = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Oyuncuya yakla�ma mesafesine geldi�inde dur
        if (distanceToPlayer > playerStopDistance)
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }
}
