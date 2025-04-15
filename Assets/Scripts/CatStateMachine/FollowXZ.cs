using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowXZ : MonoBehaviour
{
    public Transform target;  // 指定要跟隨的對象（Cube）

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 pos = transform.position;
            pos.x = target.position.x;
            pos.z = target.position.z;
            transform.position = pos;
        }
    }
}
