//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerShootting : MonoBehaviour
//{
//    public GameObject bullet;
//    public GameObject firePosition;

//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    private void Shotting()
//    {
//        Instantiate(bullet, firePosition.transform.position, firePosition.transform.rotation);
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        // Fare sol tuþuna basýldýðýnda
//        if (Input.GetMouseButtonDown(0))
//        {
//            Shotting();
//        }
//    }
//}



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShootting : MonoBehaviour
{
    public GameObject bullet;
    public GameObject firePosition;

    // Mermi sesi için AudioSource ve AudioClip referanslarý
    public AudioSource audioSource;
    public AudioClip bulletSound;

    void Start()
    {
        // Eðer audioSource inspector üzerinden atanmamýþsa,
        // script'in bulunduðu objedeki AudioSource bileþenini bul:
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Shotting()
    {
        // Mermi üretiliyor
        Instantiate(bullet, firePosition.transform.position, firePosition.transform.rotation);

        // Ses efekti çalýnýyor
        if (audioSource != null && bulletSound != null)
        {
            audioSource.PlayOneShot(bulletSound);
        }
        else
        {
            Debug.LogWarning("AudioSource veya bulletSound atanmamýþ!");
        }
    }

    void Update()
    {
        // Fare sol tuþuna basýldýðýnda ateþ
        if (Input.GetMouseButtonDown(0))
        {
            Shotting();
        }
    }
}
