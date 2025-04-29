using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatFood : MonoBehaviour
{
    // Start is called before the first frame update
    public int foodType; // (0: find bowl, 1: can, 2: fish, 4: 抹茶巴菲?)
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BeEaten()
    {
        Destroy(this.gameObject);
    }
}
