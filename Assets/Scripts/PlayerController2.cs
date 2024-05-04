using UnityEngine;

public class PlayerController2 : MonoBehaviour
{
    // Hareket h�z�
    public float moveSpeed = 5.0f;

    // D�n�� h�z�
    public float turnSpeed = 100.0f;

    // Karakter Controller bile�eni
    private CharacterController controller;

    void Start()
    {
        // Karakter Controller bile�enini al
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Hareket y�n�n� belirle
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);

        // Karakteri hareket ettir
        controller.SimpleMove(moveDirection * moveSpeed);

        // Fare imlecine do�ru d�n
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 targetDirection = hit.point - transform.position;
            targetDirection.y = 0; // Y ekseninde d�nmeyi engelle

            // D�n�� a��s�n� hesapla
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Karakteri d�nd�r
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}