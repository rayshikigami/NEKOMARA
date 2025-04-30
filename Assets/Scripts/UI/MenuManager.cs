using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public GameObject[] subMenus;

    public void ShowMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        foreach (GameObject menu in subMenus)
        {
            menu.SetActive(false);
        }
    }

    public void ShowSubMenu(int index)
    {
        mainMenuCanvas.SetActive(false);
        for (int i = 0; i < subMenus.Length; i++)
        {
            subMenus[i].SetActive(i == index);
        }
    }
}
