using UnityEngine;
using System.Collections;
using TMPro;

public class DoorManager : MonoBehaviour
{
    [Header("Doors Settings")]
    public GameObject parentDoorObject; // Kapýlarýn baðlý olduðu parent nesne
    public float openDuration = 10f;    // Kapýlarýn açýk kalacaðý süre
    public float closeDuration = 10f;   // Kapýlarýn kapalý kalacaðý süre

    [Header("UI Settings")]
    public TMP_Text statusText;         // Durum bilgisini gösterecek TMP metni

    private Transform[] doorTransforms;

    private void Start()
    {
        // Parent altýnda bulunan tüm child objelerini (kapýlar dahil) bul
        doorTransforms = parentDoorObject.GetComponentsInChildren<Transform>();
        StartCoroutine(OpenAndCloseDoors());
    }

    IEnumerator OpenAndCloseDoors()
    {
        while (true)
        {
            // Baþlangýçta kapýlar kapalý durumda olsun
            SetDoorsActive(true);
            statusText.text = "Gates are closed.";

            // Kapýlarýn açýlmasýna closeDuration süre var
            // Açýlmadan 3 saniye önce bildir
            yield return new WaitForSeconds(closeDuration - 3f);
            statusText.text = "Gates are opening...";
            yield return new WaitForSeconds(3f);

            // Kapýlarý aç (burada aktif deðil, yani kapý engeli yok -> açýk demek)
            SetDoorsActive(false);
            statusText.text = "Gates are open.";

            // Kapýlarýn kapanmasýna openDuration süre var
            // Kapanmadan 3 saniye önce bildir
            yield return new WaitForSeconds(openDuration - 3f);
            statusText.text = "Gates are closing...";
            yield return new WaitForSeconds(3f);

            // Kapýlarý tekrar kapat
            SetDoorsActive(true);
            // Döngü yeniden baþa dönecek ve baþta "Gates are closed." yazýsý tekrar görünecek.
        }
    }

    void SetDoorsActive(bool isActive)
    {
        // Ýlk element parent'ýn kendisi olduðu için i=1'den baþlýyoruz
        for (int i = 1; i < doorTransforms.Length; i++)
        {
            if (doorTransforms[i] != null)
                doorTransforms[i].gameObject.SetActive(isActive);
        }
    }
}








//eski çalýþan kod
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//// Her kapý için bir sýnýf
//[System.Serializable]
//public class Door
//{
//    public GameObject doorObject; // Kapý nesnesi
//    public float openDuration = 3f; // Kapýnýn açýk kalacaðý süre
//    public float closeDuration = 3f; // Kapýnýn kapalý kalacaðý süre
//}

//public class DoorManager : MonoBehaviour
//{
//    // Kapýlarýn listesi
//    public List<Door> doors = new List<Door>();

//    private void Start()
//    {
//        // Her bir kapý için açma ve kapama iþlemlerini baþlat
//        foreach (Door door in doors)
//        {
//            StartCoroutine(OpenAndCloseDoor(door));
//        }
//    }

//    IEnumerator OpenAndCloseDoor(Door door)
//    {
//        while (true)
//        {
//            // Kapýyý aç
//            door.doorObject.SetActive(true);

//            // Açýk kalma süresini bekleyin
//            yield return new WaitForSeconds(door.openDuration);

//            // Kapýyý kapat
//            door.doorObject.SetActive(false);

//            // Kapalý kalma süresini bekleyin
//            yield return new WaitForSeconds(door.closeDuration);
//        }
//    }
//}
