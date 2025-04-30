using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AchievementListUI : MonoBehaviour
{
    public GameObject itemTemplate; // 指向 AchievementItemTemplate
    public Transform contentParent1; // 指向第一個 AchievementListPanel
    public Transform contentParent2; // 指向第二個 AchievementListPanel

    // 測試資料（正式使用可從外部傳入）
    public AchieveSystem achieveSystem;
    private Dictionary<string, int> achievements;

    void Start()
    {}

    public void PopulateAchievements()
    {
        achievements = achieveSystem.GetAchievements(); // 獲取成就資料
        if (achievements == null || achievements.Count == 0)
        {
            Debug.LogWarning("沒有成就資料可顯示！");
            return;
        }

        // Clear both panels before populating
        foreach (Transform child in contentParent1)
        {
            if (child != itemTemplate.transform)
                Destroy(child.gameObject);
        }

        foreach (Transform child in contentParent2)
        {
            if (child != itemTemplate.transform)
                Destroy(child.gameObject);
        }

        // Split the achievements into two parts
        List<KeyValuePair<string, int>> achievementList = new List<KeyValuePair<string, int>>(achievements);
        int halfCount = achievementList.Count / 2;

        // First half to contentParent1
        for (int i = 0; i < halfCount; i++)
        {
            GameObject item = Instantiate(itemTemplate, contentParent1);
            item.SetActive(true);

            TextMeshProUGUI tmp = item.GetComponent<TextMeshProUGUI>();
            tmp.text = $"{achievementList[i].Key} - {(achievementList[i].Value == 1 ? " 已完成" : " 未完成")}";
        }

        // Second half to contentParent2
        for (int i = halfCount; i < achievementList.Count; i++)
        {
            GameObject item = Instantiate(itemTemplate, contentParent2);
            item.SetActive(true);

            TextMeshProUGUI tmp = item.GetComponent<TextMeshProUGUI>();
            tmp.text = $"{achievementList[i].Key} - {(achievementList[i].Value == 1 ? " 已完成" : " 未完成")}";
        }
    }
}
