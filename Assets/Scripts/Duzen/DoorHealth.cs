using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHealth : MonoBehaviour
{
    public int health = 10;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Hand")
        {
            health -= 2;

            if (health <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }


    // Hasar al�nd���nda bu metot �a�r�l�r
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
