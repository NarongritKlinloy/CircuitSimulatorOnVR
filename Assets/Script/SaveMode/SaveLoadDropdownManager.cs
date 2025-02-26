using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;               // สำหรับ TMP_Dropdown
using UnityEngine.Networking;
using System;

// เก็บข้อมูลแต่ละเซฟจาก DB
[Serializable]
public class SimulatorSaveItem
{
    public long simulate_id;
    public string simulate_name;
    public string simulate_date;
}

// Helper สำหรับแปลง JSON array -> List<T>
public static class JsonArrayHelper
{
    public static List<T> FromJson<T>(string json)
    {
        // ครอบ JSON array ด้วยคีย์สมมติ "Items"
        // เพื่อใช้ JsonUtility.FromJson() ได้
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
    public TMP_Dropdown saveDropdown;   // Dropdown สำหรับ Save
    public TMP_Dropdown loadDropdown;   // Dropdown สำหรับ Load

    // (ใหม่) เพิ่ม Dropdown สำหรับลบ
    public TMP_Dropdown deleteDropdown; // Dropdown สำหรับ Delete

    [Header("Script References")]
    public SaveLoadManager2 saveLoadManager; // อ้างอิงสคริปต์ Save/Load
    public SoundManager soundManager;        // ถ้ามี SoundManager ไว้เล่นเสียงปุ่ม

    // เก็บรายการเซฟทั้งหมด (simulate_id, name, date)
    private List<SimulatorSaveItem> allUserSaves = new List<SimulatorSaveItem>();

    // ตั้ง interval ให้รีเฟรชรายการทุก 5 วินาที
    private float refreshInterval = 5f;

    void Start()
    {
        // ดึงรายการเซฟตอนเริ่ม (โดยรักษา selection false)
        StartCoroutine(GetAllSavesFromServer(true));

        // เริ่ม coroutine ที่จะวน refresh ทุก 5 วิ
        StartCoroutine(AutoRefreshLoop());
    }

    // วนลูปทุกๆ 5 วินาทีเพื่อรีเฟรชรายการเซฟ
    private IEnumerator AutoRefreshLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            // refresh โดยพยายามรักษา selection เดิม
            StartCoroutine(GetAllSavesFromServer(keepSelection: true));
        }
    }

    /// <summary>
    /// ดึงรายการเซฟจากเซิร์ฟเวอร์
    /// keepSelection = จะพยายามรักษา selection เดิมใน dropdown
    /// </summary>
    private IEnumerator GetAllSavesFromServer(bool keepSelection = false)
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("No userId in PlayerPrefs => cannot fetch saves from server.");
            yield break;
        }

        string url = "http://localhost:5000/api/simulator/listSavesDigital?userId=" + userId;
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

                // อัปเดตรายการใน allUserSaves
                allUserSaves = newList;

                // อัปเดต dropdown
                UpdateDropdowns(keepSelection);
            }
        }
    }

    /// <summary>
    /// สร้าง/เคลียร์รายการใน saveDropdown, loadDropdown, deleteDropdown
    /// โดยพยายามรักษาค่า selection เดิม (ถ้า keepSelection=true)
    /// </summary>
    private void UpdateDropdowns(bool keepSelection)
    {
        // จำค่า selection เก่า
        int oldSaveIndex = saveDropdown.value;
        int oldLoadIndex = loadDropdown.value;
        int oldDeleteIndex = deleteDropdown.value;

        // เก็บ ID ที่เคยเลือกใน saveDropdown, loadDropdown, deleteDropdown
        long oldSaveId = -1;
        if (oldSaveIndex > 0 && oldSaveIndex - 1 < allUserSaves.Count)
        {
            oldSaveId = allUserSaves[oldSaveIndex - 1].simulate_id;
        }
        long oldLoadId = -1;
        if (oldLoadIndex > 0 && oldLoadIndex - 1 < allUserSaves.Count)
        {
            oldLoadId = allUserSaves[oldLoadIndex - 1].simulate_id;
        }
        long oldDeleteId = -1;
        if (oldDeleteIndex > 0 && oldDeleteIndex - 1 < allUserSaves.Count)
        {
            oldDeleteId = allUserSaves[oldDeleteIndex - 1].simulate_id;
        }

        // ---------- Populate Save Dropdown ----------
        saveDropdown.options.Clear();
        // option[0] = New Save
        saveDropdown.options.Add(new TMP_Dropdown.OptionData("New Save"));
        foreach (var item in allUserSaves)
        {
            string displayText = $"{item.simulate_name} ({item.simulate_date})";
            saveDropdown.options.Add(new TMP_Dropdown.OptionData(displayText));
        }

        // ---------- Populate Load Dropdown ----------
        loadDropdown.options.Clear();
        loadDropdown.options.Add(new TMP_Dropdown.OptionData("Select Save to Load"));
        foreach (var item in allUserSaves)
        {
            string displayText = $"{item.simulate_name} ({item.simulate_date})";
            loadDropdown.options.Add(new TMP_Dropdown.OptionData(displayText));
        }

        // ---------- Populate Delete Dropdown ----------
        deleteDropdown.options.Clear();
        deleteDropdown.options.Add(new TMP_Dropdown.OptionData("Select Save to Delete"));
        foreach (var item in allUserSaves)
        {
            string displayText = $"{item.simulate_name} ({item.simulate_date})";
            deleteDropdown.options.Add(new TMP_Dropdown.OptionData(displayText));
        }

        if (keepSelection)
        {
            // หา newIndex ของ oldSaveId
            if (oldSaveId != -1)
            {
                int newIndex = allUserSaves.FindIndex(s => s.simulate_id == oldSaveId);
                saveDropdown.value = (newIndex >= 0) ? newIndex + 1 : 0;
            }
            else
            {
                saveDropdown.value = 0;
            }

            // หา newIndex ของ oldLoadId
            if (oldLoadId != -1)
            {
                int newIndex = allUserSaves.FindIndex(s => s.simulate_id == oldLoadId);
                loadDropdown.value = (newIndex >= 0) ? newIndex + 1 : 0;
            }
            else
            {
                loadDropdown.value = 0;
            }

            // หา newIndex ของ oldDeleteId
            if (oldDeleteId != -1)
            {
                int newIndex = allUserSaves.FindIndex(s => s.simulate_id == oldDeleteId);
                deleteDropdown.value = (newIndex >= 0) ? newIndex + 1 : 0;
            }
            else
            {
                deleteDropdown.value = 0;
            }
        }
        else
        {
            // ไม่รักษา selection -> เซ็ตเป็น 0 ทุก dropdown
            saveDropdown.value = 0;
            loadDropdown.value = 0;
            deleteDropdown.value = 0;
        }

        // อัปเดต UI
        saveDropdown.RefreshShownValue();
        loadDropdown.RefreshShownValue();
        deleteDropdown.RefreshShownValue();
    }

    // ----------------------------------------------------------------
    // --------------------- ส่วนของปุ่ม Confirm ----------------------
    // ----------------------------------------------------------------

    // (1) ปุ่มยืนยัน Save
    public void OnClick_ConfirmSave()
    {
        if (soundManager != null)
        {
            soundManager.PlayButtonSound();
        }

        int index = saveDropdown.value;
        if (index == 0)
        {
            Debug.Log("User chooses to create a New Save");
            saveLoadManager.Save();
        }
        else
        {
            int dataIndex = index - 1;
            if (dataIndex >= 0 && dataIndex < allUserSaves.Count)
            {
                var chosen = allUserSaves[dataIndex];
                Debug.Log($"User chooses to overwrite: {chosen.simulate_name} (ID={chosen.simulate_id})");

                // ตัวอย่างนี้ยังคงเรียก Save() -> Insert ใหม่
                // ถ้าต้องการ Update จริง ๆ ต้องทำ Endpoint update
                saveLoadManager.Save();
            }
        }
    }

    // (2) ปุ่มยืนยัน Load
    public void OnClick_ConfirmLoad()
    {
        if (soundManager != null)
        {
            soundManager.PlayButtonSound();
        }

        int index = loadDropdown.value;
        if (index == 0)
        {
            Debug.Log("No save chosen to load.");
            return;
        }

        int dataIndex = index - 1;
        if (dataIndex >= 0 && dataIndex < allUserSaves.Count)
        {
            var chosen = allUserSaves[dataIndex];
            Debug.Log($"User chooses to Load: {chosen.simulate_name} (ID={chosen.simulate_id})");

            saveLoadManager.LoadById(chosen.simulate_id);
        }
    }

    // (3) ปุ่มยืนยัน Delete
    public void OnClick_ConfirmDelete()
    {
        if (soundManager != null)
        {
            soundManager.PlayButtonSound();
        }

        int index = deleteDropdown.value;
        if (index == 0)
        {
            Debug.Log("No save chosen to delete.");
            return;
        }

        int dataIndex = index - 1;
        if (dataIndex >= 0 && dataIndex < allUserSaves.Count)
        {
            var chosen = allUserSaves[dataIndex];
            Debug.Log($"User chooses to DELETE: {chosen.simulate_name} (ID={chosen.simulate_id})");

            // เรียก coroutine ลบ
            StartCoroutine(DeleteSaveFromServer(chosen.simulate_id));
        }
    }

    /// <summary>
    /// Coroutine สำหรับเรียก API ลบเซฟตาม ID
    /// </summary>
    private IEnumerator DeleteSaveFromServer(long saveId)
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("No userId => cannot delete save from server.");
            yield break;
        }

        // เรียก /api/simulator/deleteById?userId=xxx&saveId=yyy
        string url = $"http://localhost:5000/api/simulator/deleteById?userId={userId}&saveId={saveId}";

        using (UnityWebRequest req = UnityWebRequest.Delete(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Delete save failed: " + req.error);
            }
            else
            {
                Debug.Log("Delete success => " + req.downloadHandler.text);
                // หลังจากลบเสร็จ ให้รีเฟรชรายการเซฟ
                // จะไม่รักษา selection (เพราะอันที่ลบอาจหายไปแล้ว)
                StartCoroutine(GetAllSavesFromServer(keepSelection: false));
            }
        }
    }
}
