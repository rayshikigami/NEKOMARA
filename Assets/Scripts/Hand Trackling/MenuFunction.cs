using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class MenuFunction : MonoBehaviour
{
    // Start is called before the first frame update

    public List<GameObject> SceneObjectList;
    public List<GameObject> InteractorObjectList;
    public List<GameObject> FoodList;
    private float lastTime;

    public void CallInteractorObject(int id)
    {
        if (Time.time - lastTime < 0.1f) { return; }
        lastTime = Time.time;
        //加粒子特效?
        if (id == 1)
        {
            InteractorObjectList[1].transform.position = transform.position;
            InteractorObjectList[1].GetComponent<CatTeaser>().teasing = false;
            InteractorObjectList[1].GetComponent<Rigidbody>().velocity = Vector3.zero;
           
            InteractorObjectList[1].SetActive(true);
            return;
        }
        Instantiate(InteractorObjectList[id], transform.position, transform.rotation);
    }

    public void CallSceneObject(int id)
    {
        if (Time.time - lastTime < 0.1f) { return; }
        lastTime = Time.time;
        if (id == 1 || id == 2) 
        {
            Instantiate(SceneObjectList[id], transform.position, transform.rotation);
            return;
        }
        SceneObjectList[id].transform.position = transform.position;
        SceneObjectList[id].SetActive(true);

    }

    public void CallFood(int id)
    {
        if (Time.time - lastTime < 0.1f) { return; }
        lastTime = Time.time;
        Instantiate(FoodList[id], transform.position, transform.rotation);
    }
}
