using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class CatHouse : MonoBehaviour
{
    public List<GameObject> CatList;
    float timer = 3;
    Vector3 prevPosition;
    Quaternion prevRotation;

    bool OnTheFloor = false;
    public bool isSummon = false;
    public GameObject grabInteractor;
    public GameObject catForThisHouse;

    public int catID = -99;

    public GameObject smokeParticle;
    // Start is called before the first frame update
    void Start()
    {
        prevPosition = transform.position;
        prevRotation = transform.rotation;
        OnTheFloor = false;
        isSummon = false;
        // CatList.Add(GameObject.Find("cat1"));
        // CatList.Add(GameObject.Find("cat2"));
        // CatList.Add(GameObject.Find("cat3"));

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
                FindObjectOfType<AchieveSystem>().UpdateProgress("set_object", 1);
                FindObjectOfType<AchieveSystem>().UpdateProgress("summon", 1);
                GetComponent<AudioSource>().Play();
                if (catID == -99)
                {
                    catID = UnityEngine.Random.Range(0, 3);
                }
                summon(catID);//要召喚的貓的id
            }
        }
    }

    public void summon(int id)
    {
        print("召喚貓");
        FindObjectOfType<AchieveSystem>().UpdateProgress("summon", 1);
        catForThisHouse = CatList[id];
        catForThisHouse.SetActive(true);
        catForThisHouse.transform.position = transform.position + new Vector3(0, 0.3f, 0);

        Instantiate(smokeParticle, transform.position, transform.rotation);
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

    public void OnDestroy()
    {
        Instantiate(smokeParticle, transform.position, transform.rotation);
        catForThisHouse.SetActive(false);
        //Destroy(catForThisHouse);//這邊之後要改成setActive(false);
    }

}
