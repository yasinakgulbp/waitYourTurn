using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform[] spawnPoints; // Spawn noktalar�n�n listesi
    public GameObject[] enemyPrefabs; // D��man prefablar�n�n listesi
    public float spawnInterval = 5f; // Spawn aral��� (saniye cinsinden)

    void Start()
    {
        InvokeRepeating("SpawnEnemies", 0f, spawnInterval);
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // Belirli spawn noktas�ndaki d��man�n indexi, spawn noktas�n�n indexine e�it olacak �ekilde ayarlan�r
            int enemyIndex = i;

            // Belirli spawn noktas�nda belirli bir d��man tipini olu�tur
            GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], spawnPoints[i].position, Quaternion.identity);
        }
    }
}
