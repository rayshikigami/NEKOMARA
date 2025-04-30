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
            Debug.LogError("找不到 OVRSkeleton 元件！");
            return;
        }

        if (!skeleton.IsInitialized)
        {
            // 等骨架初始化完成再處理
            StartCoroutine(DelayedTagging());
        }
        else
        {
            TagAllCapsules();
        }
    }

    IEnumerator DelayedTagging()
    {
        yield return new WaitForSeconds(1f); // 等骨架初始化完成
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
                Debug.LogError($"已設定 Tag 'Hand' 給：{bone.Id}");
            }
        }
    }
}
