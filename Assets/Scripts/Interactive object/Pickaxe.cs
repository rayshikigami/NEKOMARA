using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickaxe : MonoBehaviour
{
    
    // Start is called before the first frame update

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("SceneObject")||collision.gameObject.CompareTag("InteractObject")||collision.gameObject.CompareTag("foodpack"))
        {

            print("break the object");
            Destroy(collision.gameObject);
            GetComponent<AudioSource>().Play();
        }
        if(collision.gameObject.CompareTag("catHouse"))
        {
            print("break the cat house");
            collision.gameObject.GetComponent<CatHouse>().breakTheHouse();
            collision.gameObject.SetActive(false);
            
            GetComponent<AudioSource>().Play();
        }
    }
}
