using UnityEngine;

public class PlayerShot : MonoBehaviour
{
    public Animator animator; // Animator bile�enini referans almak i�in

    // Update her karede �a�r�l�r
    void Update()
    {
        // Fare sol tu�una bas�ld���nda
        if (Input.GetMouseButtonDown(0))
        {
            // isShotting bool de�erini true yap
            animator.SetBool("isShotting", true);
        }

        // Fare sol tu�undan elimizi kald�rd���m�zda
        if (Input.GetMouseButtonUp(0))
        {
            // isShotting bool de�erini false yap
            animator.SetBool("isShotting", false);
        }

        // "R" tu�una bas�ld���nda
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reloading animasyonunu �al��t�r
            animator.SetTrigger("Reload");
        }

        // "T" tu�una bas�ld���nda
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Hit animasyonunu �al��t�r
            animator.SetTrigger("TakeDamage");
        }

        // "Y" tu�una bas�ld���nda
        if (Input.GetKeyDown(KeyCode.Y))
        {
            // Hit animasyonunu �al��t�r
            animator.SetBool("isDie", true);
        }

    }
}
