using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalTest : MonoBehaviour
{
    // Start is called before the first frame update

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hand"))
        {
            Debug.LogWarning("touch by hand");
        }
        else
        {
            Debug.LogWarning(collision.gameObject.transform.name);
        }
    }
}
