using UnityEngine;

public class ManagementCanvas : MonoBehaviour
{
    [Header("เมนูหลักแต่ละหน้า")]
    public GameObject LoginGoogle;
    public GameObject mainMenu;   // หน้าแรก
    public GameObject secondMenu; // หน้าที่ 2
    public GameObject modeMenu;   // หน้าที่ 3
    public GameObject saveMenu;   // หน้าที่ 4

    public GameObject Mannual;    // หน้าที่ 5

    [Header("Object ที่จะเปิดเมื่อกดปุ่มที่ 2 ใน Mode Menu")]
    public GameObject mySpecialObject;

    [Header("Object ที่จะเปิดเมื่อกดปุ่ม Simulator")]
    public GameObject simulatorObject1;
    public GameObject simulatorObject2;

    void Start()
    {
        ShowLoginGoogle(); // เริ่มต้นที่หน้าแรก
        if (simulatorObject1 != null)
            simulatorObject1.SetActive(true);
        if (simulatorObject2 != null)
            simulatorObject2.SetActive(true);
    }

    private void ResetAllMenus()
    {
        // รีเซ็ตค่าเริ่มต้นของแต่ละหน้า โดยตรวจสอบ null ก่อนเรียก SetActive()
        if (LoginGoogle != null) LoginGoogle.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(false);
        if (secondMenu != null) secondMenu.SetActive(false);
        if (modeMenu != null) modeMenu.SetActive(false);
        if (saveMenu != null) saveMenu.SetActive(false);
        if (mySpecialObject != null) mySpecialObject.SetActive(false);
        if (Mannual != null) Mannual.SetActive(false);
    }
    
    public void ShowMainMenu()
    {
        ResetAllMenus(); // รีเซ็ตก่อนเปิดหน้าใหม่
        if (mainMenu != null)
            mainMenu.SetActive(true);
    }
    
    public void ShowLoginGoogle()
    {
        ResetAllMenus(); // รีเซ็ตก่อนเปิดหน้าใหม่
        if (LoginGoogle != null)
            LoginGoogle.SetActive(true);
    }

    public void ShowSecondMenu()
    {
        ResetAllMenus();
        if (secondMenu != null)
            secondMenu.SetActive(true);
    }

    public void ShowModeMenu()
    {
        ResetAllMenus();
        if (modeMenu != null)
            modeMenu.SetActive(true);
    }

    public void ShowSaveMenu()
    {
        ResetAllMenus();
        if (saveMenu != null)
            saveMenu.SetActive(true);
    }

    public void ShowManual()
    {
        ResetAllMenus();
        if (Mannual != null)
            Mannual.SetActive(true);
    }

    //===================== ปุ่มต่าง ๆ ใน Mode Menu =====================//

    // ปุ่มแรก ยังไม่ต้องทำอะไร
    public void ModeMenuButton1()
    {
        Debug.Log("Mode Menu Button #1 Pressed (Do nothing yet)");
    }

    // ปุ่มที่ 2: เปิด mySpecialObject และปิดเมนูทั้งหมด
    public void ModeMenuButton2()
    {
        ResetAllMenus();
        if (mySpecialObject != null)
            mySpecialObject.SetActive(true);
        if (simulatorObject1 != null)
            simulatorObject1.SetActive(true); // ปิด wall circuit 
        if (simulatorObject2 != null)
            simulatorObject2.SetActive(false); // เปิด wall digital
    }

    // ปุ่มที่ 3: เปิด simulatorObject1 และ simulatorObject2 แล้วกลับไป Mode Menu
    public void ModeMenuButton3()
    {
        if (simulatorObject1 != null)
            simulatorObject1.SetActive(false);
        if (simulatorObject2 != null)
            simulatorObject2.SetActive(false);
        ShowModeMenu(); // กลับไปที่ Mode Menu
    }

    //===============================================================//

    public void ExitApplication()
    {
        Debug.Log("Exit button pressed!");
        Application.Quit();
    }
} 