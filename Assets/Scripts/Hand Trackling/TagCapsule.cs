using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TagCapsule : MonoBehaviour
{
    public OVRSkeleton skeleton;

    void Start()
    {
        // 找到 "Capsules" 子物件
        Transform capsulesRoot = transform.Find("Capsules");
        if (capsulesRoot == null)
        {
            Debug.LogError("找不到 Capsules 物件！");
            return;
        }

        int count = 0;

        // 遞迴所有子物件
        foreach (Transform child in capsulesRoot.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.tag = "Hand";
            CapsuleCollider col = child.GetComponent<CapsuleCollider>();
            if (col != null)
            {
                child.gameObject.tag = "Hand";
                count++;
            }
        }

        Debug.Log($"共標記 {count} 個帶有 CapsuleCollider 的物件為 Hand");
    }
}
