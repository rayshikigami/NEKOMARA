using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using Meta.XR.MRUtilityKit;
using UnityEngine.AI;

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

    public int catID = -99, ThisId;

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
            if (prevPosition == transform.position && prevRotation == transform.rotation && (Math.Abs(transform.eulerAngles.x) < 3 || Math.Abs(transform.eulerAngles.x) > 357) && (Math.Abs(transform.eulerAngles.z) < 3 || Math.Abs(transform.eulerAngles.z) > 357))
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
                GetComponent<resetPosition>().SetPosition();
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                GetComponent<AudioSource>().Play();

                if (catID == -99)
                {
                    ThisId = UnityEngine.Random.Range(0, 3);
                }
                else
                {
                    ThisId = catID;
                }
                
                summon(ThisId);//要召喚的貓的id
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
        FindObjectOfType<SceneNavigation>().Agents.Add(catForThisHouse.GetComponent<NavMeshAgent>());
        FindObjectOfType<SceneNavigation>().BuildSceneNavMesh();
        //FindObjectOfType<NavBuilder>().BuildMap();

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

    public void breakTheHouse()
    {
        if (catForThisHouse != null)
        {
            Instantiate(smokeParticle, catForThisHouse.transform.position, catForThisHouse.transform.rotation);
            catForThisHouse.SetActive(false);
        }
        OnTheFloor = false;
        isSummon = false;
        timer = 3;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        grabInteractor.SetActive(true);
        //Destroy(catForThisHouse);//這邊之後要改成setActive(false);
    }

}
