using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class PracticeDataExam2 : MonoBehaviour
{
    [Header("อ้างอิง QuizManager2 (สำหรับส่งคะแนน)")]
    public QuizManager2 quizManager; // กำหนดใน Inspector

    [Header("อ้างอิง SaveLoadManager2 (สำหรับส่ง practice JSON)")]
    public SaveLoadManager2 saveLoadPactics; // กำหนดใน Inspector

    [Header("URL ของ Server API สำหรับส่งข้อมูล (เช่น http://localhost:5000/api/save)")]
    [SerializeField] private string serverUrl = "http://localhost:5000/api/save";

    // ฟังก์ชันเพื่อส่งข้อมูลทั้งหมด (คะแนนและ practice JSON ) ไปยัง server
    public void SaveAllData()
    {
        StartCoroutine(SaveAllDataCoroutine());
    }

    private IEnumerator SaveAllDataCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        // ตรวจสอบว่า quizManager ได้ถูก assign แล้ว
        if (quizManager == null)
        {
            Debug.LogError("QuizManager is not assigned.");
            yield break;
        }

        // คำนวณคะแนน
        quizManager.SubmitScore();
        yield return null;

        // อ่านข้อมูล practice JSON จากไฟล์ (ถ้ามี)
        string practiceFilePath = Application.persistentDataPath + "/saveFileDigital.json";
        string practiceJson = "{}";
        if (File.Exists(practiceFilePath))
        {
            string fileContent = File.ReadAllText(practiceFilePath);
            if (!string.IsNullOrEmpty(fileContent))
                practiceJson = fileContent;
        }
        else
        {
            Debug.LogWarning("ไม่พบไฟล์ practice JSON ที่: " + practiceFilePath);
        }

        // สร้าง payload
        SaveDataPayload payload = new SaveDataPayload();
        payload.quizData = new QuizData();
        payload.quizData.score = quizManager.totalScore;
        payload.practiceData = new PracticeData();
        payload.practiceData.json = practiceJson;
        // ดึง userId จาก PlayerPrefs
        payload.userId = PlayerPrefs.GetString("userId", "unknown");
        payload.practiceId = 2;

        Debug.Log("คะแนนที่ได้: " + payload.quizData.score);
        Debug.Log("UserId from PlayerPrefs: " + payload.userId);

        string jsonPayload = JsonUtility.ToJson(payload);
        Debug.Log("Payload to save: " + jsonPayload);

        // ส่ง payload ไปยัง server
        UnityWebRequest www = new UnityWebRequest(serverUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (www.result != UnityWebRequest.Result.Success)
#else
        if (www.isNetworkError || www.isHttpError)
#endif
        {
            Debug.LogError("ส่งข้อมูลผิดพลาด: " + www.error);
        }
        else
        {
            Debug.Log("ส่งข้อมูลสำเร็จ: " + www.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class SaveDataPayload
    {
        public QuizData quizData;
        public PracticeData practiceData;
        public string userId;
        public int practiceId;
    }

    [System.Serializable]
    public class QuizData
    {
        public int score;
    }

    [System.Serializable]
    public class PracticeData
    {
        public string json;
    }
}
