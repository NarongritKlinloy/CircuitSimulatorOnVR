using UnityEngine;

public class ManagementCanvas : MonoBehaviour
{
    [Header("เมนูหลักแต่ละหน้า")]
    public GameObject mainMenu;   // หน้าแรก
    public GameObject secondMenu; // หน้าที่ 2
    public GameObject modeMenu;   // หน้าที่ 3
    public GameObject saveMenu;   // หน้าที่ 4

    [Header("Object ที่จะเปิดเมื่อกดปุ่มที่ 2 ใน Mode Menu")]
    public GameObject mySpecialObject;

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
        mySpecialObject.SetActive(false);
    }

    public void ShowSecondMenu()
    {
        mainMenu.SetActive(false);
        secondMenu.SetActive(true);
        modeMenu.SetActive(false);
        saveMenu.SetActive(false);
        mySpecialObject.SetActive(false);
    }

    public void ShowModeMenu()
    {
        mainMenu.SetActive(false);
        secondMenu.SetActive(false);
        modeMenu.SetActive(true);
        saveMenu.SetActive(false);
        mySpecialObject.SetActive(false);
    }

    public void ShowSaveMenu()
    {
        mainMenu.SetActive(false);
        secondMenu.SetActive(false);
        modeMenu.SetActive(false);
        saveMenu.SetActive(true);
        mySpecialObject.SetActive(false);
    }

    //===================== ปุ่มต่าง ๆ ใน Mode Menu =====================//

    // ปุ่มแรก ยังไม่ต้องทำอะไร
    public void ModeMenuButton1()
    {
        Debug.Log("Mode Menu Button #1 Pressed (Do nothing yet)");
        // ไว้เพิ่มเติมฟังก์ชันภายหลังได้
    }

    // ปุ่มที่ 2: สั่งเปิด (หรือปิด) Object ที่ต้องการ
    public void ModeMenuButton2()
    {
        mainMenu.SetActive(false);
        secondMenu.SetActive(false);
        modeMenu.SetActive(false);
        saveMenu.SetActive(false);
        mySpecialObject.SetActive(true);
    }

    //===============================================================//

    public void ExitApplication()
    {
        Debug.Log("Exit button pressed!"); // สำหรับ Debug ใน Unity Console
        Application.Quit(); // ปิดแอป (ใช้ได้จริงตอนรันเป็น Build)
    }
}
