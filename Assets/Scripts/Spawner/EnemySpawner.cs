using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform[] spawnPoints; // Spawn noktalarýnýn listesi
    public GameObject[] enemyPrefabs; // Düþman prefablarýnýn listesi
    public float spawnInterval = 5f; // Spawn aralýðý (saniye cinsinden)

    void Start()
    {
        InvokeRepeating("SpawnEnemies", 0f, spawnInterval);
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // Belirli spawn noktasýndaki düþmanýn indexi, spawn noktasýnýn indexine eþit olacak þekilde ayarlanýr
            int enemyIndex = i;

            // Belirli spawn noktasýnda belirli bir düþman tipini oluþtur
            GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], spawnPoints[i].position, Quaternion.identity);
        }
    }
}
