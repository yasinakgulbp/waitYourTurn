using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Kameranýn yatayda takip edeceði hedef nesne

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
