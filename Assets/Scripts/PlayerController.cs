using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Oyuncu hareket h�z�
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        // Kameran�n y�n�ne g�re hareket etme
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveDirection = cameraForward * Input.GetAxis("Vertical") + Camera.main.transform.right * Input.GetAxis("Horizontal");

        // Hareket vekt�r�n� normalize et ve oyuncu h�z� ile �arp
        moveDirection.Normalize();
        velocity = moveDirection * moveSpeed;

        // Hareketi uygula
        transform.position += velocity * Time.deltaTime;

        // Fare imlecinin oldu�u y�ne bakma
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Vector3 lookDirection = point - transform.position;
            lookDirection.y = 0; // Y ekseninde d�n�� yapma
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // Tu� b�rak�ld���nda h�z� s�f�rla
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            velocity = Vector3.zero;
        }
    }
}
