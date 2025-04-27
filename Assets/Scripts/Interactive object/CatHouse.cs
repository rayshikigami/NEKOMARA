using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class CatHouse : MonoBehaviour
{
    public GameObject[] CatList;
    float timer = 3;
    Vector3 prevPosition;
    Quaternion prevRotation;

    bool OnTheFloor = false;
    public bool isSummon = false;
    public GameObject grabInteractor;
    // Start is called before the first frame update
    void Start()
    {
        prevPosition = transform.position;
        prevRotation = transform.rotation;
        OnTheFloor = false;
        isSummon = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (OnTheFloor && !isSummon)
        {
            if (prevPosition == transform.position && prevRotation == transform.rotation && Math.Abs(transform.eulerAngles.x) < 3 && Math.Abs(transform.eulerAngles.z) < 3)
            {
                timer -= Time.fixedDeltaTime;
            }
            else
            {
                prevPosition = transform.position;
                prevRotation = transform.rotation;
                timer = 3;

            }

            if (timer <= 0 && !isSummon)
            {
                isSummon = true;
                timer = 3;
                grabInteractor.SetActive(false);
                GetComponent<AudioSource>().Play();
                summon(0);//要召喚的貓的id
            }
        }
    }

    public void summon(int id)
    {
        print("召喚貓");
        FindObjectOfType<AchieveSystem>().UpdateProgress("summon", 1);
        Instantiate(CatList[id], transform.position + new Vector3(0, 0.3f, 0), transform.rotation);
        FindObjectOfType<NavBuilder>().BuildMap();

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
            timer = 3;
        }
    }

}
