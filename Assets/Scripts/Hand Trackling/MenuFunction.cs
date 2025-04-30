using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuFunction : MonoBehaviour
{
    // Start is called before the first frame update

    public List<GameObject> SceneObjectList;
    public List<GameObject> InteractorObjectList;
    public List<GameObject> FoodList;

    public void CallInteractorObject(int id)
    {
        //加粒子特效?
        if (id == 1)
        {
            InteractorObjectList[1].transform.position = transform.position;
            InteractorObjectList[1].SetActive(true);
            return;
        }
        Instantiate(InteractorObjectList[id], transform.position, transform.rotation);
    }

    public void CallSceneObject(int id)
    {
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
        Instantiate(FoodList[id], transform.position, transform.rotation);
    }
}
