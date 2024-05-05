using UnityEngine;
using UnityEngine.AI;

public class NavMeshEnemy : MonoBehaviour
{
    public Transform player; // Oyuncunun pozisyonunu alacak transform
    private NavMeshAgent navMeshAgent; // NavMeshAgent bileþenine eriþim için referans

    void Start()
    {
        // NavMeshAgent bileþenini al
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Eðer oyuncu referansý atanmadýysa, "player" adýnda bir GameObject arar ve onun transformunu kullanýr
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        // Oyuncunun pozisyonuna doðru hareket et
        if (player != null)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }
}
