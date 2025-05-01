using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CatTower : MonoBehaviour
{
    // Start is called before the first frame update

    public float timer = 3;
    Vector3 prevPosition;
    Quaternion prevRotation;

    public GameObject grabInteractor;

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


        if (!isSet && OnTheFloor && (Math.Abs(transform.eulerAngles.x) < 3 || Math.Abs(transform.eulerAngles.x) > 357) && (Math.Abs(transform.eulerAngles.z) < 3 || Math.Abs(transform.eulerAngles.z) > 357))
        {
            print(" on the floor");
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
                GetComponent<SphereCollider>().enabled = true;
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                grabInteractor.SetActive(false);
                GetComponent<AudioSource>().Play();
                //FindObjectOfType<NavBuilder>().BuildMap();
                FindObjectOfType<AchieveSystem>().UpdateProgress("set_object", 1);
                timer = 3;
                GetComponent<resetPosition>().SetPosition();
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

            // get other.GetComponent<CatStateManager>()

        }
    }

    void OnTriggerExit(Collider other)
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
