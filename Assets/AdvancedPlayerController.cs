using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AdvancedPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Hareket h�z�
    public float jumpForce = 8f; // Z�plama kuvveti
    public float gravity = 20f; // Yer�ekimi miktar�

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
        // Yatay ve dikey giri�leri al
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveVector = new Vector3(horizontalInput, 0f, verticalInput);
        moveVector = transform.TransformDirection(moveVector);
        moveVector *= moveSpeed;

        // Yer�ekimini uygula
        if (!controller.isGrounded)
        {
            moveVector.y -= gravity * Time.deltaTime;
        }

        // Karakteri hareket ettir
        controller.Move(moveVector * Time.deltaTime);

        // Hareket vekt�r� s�f�r de�ilse animasyonu ba�lat
        bool isRunning = moveVector.magnitude > 0;
        animator.SetBool("isRunning", isRunning);
    }

    private void RotateTowardsMouse()
    {
        // Fare pozisyonunu d�nya koordinatlar�na �evir
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.y - transform.position.y;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // Karakteri fare pozisyonuna do�ru d�nd�r
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f; // Yaln�zca yatay d�zlemde d�nmek i�in y eksenini s�f�rla
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
