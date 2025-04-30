using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TagCapsule : MonoBehaviour
{
    public OVRSkeleton skeleton;

    void Start()
    {
        // ��� "Capsules" �l����
        Transform capsulesRoot = transform.Find("Capsules");
        if (capsulesRoot == null)
        {
            Debug.LogError("�䤣�� Capsules ����I");
            return;
        }

        int count = 0;

        // ���j�Ҧ��l����
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

        Debug.Log($"�@�аO {count} �ӱa�� CapsuleCollider ������ Hand");
    }
}
