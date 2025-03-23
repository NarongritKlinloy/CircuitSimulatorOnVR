using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;  // จำเป็นสำหรับ UnityWebRequest
using System;


// ในที่นี้เราจะใช้คลาส Switch แทน toggle switch interface
// โดย Switch.cs มีฟังก์ชัน Toggle() และ property IsClosed
public class CircuitTesterWithScore : MonoBehaviour
{
    // อ้างอิงไปที่ CircuitLab ที่มีใน Scene (กำหนดใน Inspector)
    public CircuitLab circuitLab;

    // ใช้เก็บรายการอุปกรณ์ที่ถูกวางลงบน Breadboard
    private List<PlacedComponent> placedComponents;

    // ตัวแปรสำหรับคะแนน สามารถกำหนดคะแนนเต็มได้จาก Inspector
    [Header("คะแนน")]
    [Tooltip("คะแนนเต็มที่ต้องการ (เช่น 60 คะแนน หรือ 100 คะแนน)")]
    public int maxScore = 60;
    private int score = 0;

    // คะแนนฐานสำหรับแต่ละประเภท (รวมกัน = 60 คะแนน)
    private int baseBatteryScore = 10;
    private int baseFluteScore = 10;
    private int baseSwitchScore = 10;
    private int baseWireScore = 10;
    private int baseToggleTestScore = 20;

    // เก็บข้อความข้อผิดพลาดของการตรวจสอบวงจร
    public string ErrorMessage { get; private set; } = "";
    // Flag สำหรับบอกว่าการตรวจสอบวงจรเสร็จสมบูรณ์แล้ว
    public bool IsCheckComplete { get; private set; } = false;

    // Property ให้สามารถอ่านคะแนนจากภายนอกได้
    public int Score
    {
        get { return score; }
    }

    // สำหรับเก็บคะแนนที่โหลดมาจากฐานข้อมูล
    public int LoadedScore { get; private set; } = 0;

    void Start()
    {
        StartCoroutine(LoadScoreFromServer());
    }

    // ฟังก์ชันนี้ให้เรียกจาก UI (เช่นปุ่ม) เพื่อเริ่มตรวจสอบวงจรและคำนวณคะแนน
    public void CheckScore()
    {
        // เคลียร์ข้อความข้อผิดพลาดและ flag
        ErrorMessage = "";
        IsCheckComplete = false;
        // เรียกใช้ Coroutine เพื่อตรวจสอบวงจร
        StartCoroutine(RunCheckScore());
    }

    // Coroutine หลักในการตรวจสอบวงจรและให้คะแนน
    private IEnumerator RunCheckScore()
    {
        // รีเซ็ตคะแนนทุกครั้งก่อนตรวจสอบใหม่
        score = 0;

        // คำนวณ scale factor เพื่อปรับคะแนนจากฐานเต็ม 60 ไปเป็น maxScore ที่กำหนด
        float scale = (float)maxScore / 60f;

        // ดึงรายการอุปกรณ์จาก CircuitLab
        placedComponents = circuitLab.GetPlacedComponents();

        // ตัวแปรนับจำนวนอุปกรณ์ตามประเภทที่ต้องการ
        int countWire = 0;
        int countSwitch = 0; // ใช้สำหรับนับ Switch
        int countBattery = 0;
        int countFlute = 0; // ใช้นับ Flute (ตัวแทนของตัวต้านทาน)

        // ตรวจสอบประเภทของแต่ละอุปกรณ์ในวงจร
        foreach (PlacedComponent comp in placedComponents)
        {
            var circuitComponent = comp.Component;
            if (circuitComponent is IBattery)
                countBattery++;
            if (circuitComponent is Flute)
                countFlute++;
            if (circuitComponent is Switch)
                countSwitch++;
            if ((circuitComponent is IConductor) && !(circuitComponent is Switch))
                countWire++;
        }

        Debug.Log("Circuit Test Summary:");
        Debug.Log("Battery count: " + countBattery);
        Debug.Log("Flute (Resistor) count: " + countFlute);
        Debug.Log("Switch count: " + countSwitch);
        Debug.Log("Wire count: " + countWire);

        // คำนวณคะแนนแต่ละส่วนโดยใช้ Floor
        int batteryScore = Mathf.FloorToInt(baseBatteryScore * scale);
        int fluteScore = Mathf.FloorToInt(baseFluteScore * scale);
        int switchScore = Mathf.FloorToInt(baseSwitchScore * scale);
        int wireScore = Mathf.FloorToInt(baseWireScore * scale);
        int toggleTestScore = Mathf.FloorToInt(baseToggleTestScore * scale);

        // รวมคะแนนที่ได้จากส่วนต่าง ๆ
        int partialTotal = batteryScore + fluteScore + switchScore + wireScore + toggleTestScore;
        // คำนวณเศษที่เหลือ (remainder) ที่ต้องเพิ่มเข้าไปให้ผลรวมเท่ากับ maxScore
        int remainder = maxScore - partialTotal;
        // เพิ่ม remainder ให้กับส่วน Toggle Test (หรือแจกจ่ายไปตามต้องการ)
        toggleTestScore += remainder;

        // ตรวจสอบเงื่อนไขพื้นฐาน
        bool pass = true;
        if (countBattery < 1)
        {
            string err = "Test Failed: ต้องมี Battery อย่างน้อย 1 ชิ้น\n";
            Debug.LogError(err);
            ErrorMessage += err;
            pass = false;
        }
        if (countFlute < 1)
        {
            string err = "Test Failed: ต้องมี Flute (ตัวต้านทาน) อย่างน้อย 1 ชิ้น\n";
            Debug.LogError(err);
            ErrorMessage += err;
            pass = false;
        }
        if (countSwitch < 1)
        {
            string err = "Test Failed: ต้องมี Switch 1 ชิ้น\n";
            Debug.LogError(err);
            ErrorMessage += err;
            pass = false;
        }
        if (countWire < 1)
        {
            string err = "Test Failed: ต้องมี Wire อย่างน้อย 1 ชิ้น\n";
            Debug.LogError(err);
            ErrorMessage += err;
            pass = false;
        }

        // ถ้าผ่านเงื่อนไขพื้นฐานแล้ว ให้รวมคะแนนพื้นฐาน
        if (pass)
        {
            score += batteryScore + fluteScore + switchScore + wireScore;
            Debug.Log("ผ่าน: คะแนนพื้นฐานรวม = " + (batteryScore + fluteScore + switchScore + wireScore));
        }

        // เรียกให้ CircuitLab จำลองวงจรก่อนตรวจสอบ active circuit
        circuitLab.SimulateCircuit();

        // ตรวจสอบว่า Battery, Flute, Switch อยู่ใน active circuit เดียวกันหรือไม่
        bool batteryActive = false;
        bool fluteActive = false;
        bool switchActive = false;
        int currentGen = circuitLab.board.Generation; // ใช้ Generation ของวงจรล่าสุด
        foreach (PlacedComponent pc in circuitLab.board.Components)
        {
            if (pc.Generation == currentGen)
            {
                if (pc.Component is IBattery)
                    batteryActive = true;
                if (pc.Component is Flute)
                    fluteActive = true;
                if (pc.Component is Switch)
                    switchActive = true;
            }
        }
        if (!(batteryActive && fluteActive && switchActive))
        {
            string err = "Test Failed: อุปกรณ์บางชิ้น (Battery/Flute/Switch) ไม่ได้อยู่ใน active circuit ร่วมกัน (ขั้ว+ → ขั้ว-)\n";
            Debug.LogError(err);
            ErrorMessage += err;
            pass = false;
        }
        else
        {
            Debug.Log("ผ่าน: Battery, Flute, และ Switch อยู่ในวงจรเดียวกัน (ขั้ว+ → ขั้ว-)");
        }

        if (pass)
        {
            Debug.Log("Circuit composition ผ่านเงื่อนไขพื้นฐานแล้ว");
        }
        else
        {
            Debug.LogError("Circuit composition ไม่ผ่านเงื่อนไขที่กำหนด");
        }

        // ถ้าวงจรผ่านพื้นฐานแล้ว ให้ทดสอบการทำงานของ Switch
        if (pass)
        {
            Switch mySwitch = FindSwitch();
            if (mySwitch != null)
            {
                if (mySwitch.IsClosed)
                {
                    mySwitch.Toggle(); // ถ้าอยู่ปิด ให้ toggle เพื่อเปิดมัน
                    yield return new WaitForSeconds(1f);
                }
                if (!mySwitch.IsClosed)
                {
                    mySwitch.Toggle(); // เปลี่ยนเป็นปิด
                    Debug.Log("Switch: ปิด (closed) - จำลองวงจร");
                    yield return new WaitForSeconds(2f);
                }
                if (mySwitch.IsClosed)
                {
                    mySwitch.Toggle(); // เปลี่ยนเป็นเปิด
                    Debug.Log("Switch: เปิด (open) - จำลองวงจร");
                    yield return new WaitForSeconds(2f);
                }
                if (!mySwitch.IsClosed)
                {
                    mySwitch.Toggle(); // เปลี่ยนเป็นปิด
                    Debug.Log("Switch: ปิด (closed) อีกครั้ง - จำลองวงจร");
                    yield return new WaitForSeconds(2f);
                }
                // เพิ่มคะแนนสำหรับการทดสอบ Switch (ส่วนนี้เราใช้คะแนนที่คำนวณไว้พร้อมกับ remainder)
                score += toggleTestScore;
                Debug.Log("ผ่าน: การสลับสถานะ Switch (+" + toggleTestScore + ")");
            }
            else
            {
                string err = "Test Failed: ไม่พบ Switch ในวงจร\n";
                Debug.LogError(err);
                ErrorMessage += err;
            }
        }

        Debug.Log("คะแนนรวม: " + score + " / " + maxScore);

        // ตั้งค่าสถานะว่าการตรวจสอบเสร็จแล้ว
        IsCheckComplete = true;
        yield break;
    }

    // ฟังก์ชันค้นหา Switch จากรายการอุปกรณ์
    private Switch FindSwitch()
    {
        if (placedComponents == null) return null;
        foreach (PlacedComponent comp in placedComponents)
        {
            if (comp.Component is Switch)
            {
                return comp.Component as Switch;
            }
        }
        return null;
    }

    // -----------------------------
    // ฟังก์ชัน SubmitScore: ส่งคะแนนไปเก็บใน Database ผ่าน API โดยใช้ practice_id เป็น 3
    // -----------------------------
    public void SubmitScore()
    {
        StartCoroutine(SubmitScoreToServer(score));
    }

    private IEnumerator SubmitScoreToServer(int score)
    {
        string userId = PlayerPrefs.GetString("userId", "");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("ไม่พบ userId ใน PlayerPrefs");
            yield break;
        }

        int practiceId = 3; // ใช้ practice_id เป็น 3

        ScoreRequestData requestData = new ScoreRequestData();
        requestData.userId = userId;
        requestData.practiceId = practiceId;
        requestData.quizData = new QuizData();
        requestData.quizData.score = score;

        string jsonBody = JsonUtility.ToJson(requestData);
        string url = "https://smith11.ce.kmitl.ac.th/api/saveScore";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityWebRequest.Result.Success)
#else
            if (!request.isNetworkError && !request.isHttpError)
#endif
            {
                Debug.Log("Score saved successfully! Response: " + request.downloadHandler.text);
                StartCoroutine(SendLogToServer(userId, 1, practiceId));

            }
            else
            {
                Debug.LogError("Error saving score: " + request.error);
            }
        }
    }
public IEnumerator SendLogToServer(string userId, int logType, int practiceId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("❌ SendLogToServer() called with EMPTY userId!");
            yield break;
        }

        string logUrl = "https://smith11.ce.kmitl.ac.th/api/log/visitunity";

        // ✅ เปลี่ยนจาก Anonymous Object -> Explicit Class เพื่อให้ JsonUtility ใช้งานได้
        LogData logData = new LogData
        {
            uid = userId,
            log_type = logType,
            practice_id = practiceId
        };

        string jsonPayload = JsonUtility.ToJson(logData);
        Debug.Log($"📌 Sending log data: {jsonPayload} (userId: {userId})"); // ✅ Debug JSON Payload

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(logUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            Debug.Log($"📌 Response Code: {request.responseCode}");
            Debug.Log($"📌 Response Text: {request.downloadHandler.text}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Failed to send log data: {request.error}");
            }
            else
            {
                Debug.Log($"✅ Log data sent successfully: {request.downloadHandler.text}");
            }
        }
    }

    // ✅ เพิ่มคลาสนี้เพื่อให้ JsonUtility ใช้งานได้
    [Serializable]
    public class LogData
    {
        public string uid;
        public int log_type;
        public int practice_id;
    }
    // -----------------------------
    // ฟังก์ชัน LoadScore: โหลดคะแนนจาก Database ผ่าน API โดยใช้ practice_id เป็น 3
    // -----------------------------
    public void LoadScore()
    {
        StartCoroutine(LoadScoreFromServer());
    }

    private IEnumerator LoadScoreFromServer()
    {
        int practiceId = 3;
        string url = "https://smith11.ce.kmitl.ac.th/api/practice/" + practiceId;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityWebRequest.Result.Success)
#else
            if (!request.isNetworkError && !request.isHttpError)
#endif
            {
                Debug.Log("Load Score Success: " + request.downloadHandler.text);
                // แก้ไข JSON mapping โดยใช้ field "practice_score" แทน "score"
                ScoreResponseData responseData = JsonUtility.FromJson<ScoreResponseData>(request.downloadHandler.text);
                // เปลี่ยนค่า maxScore และ score ให้ตรงกับคะแนนที่โหลดมาจากฐานข้อมูล
                maxScore = responseData.practice_score;
                score = responseData.practice_score;
                LoadedScore = responseData.practice_score;
                Debug.Log("Loaded Score: " + LoadedScore);
            }
            else
            {
                Debug.LogError("Error loading score: " + request.error);
            }
        }
    }

    // -----------------------------
    // คลาสสำหรับส่งข้อมูลคะแนน
    // -----------------------------
    [System.Serializable]
    public class ScoreRequestData
    {
        public string userId;
        public int practiceId;
        public QuizData quizData;
    }

    [System.Serializable]
    public class QuizData
    {
        public int score;
    }

    // -----------------------------
    // คลาสสำหรับรับข้อมูลคะแนนที่โหลดมา
    // ใช้ field "practice_score" เพื่อให้ตรงกับข้อมูลใน database
    // -----------------------------
    [System.Serializable]
    public class ScoreResponseData
    {
        public int practice_score;
    }
}
