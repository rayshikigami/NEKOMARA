using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public class AchieveSystem : MonoBehaviour
{
    // Start is called before the first frame update
    //public TextAsset progressCSV, AchieveCSV;
    public Dictionary<String, int> progressData, AchieveID;
    public List<int> AchieveDone;
    public List<string> AchieveDiscribe;
    public static string version = "0";

    void Start()
    {
        progressData = new Dictionary<string, int>();
        AchieveID = new Dictionary<string, int>();
        InitSaveIfNeeded("XR_CAMP 成就系統格式 - achieve.csv", "achieve.csv");
        InitSaveIfNeeded("XR_CAMP 成就系統格式 - progress.csv", "progress.csv");
        LoadAchieve("achieve.csv");
        LoadProgress("progress.csv");
    }

    // Update is called once per frame
    void Update()
    {

    }
    public static void InitSaveIfNeeded(string defaultFileName, string saveFileName)
    {
        string defaultPath = Path.Combine(Application.streamingAssetsPath, defaultFileName);
        string savePath = Path.Combine(Application.persistentDataPath, saveFileName);

        // 如果沒有存檔 → 直接複製
        if (!File.Exists(savePath))
        {
            Debug.Log($"找不到存檔 {saveFileName}，從預設複製...");
            File.Copy(defaultPath, savePath);
            return;
        }

        // 比對版本號
        String sv = File.ReadAllLines(savePath)[0].Trim().Split(',')[1];
        String dv = File.ReadAllLines(defaultPath)[0].Trim().Split(',')[1];
        version = dv;
        if (dv != sv)
        {
            Debug.LogWarning($"版本不符（預設 {dv} ≠ 存檔 {sv}），重設為預設值");
            File.Copy(defaultPath, savePath, overwrite: true);
        }
    }
    public void LoadProgress(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"找不到檔案：{path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        progressData.Clear();
        //跳過版本檢查
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length != 2) continue;


            else
            {
                // 儲存進度資料
                if (int.TryParse(parts[1], out int value))
                {
                    progressData[parts[0]] = value;
                }
            }
        }

        Debug.Log($"成功載入進度資料，共 {progressData.Count} 筆");
    }
    public void LoadAchieve(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        print(path);
        if (!File.Exists(path))
        {
            Debug.LogError($"找不到檔案：{path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        AchieveID.Clear();
        AchieveDone.Clear();
        AchieveDiscribe.Clear();
        //跳過版本檢查
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length != 3) continue;
            else
            {
                // 儲存進度資料

                AchieveDone.Add(int.Parse(parts[2]));//0未達成 1達成
                AchieveDiscribe.Add(parts[1]);
                AchieveID[parts[0]] = AchieveDone.Count - 1;//index


            }
        }

        Debug.Log($"成功載入進度資料，共 {AchieveID.Count} 筆");
    }

    //******************************************************
    public void SaveProgress(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        using (StreamWriter writer = new StreamWriter(path, false)) // false = 覆蓋寫入
        {
            // 寫入版本號（放第一行）
            writer.WriteLine($"version,{version}");

            // 寫入所有進度資料
            foreach (var pair in progressData)
            {
                writer.WriteLine($"{pair.Key},{pair.Value}");
            }
        }

        Debug.Log($"已儲存進度到 {fileName}（{progressData.Count} 筆）");
    }
    public void SaveAchieve(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        using (StreamWriter writer = new StreamWriter(path, false)) // false = 覆蓋寫入
        {
            // 寫入版本號（放第一行）
            writer.WriteLine($"version,{version}");

            // 寫入所有進度資料
            foreach (var id in AchieveID)
            {
                writer.WriteLine($"{id.Key},{AchieveDiscribe[id.Value]},{AchieveDone[id.Value]}");
            }
        }

        Debug.Log($"已儲存進度到 {fileName}（{progressData.Count} 筆）");
    }
    //******************************************************
    public void UpdateProgress(string action, int count)
    {
        progressData[action] += count;
        SaveProgress("progress.csv");
        CheckAchieve(action);

    }
    public void CheckAchieve(string action)
    {
        //檢查是否達成成就
        switch (action)
        {
            case "touch":
                if (progressData[action] >= 1)
                {
                    AchieveDone[AchieveID["毛"]] = 1;
                    SaveAchieve("achieve.csv");
                }
                break;
            case "summon":
                if (progressData[action] >= 1)
                {
                    AchieveDone[AchieveID["召喚"]] = 1;
                    SaveAchieve("achieve.csv");
                }
                break;
            case "feed":
                if (progressData[action] >= 1)
                {
                    AchieveDone[AchieveID["罐罐"]] = 1;
                    SaveAchieve("achieve.csv");
                }
                break;
            case "favor_up":
                if (progressData[action] >= 1)
                {
                    AchieveDone[AchieveID["好人"]] = 1;
                    SaveAchieve("achieve.csv");
                }
                break;
            case "adopt":
                if (progressData[action] >= 1)
                {
                    AchieveDone[AchieveID["從今以後這裡就是你家了"]] = 1;
                    SaveAchieve("achieve.csv");
                }
                break;
            case "escape":
                if (progressData[action] >= 1)
                {
                    AchieveDone[AchieveID["欸不是"]] = 1;
                    SaveAchieve("achieve.csv");
                }
                break;
            case "set_object":
                if (progressData[action] >= 1)
                {
                    AchieveDone[AchieveID["佈置我家"]] = 1;
                    SaveAchieve("achieve.csv");
                }
                break;
            case "easter_egg":
                if (progressData[action] >= 1)
                {
                    AchieveDone[AchieveID["唉呦這是"]] = 1;
                    SaveAchieve("achieve.csv");
                }
                break;
        }


    }
}
