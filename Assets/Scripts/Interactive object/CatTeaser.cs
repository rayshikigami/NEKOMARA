using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatTeaser : MonoBehaviour
{
    // Start is called before the first frame update
    public float positionThreshold = 0.05f; // �X�G���|�۵M���ʴN�఻��
    private Vector3 lastPosition;
    public float rotationThreshold = 1f; // ���׮t���H��
    private Quaternion lastRotation;
    public bool swinging = false;
    public GameObject target;

    public float timer = 3, timer2 = 30;

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

        if(transform.position.y<-100)
        {
            timer = 5;
            teasing = false;
        }
        if (movement > positionThreshold || rotationDelta > rotationThreshold)
        {
            Debug.Log("shake");
            swinging = true;
            timer -= Time.fixedDeltaTime;
        }
        else
        {
            swinging = false;
            timer = 5;
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
            timer2 = 30;
        }

        if(timer2<=0)
        {
            teasing = false;
        }
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

}
