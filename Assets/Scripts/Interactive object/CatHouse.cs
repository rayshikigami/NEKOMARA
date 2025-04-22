using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatHouse : MonoBehaviour
{
    public GameObject[] CatList;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void summon(int id)
    {
        print("召喚貓");
        FindObjectOfType<AchieveSystem>().UpdateProgress("summon", 1);
        Instantiate(CatList[id], transform.position + new Vector3(0, 0.3f, 0), transform.rotation);

    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Floor"))
        {
            print("set on floor");
            summon(0);//要召喚的貓的id
        }
    }
}
