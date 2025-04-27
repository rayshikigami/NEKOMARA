using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Bell : MonoBehaviour
{
    // Start is called before the first frame update
    public float positionThreshold = 0.05f; // 幾乎不會自然移動就能偵測
    private Vector3 lastPosition;
    public float rotationThreshold = 1f; // 角度差異閾值
    private Quaternion lastRotation;
    AudioSource bellSound;
    public bool ringing = false;

    public float timer = 3;
    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        ringing = false; bellSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void FixedUpdate()
    {
        float movement = (transform.position - lastPosition).magnitude;
        float rotationDelta = Quaternion.Angle(transform.rotation, lastRotation);


        if (movement > positionThreshold || rotationDelta > rotationThreshold)
        {
            Debug.Log("shake");
            ringing = true;
            timer -= Time.fixedDeltaTime;
            bellSound.Play();
        }
        else
        {
            ringing = false;
            timer = 3;
            bellSound.Stop();
        }

        if (timer <= 0)
        {
            //叫醒貓
        }
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }
}
