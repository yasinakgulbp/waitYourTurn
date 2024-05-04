using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AdvancedPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Hareket hýzý
    public float jumpForce = 8f; // Zýplama kuvveti
    public float gravity = 20f; // Yerçekimi miktarý

    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Move();
        RotateTowardsMouse();
    }

    private void Move()
    {
        // Yatay ve dikey giriþleri al
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveVector = new Vector3(horizontalInput, 0f, verticalInput);
        moveVector = transform.TransformDirection(moveVector);
        moveVector *= moveSpeed;

        // Yerçekimini uygula
        if (!controller.isGrounded)
        {
            moveVector.y -= gravity * Time.deltaTime;
        }

        // Karakteri hareket ettir
        controller.Move(moveVector * Time.deltaTime);

        // Hareket vektörü sýfýr deðilse animasyonu baþlat
        bool isRunning = moveVector.magnitude > 0;
        animator.SetBool("isRunning", isRunning);
    }

    private void RotateTowardsMouse()
    {
        // Fare pozisyonunu dünya koordinatlarýna çevir
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.y - transform.position.y;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // Karakteri fare pozisyonuna doðru döndür
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f; // Yalnýzca yatay düzlemde dönmek için y eksenini sýfýrla
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    private void Jump()
    {
        animator.SetTrigger("Jump");
        moveDirection.y = jumpForce;
    }
}
