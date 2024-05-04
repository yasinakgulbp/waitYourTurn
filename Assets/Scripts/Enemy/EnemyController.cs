using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Animator animator;
    private bool isAttacking = false;
    private float attackInterval = 3f;
    private float attackTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Sald�r�y� kontrol et
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            Attack();
            attackTimer = 0f;
        }
    }

    // Sald�r� animasyonunu oynat
    private void Attack()
    {
        // Sald�r� animasyonunu ba�lat
        animator.SetBool("isAtacking", true);
    }
}
