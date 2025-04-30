using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TagCapsule : MonoBehaviour
{
    public OVRSkeleton skeleton;

    void Start()
    {
        if (skeleton == null)
            skeleton = GetComponent<OVRSkeleton>();

        if (skeleton == null)
        {
            Debug.LogError("�䤣�� OVRSkeleton ����I");
            return;
        }

        if (!skeleton.IsInitialized)
        {
            // �����[��l�Ƨ����A�B�z
            StartCoroutine(DelayedTagging());
        }
        else
        {
            TagAllCapsules();
        }
    }

    IEnumerator DelayedTagging()
    {
        yield return new WaitForSeconds(1f); // �����[��l�Ƨ���
        TagAllCapsules();
    }

    void TagAllCapsules()
    {
        foreach (var bone in skeleton.Bones)
        {
            Debug.LogError("BONE!!!!!!");
            CapsuleCollider capsule = bone.Transform.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                capsule.gameObject.tag = "Hand";
                Debug.LogError($"�w�]�w Tag 'Hand' ���G{bone.Id}");
            }
        }
    }
}
