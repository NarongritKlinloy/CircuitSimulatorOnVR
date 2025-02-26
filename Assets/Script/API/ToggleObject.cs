using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class ToggleObjects : MonoBehaviour
{
    [Header("URL ของ API (ไม่ต้องต่อท้าย UID)")]
    public string apiUrl = "http://localhost:5000/api/practice/find/";

    [Header("ช่วงเวลาที่จะเช็คซ้ำ (วินาที)")]
    public float pollingInterval = 5f;

    // ----- กลุ่ม Object ที่ต้องสลับการเปิด/ปิด พร้อมกำหนด ID -----
    [System.Serializable]
    public class ToggleItem
    {
        public GameObject targetObject;       // Object ที่จะเปิด/ปิด
        public int practiceId;                // ID ที่จะใช้จับคู่กับข้อมูลที่ได้รับจาก API
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
        // ดึง userId จาก PlayerPrefs ที่ถูกเซ็ตไว้จาก GoogleAuthen หรือ WebSocketManager
        string storedUserId = PlayerPrefs.GetString("userId", "unknown");

        if (string.IsNullOrEmpty(storedUserId) || storedUserId == "unknown")
        {
            Debug.LogWarning("No valid userId in PlayerPrefs. ToggleObjects won't fetch data.");
            return;
        }

        // เริ่ม Coroutine ตรวจสอบ practice ทุก pollingInterval วินาที
        StartCoroutine(CheckAllPracticesLoop(storedUserId));
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

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                // Parse JSON Array ด้วย JsonHelper (เพราะ Unity JsonUtility ไม่รองรับ Array โดยตรง)
                PracticeFindData[] dataArray = JsonHelper.FromJson<PracticeFindData>(json);

                if (dataArray != null)
                {
                    // สำหรับแต่ละข้อมูล practice ที่ได้รับจาก API
                    foreach (var pd in dataArray)
                    {
                        // วน loop เช็คกับแต่ละ ToggleItem ที่กำหนดไว้ใน Inspector
                        foreach (var item in toggleItems)
                        {
                            if (item.practiceId == pd.practice_id)
                            {
                                // ถ้า practice_status เป็น 1 ให้เปิด Object, ถ้า 0 ปิด
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
