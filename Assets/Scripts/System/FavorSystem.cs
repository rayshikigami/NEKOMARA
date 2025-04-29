using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class FavorSystem : MonoBehaviour
{
    // Start is called before the first frame updatevoid Start()

    public Dictionary<String, int> favorDict, adoptDict;
    public static string version = "0";

    AchieveSystem achieveSystem;
    void Start()
    {
        favorDict = new Dictionary<string, int>();
        adoptDict = new Dictionary<string, int>();
        achieveSystem = FindObjectOfType<AchieveSystem>();
        InitSaveIfNeeded("XR_CAMP 成就系統格式 - favor.csv", "favor.csv");
        LoadFavor("favor.csv");
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
    public void LoadFavor(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"找不到檔案：{path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        favorDict.Clear();
        adoptDict.Clear();
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
                if (int.TryParse(parts[1], out int value))
                {
                    favorDict[parts[0]] = value;
                    adoptDict[parts[0]] = value;
                }
            }
        }

        Debug.Log($"成功載入進度資料，共 {favorDict.Count} 筆");
    }


    //******************************************************
    public void Savefavor(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        using (StreamWriter writer = new StreamWriter(path, false)) // false = 覆蓋寫入
        {
            // 寫入版本號（放第一行）
            writer.WriteLine($"version,{version}");

            // 寫入所有進度資料
            foreach (var pair in favorDict)
            {
                writer.WriteLine($"{pair.Key},{pair.Value},{adoptDict[pair.Key]}");
            }
        }

        Debug.Log($"已儲存進度到 {fileName}（{favorDict.Count} 筆）");
    }

    //******************************************************
    public void AddFavor(string catname, int count)
    {
        favorDict[catname] += count;
        Savefavor("favor.csv");
        if (favorDict[catname] >= 30)//好感大於30者收編
        {
            SetAdopt(catname);
        }
    }

    public int GetFavor(string catname)
    {
        return favorDict[catname];
    }

    public void SetAdopt(string catname)
    {
        adoptDict[catname] = 1;
        achieveSystem.UpdateProgress("adopt", 1);
        Savefavor("favor.csv");
        //開放使用對應貓窩
    }

}
