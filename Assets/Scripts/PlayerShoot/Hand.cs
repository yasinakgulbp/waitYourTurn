using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject Hit;
    public GameObject Fire;

    // Start is called before the first frame update
    void Start()
    {
        //rb.AddForce(transform.forward * 1000);
        GameObject A = Instantiate(Fire, this.transform.position, Quaternion.identity);
        Destroy(A, 2);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject B = Instantiate(Hit, this.transform.position, Quaternion.identity);
        Destroy(B, 2);
        //Destroy(this.gameObject);
    }
}
