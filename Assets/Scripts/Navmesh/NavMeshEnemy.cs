using UnityEngine;
using UnityEngine.AI;

public class NavMeshEnemy : MonoBehaviour
{
    public Transform player; // Oyuncunun pozisyonunu alacak transform
    private NavMeshAgent navMeshAgent; // NavMeshAgent bile�enine eri�im i�in referans

    void Start()
    {
        // NavMeshAgent bile�enini al
        navMeshAgent = GetComponent<NavMeshAgent>();

        // E�er oyuncu referans� atanmad�ysa, "player" ad�nda bir GameObject arar ve onun transformunu kullan�r
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        // Oyuncunun pozisyonuna do�ru hareket et
        if (player != null)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }
}
