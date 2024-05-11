using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Kameranýn yatayda takip edeceði hedef nesne

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
            Debug.LogWarning("Player objesi bulunamadý! Kamera takibi yapýlamayacak.");
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Hedef nesnenin yatay pozisyonunu alarak, kameranýn yatay pozisyonunu güncelle
        Vector3 newPosition = transform.position;
        newPosition.x = target.position.x;
        transform.position = newPosition;
    }
}




//using UnityEngine;

//public class CameraFollow : MonoBehaviour
//{
//    public Transform target; // Kameranýn yatayda takip edeceði hedef nesne

//    void LateUpdate()
//    {
//        if (target == null)
//            return;

//        // Hedef nesnenin yatay pozisyonunu alarak, kameranýn yatay pozisyonunu güncelle
//        Vector3 newPosition = transform.position;
//        newPosition.x = target.position.x;
//        transform.position = newPosition;
//    }
//}
