using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviour
{
    public Transform[] spawnPoints; // spawn noktalarý
    public GameObject playerPrefab; // Player objesinin prefab'i
    public GameObject spawnPointsObject; // spawn noktalarýný tutan obje
    public TMP_Text countdownText; // geri sayým metni
    public float countdownDuration = 60f; // geri sayým süresi
    private float countdownTimer; // geri sayým zamanlayýcýsý

    void Start()
    {
        // Oyuncuyu spawn et
        SpawnPlayer();

        countdownTimer = countdownDuration;
        UpdateCountdownText();
    }

    void Update()
    {
        countdownTimer -= Time.deltaTime;

        if (countdownTimer <= 0f)
        {
            // Oyunu yeniden baþlat
            ReloadScene();
        }

        UpdateCountdownText();
    }

    void SpawnPlayer()
    {
        // Rastgele bir spawn noktasý seçme
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        // Prefabý spawn noktasýna yerleþtirme
        Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

        // Spawn noktalarýný da ayný X pozisyonuna taþýma
        spawnPointsObject.transform.position = new Vector3(spawnPoint.position.x, spawnPointsObject.transform.position.y, spawnPointsObject.transform.position.z);
    }


    void UpdateCountdownText()
    {
        int minutes = Mathf.FloorToInt(countdownTimer / 60f);
        int seconds = Mathf.FloorToInt(countdownTimer % 60f);
        countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void ReloadScene()
    {
        // Önceki sahnede kalan nesneleri temizle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}








//using UnityEngine;

//public class PlayerSpawn : MonoBehaviour
//{
//    public Transform[] spawnPoints; // spawn noktalarý
//    public GameObject playerObject; // Player objesi
//    public GameObject spawnPointsObject; // spawn noktalarýný tutan obje

//    void Start()
//    {
//        // Rastgele bir spawn noktasý seçme
//        int randomIndex = Random.Range(0, spawnPoints.Length);
//        Transform spawnPoint = spawnPoints[randomIndex];

//        // Player'ý spawn noktasýna taþýma
//        playerObject.transform.position = new Vector3(spawnPoint.position.x, playerObject.transform.position.y, playerObject.transform.position.z);

//        // Spawn noktalarý objesini de ayný X pozisyonuna taþýma
//        spawnPointsObject.transform.position = new Vector3(spawnPoint.position.x, spawnPointsObject.transform.position.y, spawnPointsObject.transform.position.z);
//    }
//}





//using UnityEngine;

//public class PlayerSpawn : MonoBehaviour
//{
//    public Transform[] spawnPoints; // spawn noktalarý
//    public GameObject playerObject; // Player objesi

//    void Start()
//    {
//        // Rastgele bir spawn noktasý seçme
//        int randomIndex = Random.Range(0, spawnPoints.Length);
//        Transform spawnPoint = spawnPoints[randomIndex];

//        // Player'ý spawn noktasýna taþýma
//        playerObject.transform.position = new Vector3(spawnPoint.position.x, playerObject.transform.position.y, playerObject.transform.position.z);
//    }
//}
