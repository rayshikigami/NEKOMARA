using UnityEngine;
//using OculusSampleFramework;
using UnityEngine.UI;  // 新增，給 UI Text 用

public class HandGestureMenuController : MonoBehaviour
{
    public OVRHand leftHand;
    public GameObject gestureMenuCanvas;
    public Text gestureStatusText; // 顯示手勢狀態文字

    private bool isMenuVisible = false;

    void Update()
    {
        if (leftHand == null || gestureMenuCanvas == null)
            return;

        bool isSixGesture = IsMakingSixGesture(leftHand);

        // 更新手勢偵測顯示
        if (gestureStatusText != null)
        {
            gestureStatusText.text = isSixGesture ? "手勢：6" : "手勢：無";
        }

        // 根據手勢決定顯示或隱藏選單
        if (isSixGesture && !isMenuVisible)
        {
            gestureMenuCanvas.SetActive(true);
            isMenuVisible = true;
        }
        else if (!isSixGesture && isMenuVisible)
        {
            gestureMenuCanvas.SetActive(false);
            isMenuVisible = false;
        }
    }

    // 正確定義6手勢
    bool IsMakingSixGesture(OVRHand hand)
    {
        bool thumbExtended = hand.GetFingerConfidence(OVRHand.HandFinger.Thumb) == OVRHand.TrackingConfidence.High;
        bool pinkyExtended = hand.GetFingerConfidence(OVRHand.HandFinger.Pinky) == OVRHand.TrackingConfidence.High;

        bool indexExtended = hand.GetFingerConfidence(OVRHand.HandFinger.Index) == OVRHand.TrackingConfidence.High;
        bool middleExtended = hand.GetFingerConfidence(OVRHand.HandFinger.Middle) == OVRHand.TrackingConfidence.High;
        bool ringExtended = hand.GetFingerConfidence(OVRHand.HandFinger.Ring) == OVRHand.TrackingConfidence.High;

        // 只有拇指、小拇指伸直，其他三指都是縮的
        return thumbExtended && pinkyExtended && !indexExtended && !middleExtended && !ringExtended;
    }
}
