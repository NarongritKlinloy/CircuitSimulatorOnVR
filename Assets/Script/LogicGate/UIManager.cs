using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject firstUIPanel;  // UI หน้าแรก
    public GameObject secondUIPanel; // UI หน้าที่สอง

    private void Start()
    {
        ShowFirstUI(); // เปิด UI หน้าแรกเป็นค่าเริ่มต้น
    }

    public void ShowFirstUI()
    {
        firstUIPanel.SetActive(true);
        secondUIPanel.SetActive(false);
    }

    public void ShowSecondUI()
    {
        firstUIPanel.SetActive(false);
        secondUIPanel.SetActive(true);
    }
}
