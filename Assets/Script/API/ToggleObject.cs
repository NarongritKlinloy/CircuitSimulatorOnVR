using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class ToggleObjects : MonoBehaviour
{
    private string userId;

    [Header("URL ของ API (ไม่ต้องต่อท้าย UID)")]
    public string apiUrl = "https://smith11.ce.kmitl.ac.th/api/practice/find/";

    [Header("ช่วงเวลาที่จะเช็คซ้ำ (วินาที)")]
    public float pollingInterval = 5f;

    // ----- กลุ่ม Object ที่ต้องสลับการเปิด/ปิด พร้อมกำหนด ID -----
    [System.Serializable]
    public class ToggleItem
    {
        public GameObject targetObject;       // Object ที่จะเปิด/ปิด
        public int practiceId;                // ID ที่ใช้จับคู่กับข้อมูลที่ได้รับจาก API
        public TextMeshProUGUI nameText;      // สำหรับแสดงชื่อ practice
        public TextMeshProUGUI detailText;    // สำหรับแสดงรายละเอียด practice
    }

    [Header("รายการ Object ที่ต้องการ Toggle")]
    public ToggleItem[] toggleItems;          // ตั้งค่าใน Inspector

    // โครงสร้างข้อมูลที่รับมาจาก API
    [System.Serializable]
    public class PracticeFindData
    {
        public int practice_id;
        public string practice_name;
        public string practice_detail;
        public int practice_status;
    }

    void Start()
    {   
        // userId = "65015041@kmitl.ac.th";
        // StartCoroutine(CheckAllPracticesLoop(userId));
    }

    public void UpdateUserId(string newUserId)
    {
        userId = newUserId;
        // เก็บลง PlayerPrefs เผื่อใช้ข้ามซีน หรือเปิด-ปิดโปรแกรม
        PlayerPrefs.SetString("userId", userId);
        PlayerPrefs.Save();
        // เริ่ม Coroutine ตรวจสอบ practice ทุก pollingInterval วินาที
        StartCoroutine(CheckAllPracticesLoop(userId));
        Debug.Log("✅ Updated userId in ToggleObject: " + userId);
    }

    public void ClearUserId()
    {
        // หยุดการทำงานของ Coroutine ที่กำลังเช็คสถานะ
        StopAllCoroutines();

        // ลบคีย์ userId ใน PlayerPrefs
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.Save();

        // เคลียร์ตัวแปรภายใน
        userId = null;

        // ปิดวัตถุทุกตัวเป็น default
        if (toggleItems != null)
        {
            foreach (var item in toggleItems)
            {
                if (item.targetObject != null)
                    item.targetObject.SetActive(false);

                if (item.nameText != null)
                    item.nameText.text = "";

                if (item.detailText != null)
                    item.detailText.text = "";
            }
        }

        Debug.Log("🔸 ToggleObjects: Cleared userId and stopped checking practice.");
    }

    IEnumerator CheckAllPracticesLoop(string userId)
    {
        while (true)
        {
            yield return StartCoroutine(GetAllPracticeData(userId));
            yield return new WaitForSeconds(pollingInterval);
        }
    }

    IEnumerator GetAllPracticeData(string userId)
    {
        // ใช้ EscapeURL เพื่อ encode userId ที่อาจมีอักขระพิเศษ เช่น @
        string finalUrl = apiUrl + UnityWebRequest.EscapeURL(userId);
        using (UnityWebRequest www = UnityWebRequest.Get(finalUrl))
        {
            yield return www.SendWebRequest();

            // ก่อนตรวจสอบข้อมูล ให้ปิด Object ทุกตัวเป็นค่า default
            foreach (var item in toggleItems)
            {
                item.targetObject.SetActive(false);
                if (item.nameText != null) item.nameText.text = "";
                if (item.detailText != null) item.detailText.text = "";
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                // Parse JSON Array ด้วย JsonHelper
                PracticeFindData[] dataArray = JsonHelper.FromJson<PracticeFindData>(json);

                // ถ้าไม่มีข้อมูลหรือ array ว่าง ก็จะคง Object ปิดอยู่
                if (dataArray != null && dataArray.Length > 0)
                {
                    foreach (var pd in dataArray)
                    {
                        foreach (var item in toggleItems)
                        {
                            if (item.practiceId == pd.practice_id)
                            {
                                // เปิด Object ถ้า practice_status เป็น 1
                                bool isOpen = (pd.practice_status == 1);
                                item.targetObject.SetActive(isOpen);

                                // อัปเดตชื่อและรายละเอียดใน UI
                                if (item.nameText != null)
                                {
                                    item.nameText.text = pd.practice_name;
                                }
                                if (item.detailText != null)
                                {
                                    item.detailText.text = pd.practice_detail;
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("No practice data found. All objects set to inactive.");
                }
            }
            else
            {
                Debug.LogError($"Error calling {finalUrl}: {www.error}");
            }
        }
    }
}

// ---------------------------------------------------
// Helper สำหรับ parse JSON array ด้วย Unity JsonUtility
// Unity ไม่สามารถ parse JSON Array ได้ตรง ๆ ดังนั้นต้องใช้ Wrapper
// ---------------------------------------------------
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.Items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
