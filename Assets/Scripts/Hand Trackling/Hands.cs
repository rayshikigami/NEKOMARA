using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.PoseDetection;
public class Hands : MonoBehaviour
{
    // Start is called before the first frame update

    public OVRHand hand;
    public OVRSkeleton skeleton;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.LogError("CCCCC");
        //if (IsSix())
        //{ Debug.LogError("AAAAA"); }
        if (IsFingerCurled(OVRSkeleton.BoneId.Hand_Index1, OVRSkeleton.BoneId.Hand_Index2, OVRSkeleton.BoneId.Hand_Index3))
        { Debug.LogError("AAAAABBBBBB"); }
    }

    //********手勢偵測********

    //bool IsSix()
    //{
    //    if (!GetFingerIsCurling(OVRHand.HandFinger.Thumb) &&
    //    GetFingerIsCurling(OVRHand.HandFinger.Index) &&
    //    GetFingerIsCurling(OVRHand.HandFinger.Middle) &&
    //    GetFingerIsCurling(OVRHand.HandFinger.Ring) &&
    //    !GetFingerIsCurling(OVRHand.HandFinger.Pinky)
    //    )
    //    {
    //        return true;
    //    }
    //    return false;
    //    }
//bool GetFingerIsCurling(OVRHand.HandFinger finger)
//{
//    string s = "2";
//    //var indexFingerState = fingerFeatureStateProvider.GetCurrentState((Oculus.Interaction.Input.HandFinger)finger, FingerFeature.Curl, out s);

//    //print(s);
//    //true為彎起 false為伸直
//    return (s == "2" ? true : false);
//}


public bool IsFingerCurled(OVRSkeleton.BoneId proximalId, OVRSkeleton.BoneId intermediateId, OVRSkeleton.BoneId distalId, float angleThreshold = 45f)
{
    if (skeleton == null || !skeleton.IsDataValid || !skeleton.IsDataHighConfidence)
        return false;

    Transform proximal = GetBoneTransform(proximalId);
    Transform intermediate = GetBoneTransform(intermediateId);
    Transform distal = GetBoneTransform(distalId);

    if (proximal == null || intermediate == null || distal == null)
        return false;

    Vector3 dir1 = (intermediate.position - proximal.position).normalized;
    Vector3 dir2 = (distal.position - intermediate.position).normalized;

    float angle = Vector3.Angle(dir1, dir2);

    return angle > angleThreshold;
}

private Transform GetBoneTransform(OVRSkeleton.BoneId boneId)
{
    foreach (var bone in skeleton.Bones)
    {
        if (bone.Id == boneId)
            return bone.Transform;
    }
    return null;
}



}
