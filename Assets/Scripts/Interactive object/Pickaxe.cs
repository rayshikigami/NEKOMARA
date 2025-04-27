using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickaxe : MonoBehaviour
{
    
    // Start is called before the first frame update

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("SceneObjcet")||collision.gameObject.CompareTag("InteractObject")||collision.gameObject.CompareTag("foodpack"))
        {
            print("break the object");
            Destroy(collision.gameObject);
            GetComponent<AudioSource>().Play();
        }
    }
}
