using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class resetPosition : MonoBehaviour
{
    private Vector3 initialPosition; // 儲存初始位置

    void Start()
    {
        initialPosition = transform.position; // 在腳本啟動時記錄初始位置
    }

    void Update()
    {
        // 如果 Y 座標低於 -100
        if (transform.position.y < -100)
        {
            // 將 Rigidbody 的速度設為零
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            // 將物件傳送回初始位置
            transform.position = initialPosition;
        }
    }
}
