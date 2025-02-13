using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SaveLoadUI : MonoBehaviour
{
    public GameObject savePanel;
    public GameObject loadPanel;
    public Transform saveSlotContainer;
    public Transform loadSlotContainer;
    public GameObject saveSlotPrefab;
    public TMP_Text confirmationText;
    public GameObject confirmationDialog;

    private List<SaveSlot> saveSlots = new List<SaveSlot>();
    private Dictionary<int, string> localSaves = new Dictionary<int, string>(); // จำลองการเซฟข้อมูลในตัวเกม

    void Start()
    {
        LoadSaveSlots();
    }

    public void ShowSavePanel()
    {
        savePanel.SetActive(true);
        loadPanel.SetActive(false);
        LoadSaveSlots();
    }

    public void ShowLoadPanel()
    {
        savePanel.SetActive(false);
        loadPanel.SetActive(true);
        LoadSaveSlots();
    }

    private void LoadSaveSlots()
    {
        foreach (Transform child in saveSlotContainer) Destroy(child.gameObject);
        foreach (Transform child in loadSlotContainer) Destroy(child.gameObject);

        for (int i = 1; i <= 5; i++) // จำลองว่ามี 5 ช่องเซฟ
        {
            GameObject slot = Instantiate(saveSlotPrefab, savePanel.activeSelf ? saveSlotContainer : loadSlotContainer);
            SaveSlot slotScript = slot.GetComponent<SaveSlot>();
            slotScript.SetData(i, localSaves.ContainsKey(i), this);
            saveSlots.Add(slotScript);
        }
    }

    public void ShowConfirmation(string message, SaveSlot slot)
    {
        confirmationDialog.SetActive(true);
        confirmationText.text = message;
        confirmationDialog.GetComponent<ConfirmationDialog>().Setup(slot);
    }

    public void SaveGame(int slotID)
    {
        localSaves[slotID] = "GameData"; // บันทึกข้อมูลลง Dictionary จำลอง
        Debug.Log($"✅ บันทึกเกมที่ Slot {slotID}");
        LoadSaveSlots(); // รีเฟรช UI
    }

    public void LoadGame(int slotID)
    {
        if (localSaves.ContainsKey(slotID))
        {
            Debug.Log($"🎮 โหลดเกมจาก Slot {slotID}");
        }
    }
}
