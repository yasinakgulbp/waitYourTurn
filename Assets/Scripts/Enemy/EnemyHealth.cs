using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 10;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Bullet")
        {
            health -= 2;

            if (health <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
