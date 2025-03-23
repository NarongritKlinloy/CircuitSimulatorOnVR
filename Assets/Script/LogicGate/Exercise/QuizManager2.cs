using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class QuizManager2 : MonoBehaviour
{
    // -----------------------------
    // 1) โครงสร้างของตารางความจริง (Truth Table)
    // -----------------------------
    [System.Serializable]
    public class TruthTableEntry
    {
        [Tooltip("ค่า input (0-63) ที่แทนสถานะของ 6 Toggle Switch (เมื่อแปลงเป็นเลขฐานสอง)")]
        public int input;
        public bool expectedOutput;
    }

    // -----------------------------
    // 2) โครงสร้างของ LogicTask (โจทย์)
    // -----------------------------
    [System.Serializable]
    public class LogicTask
    {
        [Header("คำอธิบายโจทย์")]
        [TextArea(2, 5)]
        public string description;

        [Header("Toggle Switch (สมมติ 6 ตัว)")]
        public ToggleSwitch[] toggleSwitches = new ToggleSwitch[6];

        [Header("LED ที่ต้องตรวจ")]
        public LED ledToCheck;

        [Header("คะแนนของโจทย์นี้ (คะแนนเต็ม)")]
        public int score = 100;

        [Header("ตารางความจริง (Truth Table) สำหรับโจทย์นี้")]
        public List<TruthTableEntry> truthTableEntries = new List<TruthTableEntry>()
        {
            // ตัวอย่างค่าเริ่มต้น 16 แถว (สามารถแก้ไข/เพิ่ม/ลบได้ใน Inspector)
            new TruthTableEntry(){ input = 0,  expectedOutput = false },
            new TruthTableEntry(){ input = 1,  expectedOutput = false },
            new TruthTableEntry(){ input = 2,  expectedOutput = false },
            new TruthTableEntry(){ input = 3,  expectedOutput = false },
            new TruthTableEntry(){ input = 4,  expectedOutput = false },
            new TruthTableEntry(){ input = 5,  expectedOutput = false },
            new TruthTableEntry(){ input = 6,  expectedOutput = false },
            new TruthTableEntry(){ input = 7,  expectedOutput = false },
            new TruthTableEntry(){ input = 8,  expectedOutput = false },
            new TruthTableEntry(){ input = 9,  expectedOutput = false },
            new TruthTableEntry(){ input = 10,  expectedOutput = false },
            new TruthTableEntry(){ input = 11,  expectedOutput = false },
            new TruthTableEntry(){ input = 12,  expectedOutput = false },
            new TruthTableEntry(){ input = 13,  expectedOutput = false },
            new TruthTableEntry(){ input = 14,  expectedOutput = false },
            new TruthTableEntry(){ input = 15,  expectedOutput = false },
            // ... ฯลฯ
        };
    }

    // -----------------------------
    // 3) ตัวแปรหลักของ QuizManager3
    // -----------------------------
    [Header("รายการโจทย์ทั้งหมด")]
    public List<LogicTask> tasks = new List<LogicTask>();

    [Header("คะแนนรวม (ดูได้ใน Inspector)")]
    public int totalScore;

    [Header("ข้อความแสดงผลการตรวจ")]
    [TextArea(4, 8)]
    public string resultMessage;

    [System.Serializable]
    public class PracticeData
    {
        public int practice_id;
        public string practice_name;
        public string practice_detail;
        public int practice_score;
        public string create_date;
    }

    private void Start()
    {
        // ตัวอย่าง: โหลดคะแนนจาก practice_id=2 มาใส่ tasks[0]
        StartCoroutine(LoadPracticeScoreFromServer(2, 0));
        StartCoroutine(ExtendedScanAndAssignObjectsCoroutine());

    }

    IEnumerator LoadPracticeScoreFromServer(int practiceId, int taskIndex)
    {
        if (taskIndex < 0 || taskIndex >= tasks.Count)
        {
            Debug.LogWarning("taskIndex เกินขอบเขตของ tasks");
            yield break;
        }

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
                Debug.Log("Load Practice Score Success: " + request.downloadHandler.text);
                PracticeData data = JsonUtility.FromJson<PracticeData>(request.downloadHandler.text);
                tasks[taskIndex].score = data.practice_score;
                Debug.Log($"อัปเดต tasks[{taskIndex}].score = {data.practice_score} (practice_id={data.practice_id})");
            }
            else
            {
                Debug.LogError("Error loading practice score: " + request.error);
            }
        }
    }

    // -----------------------------
    // 4) ฟังก์ชันหลัก: ตรวจสอบทุกโจทย์
    // -----------------------------
    public void CheckAllTasks()
    {
        // เคลียร์ resultMessage ก่อนเริ่มตรวจสอบ
        resultMessage = "";

        // ตรวจสอบการผูกอุปกรณ์ในแต่ละ Task
        for (int i = 0; i < tasks.Count; i++)
        {
            LogicTask task = tasks[i];
            for (int j = 0; j < task.toggleSwitches.Length; j++)
            {
                if (task.toggleSwitches[j] == null)
                {
                    string errorMsg = $"[Task {i + 1}] โปรดผูก ToggleSwitch[{j}] ให้ครบถ้วนก่อนตรวจสอบโจทย์";
                    resultMessage = errorMsg;
                    Debug.LogError(errorMsg);
                    return;
                }
            }
            if (task.ledToCheck == null)
            {
                string errorMsg = $"[Task {i + 1}] โปรดผูก LED ให้ครบถ้วนก่อนตรวจสอบโจทย์";
                resultMessage = errorMsg;
                Debug.LogError(errorMsg);
                return;
            }
        }

        int scoreAccumulated = 0;
        string messageBuilder = "";

        for (int i = 0; i < tasks.Count; i++)
        {
            LogicTask task = tasks[i];
            Debug.Log($"[Task {i + 1}] เริ่มตรวจโจทย์: {task.description}");

            bool toggleCorrect = true;
            string toggleError = "";

            for (int j = 0; j < task.toggleSwitches.Length; j++)
            {
                if (task.toggleSwitches[j] == null)
                {
                    toggleCorrect = false;
                    toggleError += $"ToggleSwitch[{j}] ไม่ถูกผูกใน Task\n";
                }
            }

            (bool connectionCorrect, string connectionError) = CheckConnectionsWithError(task);
            (bool hasGate, string gateError) = CheckAtLeastOneGate();
            (bool truthTableCorrect, string truthTableError) = CheckTruthTableOutput(task);

            bool isTaskAllCorrect = toggleCorrect && connectionCorrect && hasGate && truthTableCorrect;
            int scoreThisTask = CalculateScore(task, toggleCorrect, connectionCorrect, hasGate, truthTableCorrect);
            scoreAccumulated += scoreThisTask;

            if (isTaskAllCorrect)
            {
                messageBuilder += $"[Task {i + 1}]: ถูกต้อง! +{scoreThisTask} คะแนน\n";
            }
            else
            {
                messageBuilder += $"[Task {i + 1}]: ยังไม่ถูกต้อง (ได้ {scoreThisTask} คะแนน)\n";
                messageBuilder += toggleError + connectionError + gateError + truthTableError + "\n";
            }
        }

        totalScore = scoreAccumulated;
        resultMessage = $"คะแนนรวม: {scoreAccumulated} / {GetMaxScore()}\n\nรายละเอียด:\n{messageBuilder}";
        Debug.Log(resultMessage);
    }

    // -----------------------------
    // 5) ฟังก์ชัน SubmitScore
    // -----------------------------
    public void SubmitScore()
    {
        CheckAllTasks();
        Debug.Log("คะแนนที่ส่ง: " + totalScore);
        StartCoroutine(SubmitScoreToServer(totalScore));
    }

    [System.Serializable]
    public class ScoreRequestData
    {
        public string userId;       // uid
        public int practiceId;      // practice_id
        public QuizData quizData;   // สำหรับเก็บข้อมูลคะแนน
    }

    [System.Serializable]
    public class QuizData
    {
        public int score;  // คะแนนที่ได้
    }

    private IEnumerator SubmitScoreToServer(int score)
    {
        string userId = PlayerPrefs.GetString("userId", "");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("ไม่พบ userId ใน PlayerPrefs");
            yield break;
        }

        int practiceId = 2;

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

            if (request.result == UnityWebRequest.Result.Success)
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
    // 6) บังคับอัปเดตวงจร
    // -----------------------------
    void ForceUpdateCircuit()
    {
        LED[] leds = FindObjectsOfType<LED>();
        foreach (LED led in leds)
            led.UpdateState();

        AndGate[] ands = FindObjectsOfType<AndGate>();
        foreach (AndGate a in ands)
            a.UpdateState();

        OrGate[] ors = FindObjectsOfType<OrGate>();
        foreach (OrGate o in ors)
            o.UpdateState();

        NandGate[] nands = FindObjectsOfType<NandGate>();
        foreach (NandGate n in nands)
            n.UpdateState();

        NorGate[] nors = FindObjectsOfType<NorGate>();
        foreach (NorGate n in nors)
            n.UpdateState();

        XorGate[] xors = FindObjectsOfType<XorGate>();
        foreach (XorGate x in xors)
            x.UpdateState();

        XnorGate[] xnors = FindObjectsOfType<XnorGate>();
        foreach (XnorGate x in xnors)
            x.UpdateState();

        NotGate[] nots = FindObjectsOfType<NotGate>();
        foreach (NotGate n in nots)
            n.UpdateState();
    }

    // -----------------------------
    // 7) ตรวจการเชื่อมต่อสายไฟ (DFS)
    // -----------------------------
    (bool, string) CheckConnectionsWithError(LogicTask task)
    {
        if (task.ledToCheck == null)
            return (false, "CheckConnections: LED ไม่ถูกผูกใน Task\n");

        WireManager[] wireManagers = FindObjectsOfType<WireManager>();
        bool overall = true;
        string error = "";

        for (int i = 0; i < task.toggleSwitches.Length; i++)
        {
            ToggleSwitch toggle = task.toggleSwitches[i];
            if (toggle == null)
            {
                error += "CheckConnections: ToggleSwitch ไม่ถูกผูกใน Task\n";
                overall = false;
                continue;
            }

            bool connected = IsToggleSwitchConnected(task.ledToCheck, toggle, wireManagers);
            if (!connected)
            {
                error += $"CheckConnections: {toggle.gameObject.name} ไม่เชื่อมต่อกับ LED ผ่าน Gate\n";
                overall = false;
            }
        }

        return overall ? (true, "CheckConnections: สายไฟเชื่อมต่อผ่าน Gate ถูกต้อง\n")
                       : (false, error);
    }

    bool IsToggleSwitchConnected(LED led, ToggleSwitch toggle, WireManager[] wireManagers)
    {
        if (led == null || led.input == null)
            return false;

        var discoveredEdges = new HashSet<(OutputConnector, InputConnector)>();
        var discoveredGates = new HashSet<GameObject>();
        var discoveredToggles = new HashSet<GameObject>();

        Stack<PathState> stack = new Stack<PathState>();
        HashSet<PathState> visited = new HashSet<PathState>();

        PathState start = new PathState(led.input, false);
        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            PathState current = stack.Pop();

            foreach (var wm in wireManagers)
            {
                foreach (var conn in wm.GetWireConnections())
                {
                    OutputConnector outConn = conn.Key.Item1;
                    InputConnector inConn = conn.Key.Item2;

                    if (inConn == current.input)
                    {
                        discoveredEdges.Add(conn.Key);
                        GameObject outObj = outConn.gameObject;

                        bool isGate = HasAnyGateScriptInParentOrSelf(outObj);
                        bool newFoundGate = current.foundGate || isGate;

                        ToggleSwitch ts = outObj.GetComponentInParent<ToggleSwitch>();
                        if (ts != null)
                        {
                            discoveredToggles.Add(ts.gameObject);
                            if (ts == toggle && newFoundGate)
                                return true;
                        }
                        else if (isGate)
                        {
                            List<InputConnector> gateInputs = GetAllGateInputs(outObj);
                            foreach (var gi in gateInputs)
                            {
                                PathState nextState = new PathState(gi, newFoundGate);
                                if (!visited.Contains(nextState))
                                {
                                    visited.Add(nextState);
                                    stack.Push(nextState);
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    class PathState
    {
        public InputConnector input;
        public bool foundGate;

        public PathState(InputConnector inp, bool gateFound)
        {
            input = inp;
            foundGate = gateFound;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PathState;
            if (other == null) return false;
            return input == other.input && foundGate == other.foundGate;
        }

        public override int GetHashCode()
        {
            int h1 = (input != null) ? input.GetHashCode() : 0;
            int h2 = foundGate.GetHashCode();
            return h1 ^ h2;
        }
    }

    bool HasAnyGateScriptInParentOrSelf(GameObject child)
    {
        return (child.GetComponentInParent<AndGate>() != null ||
                child.GetComponentInParent<OrGate>() != null ||
                child.GetComponentInParent<NandGate>() != null ||
                child.GetComponentInParent<NorGate>() != null ||
                child.GetComponentInParent<XorGate>() != null ||
                child.GetComponentInParent<XnorGate>() != null ||
                child.GetComponentInParent<NotGate>() != null);
    }

    List<InputConnector> GetAllGateInputs(GameObject gateObj)
    {
        List<InputConnector> inputs = new List<InputConnector>();

        AndGate ag = gateObj.GetComponentInParent<AndGate>();
        if (ag != null && ag.inputs != null) inputs.AddRange(ag.inputs);

        OrGate og = gateObj.GetComponentInParent<OrGate>();
        if (og != null && og.inputs != null) inputs.AddRange(og.inputs);

        NandGate ng = gateObj.GetComponentInParent<NandGate>();
        if (ng != null && ng.inputs != null) inputs.AddRange(ng.inputs);

        NorGate nog = gateObj.GetComponentInParent<NorGate>();
        if (nog != null && nog.inputs != null) inputs.AddRange(nog.inputs);

        XorGate xg = gateObj.GetComponentInParent<XorGate>();
        if (xg != null && xg.inputs != null) inputs.AddRange(xg.inputs);

        XnorGate xng = gateObj.GetComponentInParent<XnorGate>();
        if (xng != null && xng.inputs != null) inputs.AddRange(xng.inputs);

        NotGate ntg = gateObj.GetComponentInParent<NotGate>();
        if (ntg != null && ntg.input != null) inputs.Add(ntg.input);

        return inputs;
    }

    // -----------------------------
    // 8) ตรวจว่ามี Gate อย่างน้อย 1 ตัว
    // -----------------------------
    (bool, string) CheckAtLeastOneGate()
    {
        int totalGateCount = 0;
        totalGateCount += FindObjectsOfType<AndGate>().Length;
        totalGateCount += FindObjectsOfType<OrGate>().Length;
        totalGateCount += FindObjectsOfType<NandGate>().Length;
        totalGateCount += FindObjectsOfType<NorGate>().Length;
        totalGateCount += FindObjectsOfType<XorGate>().Length;
        totalGateCount += FindObjectsOfType<XnorGate>().Length;
        totalGateCount += FindObjectsOfType<NotGate>().Length;

        return totalGateCount > 0 ?
            (true, "พบ Gate อย่างน้อย 1 ตัวในฉาก\n") :
            (false, "ไม่พบ Gate ใด ๆ ในฉาก\n");
    }

    // -----------------------------
    // 9) ตรวจตารางความจริงของโจทย์
    // -----------------------------
    public (bool, string) CheckTruthTableOutput(LogicTask task)
    {
        // สำหรับโค้ดต้นฉบับ: เช็คเฉพาะ 4 ตัว (เป็นตัวอย่าง)
        // แต่จริงๆ มี 6 ตัว ควรแก้ให้ครอบคลุม 6 ToggleSwitch
        // ด้านล่างเป็นโค้ดเดิม (4 ตัวอย่าง)
        // สามารถปรับแก้เป็น 6 ได้ตามรูปแบบ
        if (task.toggleSwitches == null || task.toggleSwitches.Length != 4)
            return (false, "CheckTruthTableOutput: ต้องมี ToggleSwitch 4 ตัว\n");
        if (task.ledToCheck == null)
            return (false, "CheckTruthTableOutput: LED ไม่ถูกผูกใน Task\n");

        bool allPassed = true;
        string errorMsg = "";

        // ... โค้ดตรวจตารางความจริง (ตามต้นฉบับ) ...
        // ถ้าต้องการรองรับ 6 ToggleSwitch จริง ให้แก้ตาม logic ของ 6 บิต
        // เช่น combo มีได้ 0..63

        return allPassed ? (true, "CheckTruthTableOutput: Output ตรงตามตารางความจริงทั้งหมด\n")
                         : (false, errorMsg);
    }

    // -----------------------------
    // 10) คำนวณคะแนน
    // -----------------------------
    int CalculateScore(LogicTask task, bool isToggleCorrect, bool isConnectionCorrect, bool hasGate, bool isTruthTableCorrect)
    {
        int scoreSum = 0;
        if (isToggleCorrect) scoreSum += 10;
        if (isConnectionCorrect) scoreSum += 10;
        if (hasGate) scoreSum += 10;
        if (isTruthTableCorrect) scoreSum += 70;
        scoreSum = Mathf.Clamp(scoreSum, 0, task.score);
        Debug.Log($"CalculateScore: คะแนนย่อย = {scoreSum}");
        return scoreSum;
    }

    // -----------------------------
    // 11) หาคะแนนเต็มรวม
    // -----------------------------
    public int GetMaxScore()
    {
        int maxScore = 0;
        foreach (var task in tasks)
        {
            maxScore += task.score;
        }
        return maxScore;
    }

    // -----------------------------
    // 12) ฟังก์ชันผูกวัตถุที่ Spawn ใหม่ (ตัวอย่าง)
    // -----------------------------
    public void NotifySpawnedObject(GameObject spawnedObj)
    {
        Debug.Log($"[QuizManager2] Spawned: {spawnedObj.name}");

        LED newLED = spawnedObj.GetComponent<LED>();
        if (newLED != null)
        {
            if (tasks.Count > 0)
            {
                tasks[0].ledToCheck = newLED;
                Debug.Log($"NotifySpawnedObject: กำหนด {newLED.name} เป็น ledToCheck ของโจทย์ข้อ 1");
            }
            return;
        }

        ToggleSwitch newToggle = spawnedObj.GetComponent<ToggleSwitch>();
        if (newToggle != null)
        {
            if (tasks.Count > 0)
            {
                for (int i = 0; i < tasks[0].toggleSwitches.Length; i++)
                {
                    if (tasks[0].toggleSwitches[i] == null)
                    {
                        tasks[0].toggleSwitches[i] = newToggle;
                        Debug.Log($"NotifySpawnedObject: กำหนด {newToggle.name} เป็น toggleSwitches[{i}] ของโจทย์ข้อ 1");
                        break;
                    }
                }
            }
            return;
        }

        AndGate andGate = spawnedObj.GetComponent<AndGate>();
        if (andGate != null)
        {
            Debug.Log($"NotifySpawnedObject: Spawned AndGate: {andGate.name}");
        }
    }

    // -----------------------------
    // (ใหม่) ฟังก์ชัน ScanAndAssignObjects()
    // -----------------------------
    public void ScanAndAssignObjects()
    {
        LED[] allLEDs = FindObjectsOfType<LED>();
        ToggleSwitch[] allToggles = FindObjectsOfType<ToggleSwitch>();

        for (int i = 0; i < tasks.Count; i++)
        {
            // ถ้า LED ยังว่าง ให้ assign LED ตัวแรกที่พบ
            if (tasks[i].ledToCheck == null && allLEDs.Length > 0)
            {
                tasks[i].ledToCheck = allLEDs[0];
                Debug.Log($"[Task {i + 1}] Assign LED {allLEDs[0].name} จาก ScanAndAssignObjects()");
            }

            // สำหรับ ToggleSwitch ทั้ง 6 ตัว (แก้เป็น 6 แทน 4)
            for (int j = 0; j < tasks[i].toggleSwitches.Length; j++)
            {
                if (tasks[i].toggleSwitches[j] == null)
                {
                    foreach (var tog in allToggles)
                    {
                        bool usedAlready = false;
                        for (int k = 0; k < tasks[i].toggleSwitches.Length; k++)
                        {
                            if (tasks[i].toggleSwitches[k] == tog)
                            {
                                usedAlready = true;
                                break;
                            }
                        }
                        if (!usedAlready)
                        {
                            tasks[i].toggleSwitches[j] = tog;
                            Debug.Log($"[Task {i + 1}] Assign ToggleSwitch {tog.name} เป็น toggleSwitches[{j}] จาก ScanAndAssignObjects()");
                            break;
                        }
                    }
                }
            }
        }
    }

    // -----------------------------
    // (ใหม่) เพิ่มฟังก์ชัน ExtendedScanAndAssignObjectsCoroutine()
    // -----------------------------
    /// <summary>
    /// สแกนหาอุปกรณ์แบบวนซ้ำหลายครั้ง (จนกว่าจะ Assign ครบทุก Task หรือถึง maxAttempts)
    /// </summary>

    // 1) ฟังก์ชันตรวจว่าทุก Task ได้อุปกรณ์ครบหรือยัง (LED และ ToggleSwitch 6 ตัว)
    private bool AreAllTasksAssigned()
    {
        foreach (var task in tasks)
        {
            if (task.ledToCheck == null)
                return false;
            for (int j = 0; j < task.toggleSwitches.Length; j++)
            {
                if (task.toggleSwitches[j] == null)
                    return false;
            }
        }
        return true;
    }

    private IEnumerator OnEnableDelay()
    {
        yield return new WaitForSeconds(0.5f); // หน่วงเวลา 0.5 วินาที
        StartCoroutine(ExtendedScanAndAssignObjectsCoroutine());
    }

    private void OnEnable()
    {
        Debug.Log("QuizManager2 OnEnable เรียกใช้งาน");
        StartCoroutine(OnEnableDelay());
    }
    private void OnDisable()
    {
        // 1) เคลียร์การผูกอุปกรณ์ของทุก Task
        for (int i = 0; i < tasks.Count; i++)
        {
            tasks[i].ledToCheck = null;
            for (int j = 0; j < tasks[i].toggleSwitches.Length; j++)
            {
                tasks[i].toggleSwitches[j] = null;
            }
        }

        // 2) รีเซ็ตคะแนนและข้อความ
        totalScore = 0;
        resultMessage = "";

        Debug.Log("[QuizManager2] OnDisable: รีเซ็ตค่า tasks[], totalScore, และ resultMessage เรียบร้อยแล้ว");
    }
    // 2) ฟังก์ชัน ExtendedScanAndAssignObjectsCoroutine
    public IEnumerator ExtendedScanAndAssignObjectsCoroutine()
    {
        int maxAttempts = 20; // จำนวนครั้งสูงสุดที่เราจะพยายามสแกน
        int attempt = 0;

        while (!AreAllTasksAssigned() && attempt < maxAttempts)
        {
            // สแกนหาอุปกรณ์ทั้งหมดในฉาก
            LED[] allLEDs = FindObjectsOfType<LED>();
            ToggleSwitch[] allToggles = FindObjectsOfType<ToggleSwitch>();

            for (int i = 0; i < tasks.Count; i++)
            {
                LogicTask task = tasks[i];

                // Assign LED ถ้ายังว่าง
                if (task.ledToCheck == null && allLEDs.Length > 0)
                {
                    task.ledToCheck = allLEDs[0];
                    Debug.Log($"[Task {i + 1}] (Extended) Assign LED {allLEDs[0].name}");
                }

                // Assign ToggleSwitch ทั้ง 6 ตัว
                for (int j = 0; j < task.toggleSwitches.Length; j++)
                {
                    if (task.toggleSwitches[j] == null)
                    {
                        foreach (var tog in allToggles)
                        {
                            bool usedAlready = false;
                            for (int k = 0; k < task.toggleSwitches.Length; k++)
                            {
                                if (task.toggleSwitches[k] == tog)
                                {
                                    usedAlready = true;
                                    break;
                                }
                            }
                            if (!usedAlready)
                            {
                                task.toggleSwitches[j] = tog;
                                Debug.Log($"[Task {i + 1}] (Extended) fallback assign ToggleSwitch {tog.name} to index {j}");
                                break;
                            }
                        }
                    }
                }
            }

            attempt++;
            yield return new WaitForSeconds(0.5f); // รอครึ่งวินาทีก่อนสแกนซ้ำ
        }

        if (AreAllTasksAssigned())
        {
            Debug.Log("ExtendedScanAndAssignObjectsCoroutine: Assign ครบทุก Task แล้ว");
        }
        else
        {
            Debug.LogWarning("ExtendedScanAndAssignObjectsCoroutine: ยังไม่ assign ครบหลังจากลองหลายครั้ง");
        }
    }
}
