using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public GameObject bullet;
    public GameObject firePosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shotting();
        }

    }

    private void Shotting()
    {
        Instantiate(bullet, firePosition.transform.position, firePosition.transform.rotation);
    }
}
