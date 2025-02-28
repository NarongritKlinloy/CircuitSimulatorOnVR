using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class QuizManager1 : MonoBehaviour
{
    // -----------------------------
    // 1) โครงสร้างของตารางความจริง (Truth Table)
    // -----------------------------
    [System.Serializable]
    public class TruthTableEntry
    {
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

        [Header("Toggle Switch (สมมติ 4 ตัว)")]
        public ToggleSwitch[] toggleSwitches = new ToggleSwitch[4];

        [Header("LED ที่ต้องตรวจ")]
        public LED ledToCheck;

        [Header("คะแนนของโจทย์นี้ (คะแนนเต็ม)")]
        public int score = 100;

        [Header("ตารางความจริง (Truth Table) สำหรับโจทย์นี้")]
        public List<TruthTableEntry> truthTableEntries = new List<TruthTableEntry>();

        // ฟิลด์สำหรับเก็บชื่ออุปกรณ์ที่ต้องการ (ถ้าต้องการ match ตามชื่อ)
        [Header("ชื่ออุปกรณ์ที่ต้องการ (ถ้าอยาก match ตามชื่อ)")]
        public string wantedLedName;
        public string[] wantedToggleNames = new string[4];
    }

    // -----------------------------
    // 3) ตัวแปรหลักของ QuizManager1
    // -----------------------------
    public List<LogicTask> tasks = new List<LogicTask>();
    public int totalScore;
    [TextArea(4, 8)] public string resultMessage;

    [System.Serializable]
    public class PracticeData
    {
        public int practice_id;
        public string practice_name;
        public string practice_detail;
        public int practice_score;
        public string create_date;
    }

    // -----------------------------
    // 4) โหลดคะแนนของ Practice จาก DB (ตัวอย่าง)
    // -----------------------------
    private void Start()
    {
        // โหลดคะแนนตัวอย่าง
        StartCoroutine(LoadPracticeScoreFromServer(1, 0));
        // เรียก Coroutine สแกนอุปกรณ์
        StartCoroutine(ExtendedScanAndAssignObjectsCoroutine());
    }

    IEnumerator LoadPracticeScoreFromServer(int practiceId, int taskIndex)
    {
        if (taskIndex < 0 || taskIndex >= tasks.Count)
        {
            Debug.LogWarning("taskIndex เกินขอบเขตของ tasks");
            yield break;
        }

        string url = "http://localhost:5000/api/practice/" + practiceId;
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
    private void OnEnable()
    {
        // เมื่อ GameObject ถูกเปิดใหม่ ให้เรียกสแกนหาอุปกรณ์อีกครั้ง
        StartCoroutine(ExtendedScanAndAssignObjectsCoroutine());
    }

    // -----------------------------
    // (ใหม่) เมื่อ GameObject ถูกปิด (Disable) ให้รีเซ็ตค่ากลับสู่สถานะเริ่มต้น
    // -----------------------------
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

        Debug.Log("[QuizManager1] OnDisable: รีเซ็ตค่า tasks[], totalScore, และ resultMessage เรียบร้อยแล้ว");
    }

    // -----------------------------
    // 5) ฟังก์ชัน CheckAllTasks
    // -----------------------------
    public void CheckAllTasks()
    {
        resultMessage = "";

        // ตรวจการผูกอุปกรณ์
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

        // หากผูกอุปกรณ์ครบแล้ว
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
    // 6) ฟังก์ชัน SubmitScore
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
        public string userId;
        public int practiceId;
        public QuizData quizData;
    }

    [System.Serializable]
    public class QuizData
    {
        public int score;
    }

    private IEnumerator SubmitScoreToServer(int score)
    {
        string userId = PlayerPrefs.GetString("userId", "");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("ไม่พบ userId ใน PlayerPrefs");
            yield break;
        }

        int practiceId = 1;

        ScoreRequestData requestData = new ScoreRequestData();
        requestData.userId = userId;
        requestData.practiceId = practiceId;
        requestData.quizData = new QuizData();
        requestData.quizData.score = score;

        string jsonBody = JsonUtility.ToJson(requestData);
        string url = "http://localhost:5000/api/saveScore";

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
            }
            else
            {
                Debug.LogError("Error saving score: " + request.error);
            }
        }
    }

    // -----------------------------
    // 7) บังคับอัปเดตวงจร
    // -----------------------------
    void ForceUpdateCircuit()
    {
        LED[] leds = FindObjectsOfType<LED>();
        foreach (LED led in leds) led.UpdateState();

        AndGate[] ands = FindObjectsOfType<AndGate>();
        foreach (AndGate a in ands) a.UpdateState();

        OrGate[] ors = FindObjectsOfType<OrGate>();
        foreach (OrGate o in ors) o.UpdateState();

        NandGate[] nands = FindObjectsOfType<NandGate>();
        foreach (NandGate n in nands) n.UpdateState();

        NorGate[] nors = FindObjectsOfType<NorGate>();
        foreach (NorGate n in nors) n.UpdateState();

        XorGate[] xors = FindObjectsOfType<XorGate>();
        foreach (XorGate x in xors) x.UpdateState();

        XnorGate[] xnors = FindObjectsOfType<XnorGate>();
        foreach (XnorGate x in xnors) x.UpdateState();

        NotGate[] nots = FindObjectsOfType<NotGate>();
        foreach (NotGate n in nots) n.UpdateState();
    }

    // -----------------------------
    // 8) ตรวจการเชื่อมต่อสายไฟ (DFS)
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
        if (led == null || led.input == null) return false;

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
    // 9) ตรวจว่ามี Gate อย่างน้อย 1 ตัว
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

        if (totalGateCount > 0)
        {
            return (true, "พบ Gate อย่างน้อย 1 ตัวในฉาก\n");
        }
        else
        {
            return (false, "ไม่พบ Gate ใด ๆ ในฉาก\n");
        }
    }

    // -----------------------------
    // 10) ตรวจตารางความจริงของโจทย์
    // -----------------------------
    public (bool, string) CheckTruthTableOutput(LogicTask task)
    {
        if (task.toggleSwitches == null || task.toggleSwitches.Length != 4)
            return (false, "CheckTruthTableOutput: ต้องมี ToggleSwitch 4 ตัว\n");
        if (task.ledToCheck == null)
            return (false, "CheckTruthTableOutput: LED ไม่ถูกผูกใน Task\n");

        bool allPassed = true;
        string errorMsg = "";

        foreach (var entry in task.truthTableEntries)
        {
            int combo = entry.input;
            for (int i = 0; i < 4; i++)
            {
                bool desiredState = ((combo >> i) & 1) == 1;
                if (task.toggleSwitches[i].isOn != desiredState)
                {
                    task.toggleSwitches[i].Toggle();
                }
            }

            ForceUpdateCircuit();

            bool ledState = (task.ledToCheck.input != null) ? task.ledToCheck.input.isOn : false;
            if (ledState != entry.expectedOutput)
            {
                allPassed = false;
                string binaryStr = Convert.ToString(combo, 2).PadLeft(4, '0');
                errorMsg += $"Combo {combo} (Toggle: {binaryStr}) -> คาด {entry.expectedOutput} แต่ได้ {ledState}\n";
            }
        }

        if (allPassed)
            return (true, "CheckTruthTableOutput: Output ตรงตามตารางความจริงทั้งหมด\n");
        else
            return (false, errorMsg);
    }

    // -----------------------------
    // 11) คำนวณคะแนน
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
    // 12) หาคะแนนเต็มรวม
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
    // 13) ฟังก์ชันผูกวัตถุที่ Spawn ใหม่ (ตัวอย่าง)
    // -----------------------------
    public void NotifySpawnedObject(GameObject spawnedObj)
    {
        Debug.Log($"[QuizManager1] Spawned: {spawnedObj.name}");

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
    // (เดิม) ฟังก์ชัน ScanAndAssignObjects()
    // -----------------------------
    public void ScanAndAssignObjects()
    {
        LED[] allLEDs = FindObjectsOfType<LED>();
        ToggleSwitch[] allToggles = FindObjectsOfType<ToggleSwitch>();

        for (int i = 0; i < tasks.Count; i++)
        {
            // 1) Assign LED ถ้ายังว่าง
            if (tasks[i].ledToCheck == null)
            {
                if (allLEDs.Length > 0)
                {
                    tasks[i].ledToCheck = allLEDs[0];
                    Debug.Log($"[Task {i + 1}] Assign LED {allLEDs[0].name} จาก ScanAndAssignObjects()");
                }
            }

            // 2) Assign ToggleSwitch ทั้ง 4 ตัว
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
                            Debug.Log($"[Task {i + 1}] Assign ToggleSwitch {tog.name} เป็น toggleSwitches[{j}]");
                            break;
                        }
                    }
                }
            }
        }
    }

    // -----------------------------
    // (ใหม่) ฟังก์ชัน ExtendedScanAndAssignObjects()
    // -----------------------------
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

    public IEnumerator ExtendedScanAndAssignObjectsCoroutine()
    {
        int maxAttempts = 20;
        int attempt = 0;

        while (!AreAllTasksAssigned() && attempt < maxAttempts)
        {
            LED[] allLEDs = FindObjectsOfType<LED>();
            ToggleSwitch[] allToggles = FindObjectsOfType<ToggleSwitch>();

            for (int i = 0; i < tasks.Count; i++)
            {
                LogicTask task = tasks[i];

                if (task.ledToCheck == null)
                {
                    if (!string.IsNullOrEmpty(task.wantedLedName))
                    {
                        foreach (var led in allLEDs)
                        {
                            if (string.Equals(led.gameObject.name, task.wantedLedName, StringComparison.OrdinalIgnoreCase))
                            {
                                task.ledToCheck = led;
                                Debug.Log($"[Task {i + 1}] (Extended) พบ LED \"{led.gameObject.name}\" ตรงกับ wantedLedName");
                                break;
                            }
                        }
                    }
                    if (task.ledToCheck == null && allLEDs.Length > 0)
                    {
                        task.ledToCheck = allLEDs[0];
                        Debug.Log($"[Task {i + 1}] (Extended) fallback assign LED \"{allLEDs[0].gameObject.name}\"");
                    }
                }

                for (int j = 0; j < task.toggleSwitches.Length; j++)
                {
                    if (task.toggleSwitches[j] == null)
                    {
                        if (task.wantedToggleNames != null && j < task.wantedToggleNames.Length &&
                            !string.IsNullOrEmpty(task.wantedToggleNames[j]))
                        {
                            string wantedName = task.wantedToggleNames[j];
                            foreach (var tog in allToggles)
                            {
                                if (string.Equals(tog.gameObject.name, wantedName, StringComparison.OrdinalIgnoreCase))
                                {
                                    task.toggleSwitches[j] = tog;
                                    Debug.Log($"[Task {i + 1}] (Extended) พบ ToggleSwitch \"{tog.gameObject.name}\" ตรงกับ wantedToggleNames[{j}]");
                                    break;
                                }
                            }
                        }

                        if (task.toggleSwitches[j] == null)
                        {
                            foreach (var tog in allToggles)
                            {
                                bool alreadyUsed = false;
                                for (int k = 0; k < task.toggleSwitches.Length; k++)
                                {
                                    if (task.toggleSwitches[k] == tog)
                                    {
                                        alreadyUsed = true;
                                        break;
                                    }
                                }
                                if (!alreadyUsed)
                                {
                                    task.toggleSwitches[j] = tog;
                                    Debug.Log($"[Task {i + 1}] (Extended) fallback assign ToggleSwitch \"{tog.gameObject.name}\" ที่ index {j}");
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            attempt++;
            yield return new WaitForSeconds(0.5f);
        }

        if (AreAllTasksAssigned())
            Debug.Log("ExtendedScanAndAssignObjects: Assign ครบทุก Task แล้ว");
        else
            Debug.LogWarning("ExtendedScanAndAssignObjects: ยังไม่ assign ครบหลังจากลองหลายครั้ง");
    }
}
