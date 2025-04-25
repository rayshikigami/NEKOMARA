using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatBowl : MonoBehaviour
{
    // Start is called before the first frame update
    float timer = 3;
    Vector3 prevPosition;
    Quaternion prevRotation;

    public bool OnTheFloor = false, isSet = false;
    public bool isFull;
    public GameObject FoodInTheBowl;
    void Start()
    {
        prevPosition = transform.position;
        prevRotation = transform.rotation;
        OnTheFloor = false;
        isFull = false; isSet = false;
    }

    // Update is called once per frame
    void Update()
    {

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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.transform.name == "FLOOR_EffectMesh")
        {
            print("set on floor");
            OnTheFloor = true;
            timer = 3;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.transform.name == "FLOOR_EffectMesh")
        {
            print("remove from floor");
            OnTheFloor = false;
            timer = 3; isSet = false;
            isFull = false;
            FoodInTheBowl.SetActive(false);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (isSet && !isFull && other.CompareTag("foodpack"))
        {
            isFull = true;
            //貓食包消失(?)
            FoodInTheBowl.SetActive(true);
        }
    }

    public void Eaten()
    {
        isFull = false;
        FoodInTheBowl.SetActive(false);
    }
}
