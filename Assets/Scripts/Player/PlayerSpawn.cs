using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviour
{
    public Transform[] spawnPoints; // spawn noktalar�
    public GameObject playerPrefab; // Player objesinin prefab'i
    public GameObject spawnPointsObject; // spawn noktalar�n� tutan obje
    public TMP_Text countdownText; // geri say�m metni
    public float countdownDuration = 60f; // geri say�m s�resi
    private float countdownTimer; // geri say�m zamanlay�c�s�

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
            // Oyunu yeniden ba�lat
            ReloadScene();
        }

        UpdateCountdownText();
    }

    void SpawnPlayer()
    {
        // Rastgele bir spawn noktas� se�me
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        // Prefab� spawn noktas�na yerle�tirme
        Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

        // Spawn noktalar�n� da ayn� X pozisyonuna ta��ma
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
        // �nceki sahnede kalan nesneleri temizle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}








//using UnityEngine;

//public class PlayerSpawn : MonoBehaviour
//{
//    public Transform[] spawnPoints; // spawn noktalar�
//    public GameObject playerObject; // Player objesi
//    public GameObject spawnPointsObject; // spawn noktalar�n� tutan obje

//    void Start()
//    {
//        // Rastgele bir spawn noktas� se�me
//        int randomIndex = Random.Range(0, spawnPoints.Length);
//        Transform spawnPoint = spawnPoints[randomIndex];

//        // Player'� spawn noktas�na ta��ma
//        playerObject.transform.position = new Vector3(spawnPoint.position.x, playerObject.transform.position.y, playerObject.transform.position.z);

//        // Spawn noktalar� objesini de ayn� X pozisyonuna ta��ma
//        spawnPointsObject.transform.position = new Vector3(spawnPoint.position.x, spawnPointsObject.transform.position.y, spawnPointsObject.transform.position.z);
//    }
//}





//using UnityEngine;

//public class PlayerSpawn : MonoBehaviour
//{
//    public Transform[] spawnPoints; // spawn noktalar�
//    public GameObject playerObject; // Player objesi

//    void Start()
//    {
//        // Rastgele bir spawn noktas� se�me
//        int randomIndex = Random.Range(0, spawnPoints.Length);
//        Transform spawnPoint = spawnPoints[randomIndex];

//        // Player'� spawn noktas�na ta��ma
//        playerObject.transform.position = new Vector3(spawnPoint.position.x, playerObject.transform.position.y, playerObject.transform.position.z);
//    }
//}
