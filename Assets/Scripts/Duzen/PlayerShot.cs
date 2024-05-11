using UnityEngine;

public class PlayerShot : MonoBehaviour
{
    public Animator animator; // Animator bileþenini referans almak için

    // Update her karede çaðrýlýr
    void Update()
    {
        // Fare sol tuþuna basýldýðýnda
        if (Input.GetMouseButtonDown(0))
        {
            // isShotting bool deðerini true yap
            animator.SetBool("isShotting", true);
        }

        // Fare sol tuþundan elimizi kaldýrdýðýmýzda
        if (Input.GetMouseButtonUp(0))
        {
            // isShotting bool deðerini false yap
            animator.SetBool("isShotting", false);
        }

        // "R" tuþuna basýldýðýnda
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reloading animasyonunu çalýþtýr
            animator.SetTrigger("Reload");
        }

        // "T" tuþuna basýldýðýnda
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Hit animasyonunu çalýþtýr
            animator.SetTrigger("TakeDamage");
        }

        // "Y" tuþuna basýldýðýnda
        if (Input.GetKeyDown(KeyCode.Y))
        {
            // Hit animasyonunu çalýþtýr
            animator.SetBool("isDie", true);
        }

    }
}
