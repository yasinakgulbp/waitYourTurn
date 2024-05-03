using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Oyuncu hareket hýzý
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        // Kameranýn yönüne göre hareket etme
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveDirection = cameraForward * Input.GetAxis("Vertical") + Camera.main.transform.right * Input.GetAxis("Horizontal");

        // Hareket vektörünü normalize et ve oyuncu hýzý ile çarp
        moveDirection.Normalize();
        velocity = moveDirection * moveSpeed;

        // Hareketi uygula
        transform.position += velocity * Time.deltaTime;

        // Fare imlecinin olduðu yöne bakma
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Vector3 lookDirection = point - transform.position;
            lookDirection.y = 0; // Y ekseninde dönüþ yapma
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // Tuþ býrakýldýðýnda hýzý sýfýrla
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            velocity = Vector3.zero;
        }
    }
}
