using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Her kapý için bir sýnýf
[System.Serializable]
public class Door
{
    public GameObject doorObject; // Kapý nesnesi
    public float openDuration = 3f; // Kapýnýn açýk kalacaðý süre
    public float closeDuration = 3f; // Kapýnýn kapalý kalacaðý süre
}

public class DoorManager : MonoBehaviour
{
    // Kapýlarýn listesi
    public List<Door> doors = new List<Door>();

    private void Start()
    {
        // Her bir kapý için açma ve kapama iþlemlerini baþlat
        foreach (Door door in doors)
        {
            StartCoroutine(OpenAndCloseDoor(door));
        }
    }

    IEnumerator OpenAndCloseDoor(Door door)
    {
        while (true)
        {
            // Kapýyý aç
            door.doorObject.SetActive(true);

            // Açýk kalma süresini bekleyin
            yield return new WaitForSeconds(door.openDuration);

            // Kapýyý kapat
            door.doorObject.SetActive(false);

            // Kapalý kalma süresini bekleyin
            yield return new WaitForSeconds(door.closeDuration);
        }
    }
}
