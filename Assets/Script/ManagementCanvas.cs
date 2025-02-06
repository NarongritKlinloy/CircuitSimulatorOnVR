using UnityEngine;

public class ManagementCanvas : MonoBehaviour
{
    public GameObject mainMenu;   // หน้าแรก
    public GameObject secondMenu; // หน้าที่ 2
    public GameObject modeMenu;   // หน้าที่ 3
    public GameObject saveMenu;   // หน้าที่ 4

    void Start()
    {
        ShowMainMenu(); // เริ่มต้นที่หน้าแรก
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        secondMenu.SetActive(false);
        modeMenu.SetActive(false);
        saveMenu.SetActive(false);
    }

    public void ShowSecondMenu()
    {
        mainMenu.SetActive(false);
        secondMenu.SetActive(true);
        modeMenu.SetActive(false);
        saveMenu.SetActive(false);
    }

    public void ShowModeMenu()
    {
        mainMenu.SetActive(false);
        secondMenu.SetActive(false);
        modeMenu.SetActive(true);
        saveMenu.SetActive(false);
    }

    public void ShowSaveMenu()
    {
        mainMenu.SetActive(false);
        secondMenu.SetActive(false);
        modeMenu.SetActive(false);
        saveMenu.SetActive(true);
    }

    public void ExitApplication()
    {
        Debug.Log("Exit button pressed!"); // สำหรับ Debug ใน Unity Console
        Application.Quit(); // ปิดแอป (ใช้ได้จริงตอนรันเป็น Build)
    }
}
