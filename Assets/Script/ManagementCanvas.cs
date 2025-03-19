using UnityEngine;
using System.Collections;
using TMPro;

public class ManagementCanvas : MonoBehaviour
{

    [Header("เมนูหลักแต่ละหน้า")]
    public GameObject LoginGoogle;
    public GameObject mainMenu;
    public GameObject secondMenu;
    public GameObject modeMenu;
    public GameObject saveMenu;
    public GameObject Mannual;
    public GameObject Practice;

    [Header("เมนูอื่นๆ")]
    public GameObject UiNotifyLogin;
    public GameObject UiNotifyErrorLogin;
    public GameObject UiNotifyLogout;
    public GameObject UiNotifySave;
    public GameObject UiNotifyConfirmSave;
    public GameObject UiNotifyLoad;
    public GameObject UiNotifyDelete;
    public GameObject UiNotifyConfirmDelete;

    [Header("UI สำหรับแสดง UID")]
    public TMP_Text statussaveloaddelete;  // ให้กำหนดอ้างอิงไปยัง TextMeshPro Text ที่จะแสดง UID
    public TMP_Text uidText;               // ให้กำหนดอ้างอิงไปยัง TextMeshPro Text ที่จะแสดง UID

    [Header("Object พิเศษ")]
    public GameObject simulatorObject1;
    public GameObject simulatorObject2;
    // ------------------ เพิ่มส่วนนี้ ------------------
    [Header("XR Origin")]
    [Tooltip("ลาก GameObject ที่เป็น XR Origin (หรือ XR Rig) มาใส่")]
    public GameObject xrOriginObject;
    // --------------------------------------------------
    private string userId;


    private GameObject currentMenu = null; // เก็บหน้าเมนูที่เปิดอยู่ปัจจุบัน

    void Start()
    {

        // เริ่มต้นเปิดหน้า LoginGoogle
        ShowLoginGoogle();
        //ShowMainMenu();
        // ตัวอย่างการเซ็ต Active ของ simulatorObject
        if (simulatorObject1 != null)
            simulatorObject1.SetActive(true);
        if (simulatorObject2 != null)
            simulatorObject2.SetActive(true);
    }

    // -------------------------------------------------------------------
    //  ฟังก์ชัน Fade
    // -------------------------------------------------------------------

    IEnumerator FadeOut(GameObject menu, float duration = 0.3f)
    {
        if (menu == null) yield break;

        CanvasGroup cg = menu.GetComponent<CanvasGroup>();
        if (cg == null) yield break; // ถ้าไม่มี CanvasGroup ให้ข้าม

        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
            yield return null;
        }

        cg.alpha = 0f;
        menu.SetActive(false);
    }

    IEnumerator FadeIn(GameObject menu, float duration = 0.5f)
    {
        if (menu == null) yield break;

        // เปิด GameObject ก่อน
        menu.SetActive(true);

        CanvasGroup cg = menu.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 1f, time / duration);
            yield return null;
        }

        cg.alpha = 1f;
    }

    // -------------------------------------------------------------------
    //  ฟังก์ชันเปลี่ยนหน้า (แสดงเมนู)
    // -------------------------------------------------------------------

    private void ShowMenu(GameObject nextMenu)
    {
        // ถ้ามีหน้าเดิม ให้ FadeOut หน้าเดิมก่อน
        if (currentMenu != null && currentMenu != nextMenu)
        {
            StartCoroutine(FadeOut(currentMenu));
        }

        // แล้ว FadeIn หน้าใหม่
        if (nextMenu != null)
        {
            StartCoroutine(FadeIn(nextMenu));
            currentMenu = nextMenu;
        }
        else
        {
            currentMenu = null;
        }
    }

    public void ShowLoginGoogle()
    {
        ShowMenu(LoginGoogle);
    }

    public void ShowMainMenu()
    {
        ShowMenu(mainMenu);
    }

    public void ShowSecondMenu()
    {
        ShowMenu(secondMenu);
    }

    public void ShowModeMenu()
    {
        ShowMenu(modeMenu);
    }

    public void ShowSaveMenu()
    {
        ShowMenu(saveMenu);
    }

    public void ShowManual()
    {
        ShowMenu(Mannual);
    }

    public void Showpractice()
    {
        ShowMenu(Practice);
        // if (xrOriginObject != null)
        // {
        //     xrOriginObject.transform.position = new Vector3(-206.8364f, -93f, 241.2679f);
        // }
        simulatorObject1.SetActive(false);
        simulatorObject2.SetActive(false);
    }

    // เมนูแจ้งเตือน Login ที่จะใช้แสดง UID ที่ได้รับมา
    public void ShowUiNotifyLogin()
    {
        ShowMenu(UiNotifyLogin);
        // ดึงค่า userId จาก PlayerPrefs แล้วแสดงบน Text
        //string uid = PlayerPrefs.GetString("userId", "Unknown UID");
        if (uidText != null)
        {
            uidText.text = "Welcome :" + userId;
        }
    }

    public void ShowUiNotifyErrorLogin()
    {
        ShowMenu(UiNotifyErrorLogin);
    }

    public void ShowUiNotifyLogOut()
    {
        ShowMenu(UiNotifyLogout);
    }

    public void ShowUiNotifySaveSuccess()
    {
        ShowMenu(UiNotifySave);
    }

    public void ShowUiNotifyConfrimSave()
    {
        ShowMenu(UiNotifyConfirmSave);
    }

    public void ShowUiNotifyLoadSuccess()
    {
        ShowMenu(UiNotifyLoad);
    }

    public void ShowUiNotifyDelete()
    {
        ShowMenu(UiNotifyDelete);
    }

    public void ShowUiNotifyConfrimDelete()
    {
        ShowMenu(UiNotifyConfirmDelete);
    }

    // -------------------------------------------------------------------
    //  ฟังก์ชันปุ่มใน Mode Menu
    // -------------------------------------------------------------------

    public void ModeMenuButton1()
    {
        Debug.Log("Mode Menu Button #1 Pressed (Do nothing yet)");
    }

    public void ModeMenuButton3()
    {
        if (simulatorObject1 != null)
            simulatorObject1.SetActive(false);
        if (simulatorObject2 != null)
            simulatorObject2.SetActive(false);

        ShowModeMenu();
    }

    // -------------------------------------------------------------------
    //  อื่น ๆ
    // -------------------------------------------------------------------
    public void UpdateUserId(string newUserId)
    {
        userId = newUserId;
        // เก็บลง PlayerPrefs เผื่อใช้ข้ามซีน หรือเปิด-ปิดโปรแกรม
        PlayerPrefs.SetString("userId", userId);
        PlayerPrefs.Save();

        Debug.Log("✅ Updated userId in ManagementCanvas: " + userId);
    }
    public void ExitApplication()
    {
        Debug.Log("Exit button pressed!");
        Application.Quit();
    }


}
