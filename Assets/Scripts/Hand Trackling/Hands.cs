using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.PoseDetection;
public class Hands : MonoBehaviour
{
    // Start is called before the first frame update

    public OVRHand hand;
    public FingerFeatureStateProvider fingerFeatureStateProvider;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (IsStop())
        { print("AAAAA"); }
    }

    //********手勢偵測********

    bool IsStop()
    {
        if (!GetFingerIsCurling(OVRHand.HandFinger.Thumb) &&
        !GetFingerIsCurling(OVRHand.HandFinger.Index) &&
        !GetFingerIsCurling(OVRHand.HandFinger.Middle) &&
        !GetFingerIsCurling(OVRHand.HandFinger.Ring) &&
        !GetFingerIsCurling(OVRHand.HandFinger.Pinky)
        )
        {
            return true;
        }
        return false;
    }
    bool GetFingerIsCurling(OVRHand.HandFinger finger)
    {
        string s;
        var indexFingerState = fingerFeatureStateProvider.GetCurrentState((Oculus.Interaction.Input.HandFinger)finger, FingerFeature.Curl, out s);

        //print(s);
        //true為彎起 false為伸直
        return (s == "2" ? true : false);
    }
}
