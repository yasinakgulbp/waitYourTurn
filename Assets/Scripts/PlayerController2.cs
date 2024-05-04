using UnityEngine;

public class PlayerController2 : MonoBehaviour
{
    // Hareket hýzý
    public float moveSpeed = 5.0f;

    // Dönüþ hýzý
    public float turnSpeed = 100.0f;

    // Karakter Controller bileþeni
    private CharacterController controller;

    void Start()
    {
        // Karakter Controller bileþenini al
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Hareket yönünü belirle
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);

        // Karakteri hareket ettir
        controller.SimpleMove(moveDirection * moveSpeed);

        // Fare imlecine doðru dön
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 targetDirection = hit.point - transform.position;
            targetDirection.y = 0; // Y ekseninde dönmeyi engelle

            // Dönüþ açýsýný hesapla
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Karakteri döndür
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}