using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;               // สำหรับ TMP_Dropdown
using UnityEngine.Networking;
using System;

[Serializable]
public class SimulatorSaveItem
{
    public long circuit_id;
    public string circuit_name;
    public string circuit_date;
   
}

public static class JsonArrayHelper
{
    public static List<T> FromJson<T>(string json)
    {
        string newJson = "{ \"Items\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.Items;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }
}

public class SaveLoadDropdownManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Dropdown ตัวเดียวสำหรับ Save, Load, และ Delete")]
    public TMP_Dropdown saveDropdown; // ใช้เป็น dropdown สำหรับเลือกเซฟ (option[0] = New Save)

    [Header("Script References")]
    public CombinedSaveLoadManager saveLoadManager; // สคริปต์ Save/Load แบบรวม
    public SoundManager soundManager;               // SoundManager สำหรับเล่นเสียงปุ่ม

    [Header("Management Canvas Reference")]
    public ManagementCanvas managementCanvas;         // สำหรับเรียกให้แสดง UI Notify ต่างๆ

    [Header("Status Texts")]
    public TMP_Text statusSave;    // สำหรับแสดงสถานะของการ Save
    public TMP_Text statusLoad;    // สำหรับแสดงสถานะของการ Load
    public TMP_Text statusDelete;  // สำหรับแสดงสถานะของการ Delete

    // เก็บรายการเซฟทั้งหมด (circuit_id, name, date)
    private List<SimulatorSaveItem> allUserSaves = new List<SimulatorSaveItem>();
    private float refreshInterval = 5f;

    // เก็บรายการเซฟที่เลือกไว้เมื่อมีการยืนยัน (overwrite หรือ delete)
    private SimulatorSaveItem selectedSaveForConfirm = null;

    void Start()
    {
        // ดึงรายการเซฟตอนเริ่มต้น (ไม่รักษา selection)
        StartCoroutine(GetAllSavesFromServer(keepSelection: false));
        StartCoroutine(AutoRefreshLoop());
    }

    private IEnumerator AutoRefreshLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            StartCoroutine(GetAllSavesFromServer(keepSelection: true));
        }
    }

    private IEnumerator GetAllSavesFromServer(bool keepSelection)
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("No userId in PlayerPrefs => cannot fetch saves from server.");
            yield break;
        }

        string url = "https://smith11.ce.kmitl.ac.th/api/simulator/listSavesDigital?userId=" + userId;
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get list of saves failed: " + req.error);
            }
            else
            {
                string jsonText = req.downloadHandler.text;
                var newList = JsonArrayHelper.FromJson<SimulatorSaveItem>(jsonText);
                // หาก newList เป็น null ให้ใช้ empty list
                if (newList == null)
                {
                    newList = new List<SimulatorSaveItem>();
                }
                allUserSaves = newList;
                UpdateDropdown(keepSelection);
            }
        }
    }

    private void UpdateDropdown(bool keepSelection)
    {
        int oldIndex = saveDropdown.value;
        long oldSelectedId = -1;
        if (oldIndex > 0 && oldIndex - 1 < allUserSaves.Count)
        {
            oldSelectedId = allUserSaves[oldIndex - 1].circuit_id;
        }

        saveDropdown.options.Clear();
        // ตัวเลือกแรก: "New Save"
        saveDropdown.options.Add(new TMP_Dropdown.OptionData("New Save"));
        foreach (var item in allUserSaves)
        {
            string displayText = $"{item.circuit_name} ({item.circuit_date})";
            saveDropdown.options.Add(new TMP_Dropdown.OptionData(displayText));
        }
        if (keepSelection && oldSelectedId != -1)
        {
            int newIndex = allUserSaves.FindIndex(s => s.circuit_id == oldSelectedId);
            saveDropdown.value = (newIndex >= 0) ? newIndex + 1 : 0;
        }
        else
        {
            saveDropdown.value = 0;
        }
        saveDropdown.RefreshShownValue();
    }

    // ---------- Save ----------
    public void OnClick_ConfirmSave()
    {
        if (soundManager != null)
            soundManager.PlayButtonSound();

        int index = saveDropdown.value;
        if (index == 0)
        {
            // New Save: ทำการเซฟทันที
            Debug.Log("User chooses to create a New Save");
            saveLoadManager.SaveCombined();
            statusSave.text = "Save new data Success!";
            // หลัง save สำเร็จ ให้แสดง Notify ใน ManagementCanvas
            managementCanvas.ShowUiNotifySaveSuccess();
        }
        else
        {
            // ถ้าเลือกเซฟเก่า -> เป็นการ Overwrite
            int dataIndex = index - 1;
            if (dataIndex >= 0 && dataIndex < allUserSaves.Count)
            {
                selectedSaveForConfirm = allUserSaves[dataIndex];
                Debug.Log($"User chooses to overwrite: {selectedSaveForConfirm.circuit_name} (ID={selectedSaveForConfirm.circuit_id})");
                // แสดงหน้าต่างยืนยัน overwrite
                managementCanvas.ShowUiNotifyConfrimSave();
                // ผู้ใช้จะต้องกดปุ่มยืนยันใน UI notify confirm ซึ่งเรียก OnConfirmOverwriteSave()
            }
        }
    }

    // ฟังก์ชันที่เรียกจากปุ่มใน UI Notify Confirm Save
    public void OnConfirmOverwriteSave()
    {
        if (selectedSaveForConfirm != null)
        {
            saveLoadManager.UpdateCombined(selectedSaveForConfirm.circuit_id);
            statusSave.text = "Overwriting save data Success!";
            managementCanvas.ShowUiNotifySaveSuccess();
            selectedSaveForConfirm = null; // เคลียร์ค่า
        }
    }


    // ---------- Load ----------
    public void OnClick_ConfirmLoad()
    {
        if (soundManager != null)
            soundManager.PlayButtonSound();

        int index = saveDropdown.value;
        if (index == 0)
        {
            Debug.Log("No save chosen to load.");
            statusLoad.text = "No save selected for loading.";
            return;
        }
        int dataIndex = index - 1;
        if (dataIndex >= 0 && dataIndex < allUserSaves.Count)
        {
            var chosen = allUserSaves[dataIndex];
            Debug.Log($"User chooses to load: {chosen.circuit_name} (ID={chosen.circuit_id})");
            saveLoadManager.LoadCombinedById(chosen.circuit_id);
            statusLoad.text = "Loading save data Success!";
            managementCanvas.ShowUiNotifyLoadSuccess();
        }
    }

    // ---------- Delete ----------
    public void OnClick_ConfirmDelete()
    {
        if (soundManager != null)
            soundManager.PlayButtonSound();

        int index = saveDropdown.value;
        if (index == 0)
        {
            Debug.Log("No save chosen to delete.");
            statusDelete.text = "No save selected for deletion.";
            return;
        }
        int dataIndex = index - 1;
        if (dataIndex >= 0 && dataIndex < allUserSaves.Count)
        {
            selectedSaveForConfirm = allUserSaves[dataIndex];
            Debug.Log($"User chooses to delete: {selectedSaveForConfirm.circuit_name} (ID={selectedSaveForConfirm.circuit_id})");
            // แสดงหน้าต่างยืนยันการลบ
            managementCanvas.ShowUiNotifyConfrimDelete();
            // ผู้ใช้จะต้องกดปุ่มยืนยันใน UI notify confirm delete ซึ่งเรียก OnConfirmDelete()
        }
    }

    // ฟังก์ชันที่เรียกจากปุ่มใน UI Notify Confirm Delete
    public void OnConfirmDelete()
    {
        if (selectedSaveForConfirm != null)
        {
            StartCoroutine(DeleteSaveFromServer(selectedSaveForConfirm.circuit_id));
            statusDelete.text = "Delete save data Success!";
            managementCanvas.ShowUiNotifyDelete();
            selectedSaveForConfirm = null;
        }
    }

    private IEnumerator DeleteSaveFromServer(long saveId)
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("No userId => cannot delete save from server.");
            yield break;
        }

        string url = $"https://smith11.ce.kmitl.ac.th/api/simulator/deleteById?userId={userId}&saveId={saveId}";
        using (UnityWebRequest req = UnityWebRequest.Delete(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Delete save failed: " + req.error);
                statusDelete.text = "Delete failed: " + req.error;
            }
            else
            {
                Debug.Log("Delete success => " + req.downloadHandler.text);
                statusDelete.text = "Delete successful!";
                StartCoroutine(GetAllSavesFromServer(keepSelection: false));
            }
        }
    }
}