using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatTeaser : MonoBehaviour
{
    // Start is called before the first frame update
    public float positionThreshold = 0.05f; // 幾乎不會自然移動就能偵測
    private Vector3 lastPosition;
    public float rotationThreshold = 1f; // 角度差異閾值
    private Quaternion lastRotation;
    public bool swinging = false;
    public GameObject target;

    public float timer = 5, timer2 = 30;

    public bool teasing = false;
    void Start()
    {
        lastPosition = target.transform.position;
        lastRotation = target.transform.rotation;
        swinging = false;
    }

    void FixedUpdate()
    {
        float movement = (transform.position - lastPosition).magnitude;
        float rotationDelta = Quaternion.Angle(transform.rotation, lastRotation);


        if (movement > positionThreshold || rotationDelta > rotationThreshold)
        {
            Debug.Log("shake");
            swinging = true;
            timer -= Time.fixedDeltaTime;
        }
        else
        {
            swinging = false;
            timer = 3;
        }

        if (timer <= 0)
        {
            teasing = true;

        }
        if (!(movement > positionThreshold || rotationDelta > rotationThreshold))
        {
            //Debug.Log("shake");
            swinging = false;
            timer2 -= Time.fixedDeltaTime;
        }
        else
        {
            swinging = true;
            timer2 = 3;
        }

        if(timer2<=0)
        {
            teasing = false;
        }
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

}
