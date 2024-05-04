using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShootting : MonoBehaviour
{
    public GameObject bullet;
    public GameObject firePosition;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Shotting()
    {
        Instantiate(bullet, firePosition.transform.position, firePosition.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        // Fare sol tuþuna basýldýðýnda
        if (Input.GetMouseButtonDown(0))
        {
            Shotting();
        }
    }
}
