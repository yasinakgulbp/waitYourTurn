using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Kameran�n yatayda takip edece�i hedef nesne

    void Start()
    {
        // Player objesini bul ve target'e ata
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Player objesi bulunamad�! Kamera takibi yap�lamayacak.");
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Hedef nesnenin yatay pozisyonunu alarak, kameran�n yatay pozisyonunu g�ncelle
        Vector3 newPosition = transform.position;
        newPosition.x = target.position.x;
        transform.position = newPosition;
    }
}




//using UnityEngine;

//public class CameraFollow : MonoBehaviour
//{
//    public Transform target; // Kameran�n yatayda takip edece�i hedef nesne

//    void LateUpdate()
//    {
//        if (target == null)
//            return;

//        // Hedef nesnenin yatay pozisyonunu alarak, kameran�n yatay pozisyonunu g�ncelle
//        Vector3 newPosition = transform.position;
//        newPosition.x = target.position.x;
//        transform.position = newPosition;
//    }
//}
