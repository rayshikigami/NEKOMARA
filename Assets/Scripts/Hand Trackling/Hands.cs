using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.PoseDetection; // 引入 Oculus 互動套件中姿勢偵測相關功能

public class Hands : MonoBehaviour
{
    // 宣告手部模型（OVRHand）和骨架（OVRSkeleton）兩個公開變數，可從 Inspector 指派
    public OVRHand hand;
    public OVRSkeleton skeleton;

    // Start 在遊戲開始時執行
    void Start()
    {
        // 目前 Start 尚未設定任何初始動作
    }

    // Update 每一幀 (frame) 都會被呼叫一次
    void Update()
    {
        Debug.LogError("CCCCC"); // 每幀輸出 "CCCCC" 至 Console，表示 Update 有正常跑
        
        // 判斷是否某個手指彎曲（目前註解掉的 IsSix() 是檢查特殊手勢）
        // if (IsSix())
        // { Debug.LogError("AAAAA"); }
        
        // 呼叫 IsFingerCurled 方法，檢查指定的骨頭是否達到彎曲條件
        if (IsFingerCurled(OVRSkeleton.BoneId.Hand_Index1, OVRSkeleton.BoneId.Hand_Index2, OVRSkeleton.BoneId.Hand_Index3))
        {
            Debug.LogError("AAAAABBBBBB"); // 若符合彎曲條件，輸出 "AAAAABBBBBB"
        }
    }

    //********手勢偵測區塊********

    // 底下這段 IsSix() 是判斷手勢是否為「比6」（類似比出電話手勢 🤙）
    /*
    bool IsSix()
    {
        // 比6手勢定義：大拇指與小指伸直，其餘手指彎曲
        if (!GetFingerIsCurling(OVRHand.HandFinger.Thumb) &&
            GetFingerIsCurling(OVRHand.HandFinger.Index) &&
            GetFingerIsCurling(OVRHand.HandFinger.Middle) &&
            GetFingerIsCurling(OVRHand.HandFinger.Ring) &&
            !GetFingerIsCurling(OVRHand.HandFinger.Pinky))
        {
            return true;
        }
        return false;
    }
    */

    // 判斷某隻手指是否彎曲
    /*
    bool GetFingerIsCurling(OVRHand.HandFinger finger)
    {
        string s = "2"; // 預設字串（此處暫時寫死）
        // 通常會呼叫 fingerFeatureStateProvider 來獲得手指狀態 (此處缺少實際程式)
        
        // print(s);
        // 回傳 true 表示彎曲，false 表示伸直
        return (s == "2" ? true : false);
    }
    */

    // 判斷三段骨頭形成的夾角是否超過門檻值，來判定手指是否彎曲
    public bool IsFingerCurled(OVRSkeleton.BoneId proximalId, OVRSkeleton.BoneId intermediateId, OVRSkeleton.BoneId distalId, float angleThreshold = 45f)
    {
        // 若骨架無效或資料不可信，直接返回 false
        if (skeleton == null || !skeleton.IsDataValid || !skeleton.IsDataHighConfidence)
            return false;

        // 取得手指三段骨頭 Transform（近端節、近中端節、遠端節）
        Transform proximal = GetBoneTransform(proximalId);
        Transform intermediate = GetBoneTransform(intermediateId);
        Transform distal = GetBoneTransform(distalId);

        // 如果其中任何一段骨頭資料是 null，則回傳 false
        if (proximal == null || intermediate == null || distal == null)
            return false;

        // 計算第一段骨頭的方向向量
        Vector3 dir1 = (intermediate.position - proximal.position).normalized;
        // 計算第二段骨頭的方向向量
        Vector3 dir2 = (distal.position - intermediate.position).normalized;

        // 計算這兩個向量之間的夾角
        float angle = Vector3.Angle(dir1, dir2);

        // 如果夾角大於設定的門檻值（預設 45度），則認定為彎曲
        return angle > angleThreshold;
    }

    // 根據 BoneId 取得骨架中對應的 Transform
    private Transform GetBoneTransform(OVRSkeleton.BoneId boneId)
    {
        // 遍歷整個骨架中的骨頭列表
        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == boneId)
                return bone.Transform; // 找到對應 BoneId 就回傳它的 Transform
        }
        return null; // 若沒找到則回傳 null
    }
}
