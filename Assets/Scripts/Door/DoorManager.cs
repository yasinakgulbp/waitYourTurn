using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Her kap� i�in bir s�n�f
[System.Serializable]
public class Door
{
    public GameObject doorObject; // Kap� nesnesi
    public float openDuration = 3f; // Kap�n�n a��k kalaca�� s�re
    public float closeDuration = 3f; // Kap�n�n kapal� kalaca�� s�re
}

public class DoorManager : MonoBehaviour
{
    // Kap�lar�n listesi
    public List<Door> doors = new List<Door>();

    private void Start()
    {
        // Her bir kap� i�in a�ma ve kapama i�lemlerini ba�lat
        foreach (Door door in doors)
        {
            StartCoroutine(OpenAndCloseDoor(door));
        }
    }

    IEnumerator OpenAndCloseDoor(Door door)
    {
        while (true)
        {
            // Kap�y� a�
            door.doorObject.SetActive(true);

            // A��k kalma s�resini bekleyin
            yield return new WaitForSeconds(door.openDuration);

            // Kap�y� kapat
            door.doorObject.SetActive(false);

            // Kapal� kalma s�resini bekleyin
            yield return new WaitForSeconds(door.closeDuration);
        }
    }
}
