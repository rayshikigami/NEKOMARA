using UnityEngine;
using UnityEngine.UI;  // 給 UI Text 用

public class HandGestureMenuController : MonoBehaviour
{
    public Hands leftHand;  // 換成 Hands 腳本 (從Inspector拉進來)
    public GameObject gestureMenuCanvas;
    public Text gestureStatusText; // 顯示手勢狀態文字

    private bool isMenuVisible = true;

    void Update()
    {
        if (leftHand == null || gestureMenuCanvas == null){
            Debug.LogError("請確保已正確設置左手和選單Canvas的參考。");
            return;
        }
        Debug.Log ("檢查手勢中...");

        bool isSixGesture = IsMakingSixGesture(leftHand);

        // 更新手勢偵測顯示
        if (gestureStatusText != null)
        {
            Debug.Log ("手勢：6");
        }

        // 根據手勢決定顯示或隱藏選單
        if (isSixGesture && !isMenuVisible)
        {
            gestureMenuCanvas.SetActive(true);
            isMenuVisible = true;
        }
        else if (!isSixGesture && isMenuVisible)
        {
            Debug.Log ("隱藏選單");
            gestureMenuCanvas.SetActive(false);
            isMenuVisible = false;
        }
    }

    // 改成用 Hands.cs 裡的 IsFingerCurled 方法來判斷
    bool IsMakingSixGesture(Hands handScript)
    {
        // 彎曲條件設定
        float angleThreshold = 45f; // 可以微調靈敏度

        bool thumbStraight = !handScript.IsFingerCurled(
            OVRSkeleton.BoneId.Hand_Thumb1,
            OVRSkeleton.BoneId.Hand_Thumb2,
            OVRSkeleton.BoneId.Hand_Thumb3,
            angleThreshold);

        bool pinkyStraight = !handScript.IsFingerCurled(
            OVRSkeleton.BoneId.Hand_Pinky1,
            OVRSkeleton.BoneId.Hand_Pinky2,
            OVRSkeleton.BoneId.Hand_Pinky3,
            angleThreshold);

        bool indexCurled = handScript.IsFingerCurled(
            OVRSkeleton.BoneId.Hand_Index1,
            OVRSkeleton.BoneId.Hand_Index2,
            OVRSkeleton.BoneId.Hand_Index3,
            angleThreshold);

        bool middleCurled = handScript.IsFingerCurled(
            OVRSkeleton.BoneId.Hand_Middle1,
            OVRSkeleton.BoneId.Hand_Middle2,
            OVRSkeleton.BoneId.Hand_Middle3,
            angleThreshold);

        bool ringCurled = handScript.IsFingerCurled(
            OVRSkeleton.BoneId.Hand_Ring1,
            OVRSkeleton.BoneId.Hand_Ring2,
            OVRSkeleton.BoneId.Hand_Ring3,
            angleThreshold);

        // 只有拇指、小拇指伸直，其他三指都是彎曲
        return thumbStraight && pinkyStraight && indexCurled && middleCurled && ringCurled;
    }
}
