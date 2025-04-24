using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CatTower : MonoBehaviour
{
    // Start is called before the first frame update

    float timer = 3;
    Vector3 prevPosition;
    Quaternion prevRotation;

    public bool OnTheFloor = false, isSet = false;
    void Start()
    {
        prevPosition = transform.position;
        prevRotation = transform.rotation;
        OnTheFloor = false;
        isSet = false;
    }
    void FixedUpdate()
    {
        if (OnTheFloor && Math.Abs(transform.rotation.x) < 3 && Math.Abs(transform.rotation.z) < 3)
        {
            if (prevPosition == transform.position && prevRotation == transform.rotation)
            {
                timer -= Time.fixedDeltaTime;
            }
            else
            {
                prevPosition = transform.position;
                prevRotation = transform.rotation;
                timer = 3;
                isSet = false;

            }

            if (timer <= 0)
            {
                isSet = true;
                timer = 3;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(Collider other)
    {
        if (isSet && other.CompareTag("cat"))
        {
            //更改貓的狀態 變成可跳躍
        }
    }

    void OTriggerExit(Collider other)
    {
        if (isSet && other.CompareTag("cat"))
        {
            //更改貓的狀態變成 不可跳躍
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.transform.name == "FLOOR_EffectMesh")
        {
            print("cat tower set on floor");
            OnTheFloor = true;
            timer = 3;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.transform.name == "FLOOR_EffectMesh")
        {
            print("cat tower remove from floor");
            OnTheFloor = false;
            timer = 3; isSet = false;
        }
    }
}
