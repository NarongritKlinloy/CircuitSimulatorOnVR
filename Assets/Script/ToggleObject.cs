using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ToggleObjects : MonoBehaviour
{
    [Header("URL ของ API (ไม่ต้องต่อท้าย ID)")]
    public string apiUrl = "http://localhost:5000/api/practice/";

    [Header("ช่วงเวลาที่จะเช็คซ้ำ (วินาที)")]
    public float pollingInterval = 5f;

    // ----- กลุ่ม Object ที่ต้องสลับการเปิด/ปิด พร้อมกำหนด ID -----
    [System.Serializable]
    public class ToggleItem
    {
        public GameObject targetObject;  // อ้างอิง GameObject ที่จะเปิด/ปิด
        public int practiceId;           // ID ที่จะดึงจากฐานข้อมูล
    }

    [Header("รายการ Object ที่ต้องการ Toggle")]
    public ToggleItem[] toggleItems; // เก็บชุด (Object, PracticeID) หลายตัว

    void Start()
    {
        // เริ่ม Coroutine ที่จะเช็คสถานะทุก ๆ pollingInterval วินาที
        StartCoroutine(CheckAllObjectsLoop());
    }

    // ลูปหลัก: เรียกเช็คสถานะของทุก Item แล้วรอ pollingInterval ก่อนทำใหม่
    IEnumerator CheckAllObjectsLoop()
    {
        while (true)
        {
            // เรียกเช็คสถานะทีละ Item
            foreach (var item in toggleItems)
            {
                yield return StartCoroutine(GetPracticeStatus(item.practiceId, item.targetObject));
            }

            // รอ pollingInterval วินาที
            yield return new WaitForSeconds(pollingInterval);
        }
    }

    // ดึงข้อมูล practice_status จาก API แล้ว SetActive() ให้ Object
    IEnumerator GetPracticeStatus(int practiceId, GameObject targetObj)
    {
        string finalUrl = apiUrl + practiceId;

        using (UnityWebRequest www = UnityWebRequest.Get(finalUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                PracticeStatusResponse response = JsonUtility.FromJson<PracticeStatusResponse>(json);

                bool isOpen = (response.practice_status == 1);
                targetObj.SetActive(isOpen);

                Debug.Log($"Practice {response.practice_id} => Status: {response.practice_status} => Active={isOpen}");
            }
            else
            {
                Debug.LogError($"Error calling {finalUrl}: {www.error}");
            }
        }
    }

    // โครงสร้างข้อมูลที่รับจาก JSON
    [System.Serializable]
    public class PracticeStatusResponse
    {
        public int practice_id;
        public int practice_status;
    }
}
