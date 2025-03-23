using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Linq;

#region Data Classes

// สำหรับวงจร (Circuit Simulation)
[System.Serializable]
public class DeviceState
{
    
    public string prefabName;
    public int pegX;
    public int pegY;

    public float rotationX;
    public float rotationY;
    public float rotationZ;

    public float positionX;
    public float positionY;
    public float positionZ;

    // ค่าเพิ่มเติม
    public double resistance;
    public double batteryVoltage;
}

[System.Serializable]
public class CircuitSaveData
{
    public List<DeviceState> deviceStates;
}

// สำหรับดิจิทัล (Digital Simulation)
[System.Serializable]
public class DeviceData
{
    public string deviceType;
    public string objectName;
    public Vector3 position;
    public Quaternion rotation;
    public bool state;
}

[System.Serializable]
public class WireData
{
    public string outputName;
    public string inputName;
    public bool isConnected;
}

[System.Serializable]
public class DigitalSaveData
{
    public List<DeviceData> devices = new List<DeviceData>();
    public List<WireData> wires = new List<WireData>();
}

// ข้อมูลรวม (Unified Save Data)
[System.Serializable]
public class UnifiedSaveData
{
    public CircuitSaveData circuitData;
    public DigitalSaveData digitalData;
}

#endregion

#region Server Request/Response Classes

[System.Serializable]
public class ServerSaveRequest
{
    public string userId;
    public string saveJson;
    public int save_type;

}

[System.Serializable]
public class ServerSaveResponse
{
    public string message;
    public long insertId;
    public string simulateName;
}

[System.Serializable]
public class ServerLoadResponse
{
    public string message;
    public UnifiedSaveData saveJson;
    public string simulateName;
    public string simulateDate;
}

#endregion

// คลาสรวมสำหรับเซฟ/โหลด (Combined SaveLoad Manager)
// ฟังก์ชันในที่นี้ถูกรวมเอาจาก SaveManager.cs :contentReference[oaicite:0]{index=0} 
// และ SaveLoadManager2.cs :contentReference[oaicite:1]{index=1}
public class CombinedSaveLoadManager : MonoBehaviour
{
    // API endpoints
    public string apiSaveUrl = "https://smith11.ce.kmitl.ac.th/api/simulator/save";
    public string apiLoadUrl = "https://smith11.ce.kmitl.ac.th/api/simulator/load";
    public string apiLoadByIdUrl = "https://smith11.ce.kmitl.ac.th/api/simulator/loadById";

    // Save type สำหรับ combined (เช่น ใช้ค่า 2 เพื่อระบุว่าข้อมูลเป็นแบบผสม)
    public int saveTypeCombined = 2;

    #region References สำหรับ Circuit Simulation
    public GameObject[] availablePrefabs;  // ใช้สำหรับวงจร
    public CircuitLab circuitLab;          // อ้างอิงไปยัง CircuitLab ใน Scene
    #endregion

    #region References สำหรับ Digital Simulation
    public SpawnManager spawnManager;
    public WireManager wireManager;
    public string[] deviceTypes = new string[] {
        "AndGate", "OrGate", "NorGate", "NandGate", "XorGate", "XnorGate", "NotGate",
        "SevenSegment", "ToggleSwitch", "BinarySwitch", "Buzzer", "Clock", "JKFlipFlop", "LED"
    };

    private Dictionary<string, GameObject> prefabMapping = new Dictionary<string, GameObject>();
    private Dictionary<string, Transform> spawnPointMapping = new Dictionary<string, Transform>();
    #endregion

    #region Loading/Saving Flags & Parameters
    private bool isLoading = false;
    private bool isSaving = false;
    public float loadCooldown = 0.1f;
    #endregion

    void Start()
    {
        // ตั้งค่า CircuitLab หากยังไม่ได้กำหนด
        if (circuitLab == null)
        {
            GameObject labObj = GameObject.Find("CircuitLab");
            if (labObj)
            {
                circuitLab = labObj.GetComponent<CircuitLab>();
            }
            else
            {
                Debug.LogError("ไม่พบ GameObject 'CircuitLab'");
            }
        }

        // ตั้งค่า SpawnManager และ Mapping สำหรับ Digital Simulation
        if (spawnManager != null)
        {
            prefabMapping["AndGate"] = spawnManager.andGatePrefab;
            prefabMapping["OrGate"] = spawnManager.orGatePrefab;
            prefabMapping["NorGate"] = spawnManager.norGatePrefab;
            prefabMapping["NandGate"] = spawnManager.nandGatePrefab;
            prefabMapping["XorGate"] = spawnManager.xorGatePrefab;
            prefabMapping["XnorGate"] = spawnManager.xnorGatePrefab;
            prefabMapping["NotGate"] = spawnManager.notGatePrefab;
            prefabMapping["SevenSegment"] = spawnManager.sevenSegmentPrefab;
            prefabMapping["ToggleSwitch"] = spawnManager.toggleSwitchPrefab;
            prefabMapping["BinarySwitch"] = spawnManager.binarySwitchPrefab;
            prefabMapping["Buzzer"] = spawnManager.buzzerPrefab;
            prefabMapping["Clock"] = spawnManager.clockPrefab;
            prefabMapping["JKFlipFlop"] = spawnManager.jkFlipFlopPrefab;
            prefabMapping["LED"] = spawnManager.ledPrefab;

            spawnPointMapping["AndGate"] = spawnManager.andGateSpawnPoint;
            spawnPointMapping["OrGate"] = spawnManager.orGateSpawnPoint;
            spawnPointMapping["NorGate"] = spawnManager.norGateSpawnPoint;
            spawnPointMapping["NandGate"] = spawnManager.nandGateSpawnPoint;
            spawnPointMapping["XorGate"] = spawnManager.xorGateSpawnPoint;
            spawnPointMapping["XnorGate"] = spawnManager.xnorGateSpawnPoint;
            spawnPointMapping["NotGate"] = spawnManager.notGateSpawnPoint;
            spawnPointMapping["SevenSegment"] = spawnManager.sevenSegmentSpawnPoint;
            spawnPointMapping["ToggleSwitch"] = spawnManager.toggleSwitchSpawnPoint;
            spawnPointMapping["BinarySwitch"] = spawnManager.binarySwitchSpawnPoint;
            spawnPointMapping["Buzzer"] = spawnManager.buzzerSpawnPoint;
            spawnPointMapping["Clock"] = spawnManager.clockSpawnPoint;
            spawnPointMapping["JKFlipFlop"] = spawnManager.jkFlipFlopSpawnPoint;
            spawnPointMapping["LED"] = spawnManager.ledSpawnPoint;
        }
        else
        {
            Debug.LogError("SpawnManager not assigned in CombinedSaveLoadManager!");
        }

        if (wireManager == null)
        {
            wireManager = FindObjectOfType<WireManager>();
            if (wireManager == null)
            {
                Debug.LogError("WireManager not found in scene!");
            }
        }
    }

    #region Unified Save

    // ฟังก์ชัน Unified Save ที่เซฟข้อมูลทั้งจาก Circuit และ Digital Simulation
    public void SaveCombined()
    {
        if (isSaving)
        {
            Debug.LogWarning("Save operation is already in progress.");
            return;
        }
        StartCoroutine(CombinedSaveCoroutine());
    }

    private IEnumerator CombinedSaveCoroutine()
    {
        // หน่วงเวลา 3 วินาที (ตามแบบของ digital save)
        yield return new WaitForSeconds(3f);
        isSaving = true;

        UnifiedSaveData unifiedData = new UnifiedSaveData();

        // --- เซฟข้อมูล Circuit Simulation ---
        if (circuitLab != null)
        {
            CircuitSaveData circuitData = new CircuitSaveData();
            circuitData.deviceStates = new List<DeviceState>();

            List<PlacedComponent> placedComponents = circuitLab.GetPlacedComponents();
            foreach (PlacedComponent comp in placedComponents)
            {
                if (comp.GameObject.activeInHierarchy)
                {
                    DeviceState state = new DeviceState();
                    state.prefabName = comp.GameObject.tag;
                    state.pegX = comp.Start.x;
                    state.pegY = comp.Start.y;
                    state.rotationX = comp.GameObject.transform.localEulerAngles.x;
                    state.rotationY = comp.GameObject.transform.localEulerAngles.y;
                    state.rotationZ = comp.GameObject.transform.localEulerAngles.z;
                    state.positionX = comp.GameObject.transform.localPosition.x;
                    state.positionY = comp.GameObject.transform.localPosition.y;
                    state.positionZ = comp.GameObject.transform.localPosition.z;

                    // บันทึกค่าความต้านทาน (หากมี)
                    var resistor = comp.GameObject.GetComponent<IResistor>();
                    if (resistor != null)
                    {
                        state.resistance = resistor.Resistance;
                    }
                    // บันทึกค่าแรงดันแบตเตอรี่ (หากมี)
                    var battery = comp.GameObject.GetComponent<IBattery>();
                    if (battery != null)
                    {
                        state.batteryVoltage = battery.BatteryVoltage;
                    }

                    circuitData.deviceStates.Add(state);
                }
            }
            unifiedData.circuitData = circuitData;
        }

        // --- เซฟข้อมูล Digital Simulation ---
        if (spawnManager != null)
        {
            DigitalSaveData digitalData = new DigitalSaveData();

            // เซฟ Device
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                foreach (string type in deviceTypes)
                {
                    if (obj.name.StartsWith(type + "_") &&
                        !obj.name.Contains("_IN") && !obj.name.Contains("_OUT"))
                    {
                        DeviceData data = new DeviceData();
                        data.deviceType = type;
                        data.objectName = obj.name;
                        data.position = obj.transform.position;
                        data.rotation = obj.transform.rotation;

                        var toggle = obj.GetComponent<ToggleSwitch>();
                        if (toggle != null)
                        {
                            data.state = toggle.isOn;
                        }
                        else
                        {
                            bool state = false;
                            var input = obj.GetComponent<InputConnector>();
                            var output = obj.GetComponent<OutputConnector>();
                            if (input != null) state = input.isOn;
                            else if (output != null) state = output.isOn;
                            data.state = state;
                        }
                        digitalData.devices.Add(data);
                        break;
                    }
                }
            }

            // เซฟสายไฟ
            var wireDict = wireManager.GetWireConnections();
            Debug.Log("[Combined Save] Number of wires: " + wireDict.Count);
            foreach (var connection in wireDict)
            {
                if (connection.Key.Item1 != null && connection.Key.Item2 != null)
                {
                    WireData wireData = new WireData();
                    wireData.outputName = connection.Key.Item1.gameObject.name;
                    wireData.inputName = connection.Key.Item2.gameObject.name;
                    wireData.isConnected = true;
                    digitalData.wires.Add(wireData);
                }
            }
            unifiedData.digitalData = digitalData;
        }

        // แปลงข้อมูลรวมเป็น JSON
        string json = JsonUtility.ToJson(unifiedData, true);
        Debug.Log("Unified Save JSON: " + json);

        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("ไม่พบ userId ที่ถูกต้องใน PlayerPrefs");
        }

        ServerSaveRequest requestBody = new ServerSaveRequest
        {
            userId = userId,
            saveJson = json,
            save_type = saveTypeCombined
        };

        string bodyJsonString = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);

        using (UnityWebRequest request = new UnityWebRequest(apiSaveUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Save to DB failed: " + request.error);
            }
            else
            {
                Debug.Log("[Combined SaveToDatabase] Success => " + request.downloadHandler.text);
                ServerSaveResponse serverResp = JsonUtility.FromJson<ServerSaveResponse>(request.downloadHandler.text);
                if (serverResp != null)
                {
                    Debug.Log("Server message: " + serverResp.message +
                              ", insertId: " + serverResp.insertId +
                              ", simulateName: " + serverResp.simulateName);
                }
            }
        }

        isSaving = false;
    }

    public string apiUpdateUrl = "https://smith11.ce.kmitl.ac.th/api/simulator/update";

    public void UpdateCombined(long saveId)
    {
        if (isSaving)
        {
            Debug.LogWarning("Update operation is already in progress.");
            return;
        }
        StartCoroutine(CombinedUpdateCoroutine(saveId));
    }

    private IEnumerator CombinedUpdateCoroutine(long saveId)
    {
        // หน่วงเวลา 3 วินาที (ตามแบบของ digital save)
        yield return new WaitForSeconds(3f);
        isSaving = true;

        UnifiedSaveData unifiedData = new UnifiedSaveData();

        // --- เซฟข้อมูล Circuit Simulation ---
        if (circuitLab != null)
        {
            CircuitSaveData circuitData = new CircuitSaveData();
            circuitData.deviceStates = new List<DeviceState>();

            List<PlacedComponent> placedComponents = circuitLab.GetPlacedComponents();
            foreach (PlacedComponent comp in placedComponents)
            {
                if (comp.GameObject.activeInHierarchy)
                {
                    DeviceState state = new DeviceState();
                    state.prefabName = comp.GameObject.tag;
                    state.pegX = comp.Start.x;
                    state.pegY = comp.Start.y;
                    state.rotationX = comp.GameObject.transform.localEulerAngles.x;
                    state.rotationY = comp.GameObject.transform.localEulerAngles.y;
                    state.rotationZ = comp.GameObject.transform.localEulerAngles.z;
                    state.positionX = comp.GameObject.transform.localPosition.x;
                    state.positionY = comp.GameObject.transform.localPosition.y;
                    state.positionZ = comp.GameObject.transform.localPosition.z;

                    // บันทึกค่าความต้านทาน (หากมี)
                    var resistor = comp.GameObject.GetComponent<IResistor>();
                    if (resistor != null)
                    {
                        state.resistance = resistor.Resistance;
                    }
                    // บันทึกค่าแรงดันแบตเตอรี่ (หากมี)
                    var battery = comp.GameObject.GetComponent<IBattery>();
                    if (battery != null)
                    {
                        state.batteryVoltage = battery.BatteryVoltage;
                    }

                    circuitData.deviceStates.Add(state);
                }
            }
            unifiedData.circuitData = circuitData;
        }

        // --- เซฟข้อมูล Digital Simulation ---
        if (spawnManager != null)
        {
            DigitalSaveData digitalData = new DigitalSaveData();

            // เซฟ Device
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                foreach (string type in deviceTypes)
                {
                    if (obj.name.StartsWith(type + "_") &&
                        !obj.name.Contains("_IN") && !obj.name.Contains("_OUT"))
                    {
                        DeviceData data = new DeviceData();
                        data.deviceType = type;
                        data.objectName = obj.name;
                        data.position = obj.transform.position;
                        data.rotation = obj.transform.rotation;

                        var toggle = obj.GetComponent<ToggleSwitch>();
                        if (toggle != null)
                        {
                            data.state = toggle.isOn;
                        }
                        else
                        {
                            bool state = false;
                            var input = obj.GetComponent<InputConnector>();
                            var output = obj.GetComponent<OutputConnector>();
                            if (input != null) state = input.isOn;
                            else if (output != null) state = output.isOn;
                            data.state = state;
                        }
                        digitalData.devices.Add(data);
                        break;
                    }
                }
            }

            // เซฟสายไฟ
            var wireDict = wireManager.GetWireConnections();
            Debug.Log("[Combined Save] Number of wires: " + wireDict.Count);
            foreach (var connection in wireDict)
            {
                if (connection.Key.Item1 != null && connection.Key.Item2 != null)
                {
                    WireData wireData = new WireData();
                    wireData.outputName = connection.Key.Item1.gameObject.name;
                    wireData.inputName = connection.Key.Item2.gameObject.name;
                    wireData.isConnected = true;
                    digitalData.wires.Add(wireData);
                }
            }
            unifiedData.digitalData = digitalData;
        }

        // แปลงข้อมูลรวมเป็น JSON
        string json = JsonUtility.ToJson(unifiedData, true);
        Debug.Log("Unified Update JSON: " + json);

        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("ไม่พบ userId ที่ถูกต้องใน PlayerPrefs");
        }

        // สร้าง Request Body สำหรับ update
        ServerSaveRequest requestBody = new ServerSaveRequest
        {
            userId = userId,
            saveJson = json,
            save_type = saveTypeCombined
        };

        string bodyJsonString = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);

        using (UnityWebRequest request = new UnityWebRequest(apiUpdateUrl + "?saveId=" + saveId, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Update to DB failed: " + request.error);
            }
            else
            {
                Debug.Log("[Combined UpdateToDatabase] Success => " + request.downloadHandler.text);
                // สามารถอ่าน response ได้เช่นเดียวกัน
            }
        }

        isSaving = false;
    }


    #endregion

    #region Unified Load

    // ฟังก์ชัน Unified Load ที่ดึงข้อมูลจาก API และโหลดข้อมูลของทั้งสองส่วน
    public void LoadCombined()
    {
        if (isLoading)
        {
            Debug.LogWarning("Load operation is already in progress.");
            return;
        }
        isLoading = true;
        StartCoroutine(CombinedLoadCoroutine());
    }

    private IEnumerator CombinedLoadCoroutine()
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogError("ไม่พบ userId สำหรับดึงข้อมูลจาก DB");
            isLoading = false;
            yield break;
        }

        string urlWithParam = apiLoadUrl + "?userId=" + userId;
        using (UnityWebRequest request = UnityWebRequest.Get(urlWithParam))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Load from DB failed: " + request.error);
                isLoading = false;
                yield break;
            }
            else
            {
                Debug.Log("[Combined LoadFromDatabase] => " + request.downloadHandler.text);
                ServerLoadResponse serverResp = JsonUtility.FromJson<ServerLoadResponse>(request.downloadHandler.text);
                if (serverResp == null || serverResp.saveJson == null)
                {
                    Debug.LogWarning("ไม่พบข้อมูล save จาก server");
                    isLoading = false;
                    yield break;
                }

                UnifiedSaveData unifiedData = serverResp.saveJson;

                // ลบอุปกรณ์เก่าของทั้ง Circuit และ Digital Simulation
                ClearCircuitDevices();
                ClearDigitalDevices();

                // โหลดข้อมูล Circuit หากมี
                if (unifiedData.circuitData != null)
                {
                    yield return StartCoroutine(LoadCircuitData(unifiedData.circuitData));
                }

                // โหลดข้อมูล Digital หากมี
                if (unifiedData.digitalData != null)
                {
                    yield return StartCoroutine(LoadDigitalData(unifiedData.digitalData));
                }
                // หลังโหลดเสร็จ ให้สแกนและผูกอุปกรณ์ใน QuizManager2
                QuizManager2 quiz = FindObjectOfType<QuizManager2>();
                if (quiz != null)
                {
                    quiz.ScanAndAssignObjects();
                }
            }
        }
        isLoading = false;
    }

    // โหลดแบบใช้ saveId
    public void LoadCombinedById(long saveId)
    {
        if (isLoading)
        {
            Debug.LogWarning("Load operation is already in progress.");
            return;
        }
        isLoading = true;
        StartCoroutine(CombinedLoadByIdCoroutine(saveId));
    }

    private IEnumerator CombinedLoadByIdCoroutine(long saveId)
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogError("ไม่พบ userId สำหรับดึงข้อมูลจาก DB");
            isLoading = false;
            yield break;
        }
        string urlWithParam = apiLoadByIdUrl + "?userId=" + userId + "&saveId=" + saveId;
        using (UnityWebRequest request = UnityWebRequest.Get(urlWithParam))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Load from DB (by ID) failed: " + request.error);
                isLoading = false;
                yield break;
            }

            Debug.Log("[Combined LoadById] => " + request.downloadHandler.text);
            ServerLoadResponse serverResp = JsonUtility.FromJson<ServerLoadResponse>(request.downloadHandler.text);

            if (serverResp == null || serverResp.saveJson == null)
            {
                Debug.LogWarning("ไม่พบข้อมูล save จาก server สำหรับ saveId=" + saveId);
                isLoading = false;
                yield break;
            }

            UnifiedSaveData unifiedData = serverResp.saveJson;
            if (unifiedData == null)
            {
                Debug.LogError("ไม่สามารถแปลงข้อมูล saveJson");
                isLoading = false;
                yield break;
            }

            ClearCircuitDevices();
            ClearDigitalDevices();

            if (unifiedData.circuitData != null)
            {
                yield return StartCoroutine(LoadCircuitData(unifiedData.circuitData));
            }
            if (unifiedData.digitalData != null)
            {
                yield return StartCoroutine(LoadDigitalData(unifiedData.digitalData));
            }
            // หลังจากโหลด Digital Data เสร็จ ให้สแกนและผูกอุปกรณ์ให้กับ QuizManager2 ด้วย
            QuizManager2 quiz = FindObjectOfType<QuizManager2>();
            if (quiz != null)
            {
                quiz.ScanAndAssignObjects();
            }
        }
        isLoading = false;
    }

    #endregion

    #region Clearing Methods

    // ลบอุปกรณ์ Circuit ที่มีอยู่
    public void ClearCircuitDevices()
    {
        // if (circuitLab != null)
        // {
        //     List<PlacedComponent> currentComponents = circuitLab.GetPlacedComponents();
        //     foreach (PlacedComponent comp in currentComponents)
        //     {
        //         if (comp.GameObject != null)
        //         {
        //             Destroy(comp.GameObject);
        //         }
        //     }
        // }
        if (circuitLab != null)
        {
            // เรียก Reset() เพื่อเคลียร์อุปกรณ์ทั้งหมดในวงจร 
            circuitLab.Reset();
        }
        
    }

    // ลบอุปกรณ์ Digital โดยตรวจสอบชื่อ (deviceTypes)
    public void ClearDigitalDevices()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            foreach (string type in deviceTypes)
            {
                if (obj.name.StartsWith(type + "_"))
                {
                    Destroy(obj);
                    break;
                }
            }
        }
    }

    #endregion

    #region Loading Circuit Data

    private IEnumerator LoadCircuitData(CircuitSaveData circuitData)
    {
        yield return StartCoroutine(ResetCircuitAndWait());
        yield return StartCoroutine(LoadCircuitSequentially(circuitData));
    }

    public IEnumerator ResetCircuitAndWait()
    {
        if (circuitLab != null)
        {
            circuitLab.Reset();
            yield return new WaitForSeconds(3f);
        }
    }

    private int numSpawnedCircuit = 1;
    private IEnumerator LoadCircuitSequentially(CircuitSaveData circuitData)
    {
        numSpawnedCircuit = 1;
        foreach (DeviceState state in circuitData.deviceStates)
        {
            GameObject prefab = GetCircuitPrefabByIdentifier(state.prefabName);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                instance.name = "Component" + state.prefabName + numSpawnedCircuit++;

                string pegName = "Peg_" + state.pegY + "_" + state.pegX;
                GameObject pegObj = GameObject.Find(pegName);
                if (pegObj != null)
                {
                    instance.transform.parent = pegObj.transform;
                    instance.transform.localPosition = new Vector3(state.positionX, state.positionY, state.positionZ);
                    instance.transform.localEulerAngles = new Vector3(state.rotationX, state.rotationY, state.rotationZ);

                    Debug.Log($"[Circuit LOAD] อุปกรณ์: {instance.name}, Peg: {pegObj.name}");

                    Rigidbody rb = instance.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = instance.AddComponent<Rigidbody>();
                    }
                    rb.isKinematic = false;
                    rb.useGravity = true;

                    PegMgr pegMgr = pegObj.GetComponent<PegMgr>();
                    if (pegMgr != null)
                    {
                        pegMgr.RegisterComponent(instance);
                        yield return new WaitForEndOfFrame();

                        rb.isKinematic = true;
                        rb.useGravity = false;
                        Physics.Simulate(0.1f);
                    }
                    else
                    {
                        Debug.LogError($"[ERROR] ไม่พบ PegMgr บน Peg: {pegObj.name}");
                    }

                    // ตั้งค่าแบตเตอรี่และความต้านทาน
                    var battery = instance.GetComponent<IBattery>();
                    if (battery != null)
                    {
                        battery.BatteryVoltage = state.batteryVoltage;
                    }
                    var bulb = instance.GetComponent<Bulb>();
                    if (bulb != null)
                    {
                        bulb.SetResistance(state.resistance);
                    }
                    else
                    {
                        var motor = instance.GetComponent<Motor>();
                        if (motor != null)
                        {
                            motor.SetResistance(state.resistance);
                        }
                        else
                        {
                            var flute = instance.GetComponent<Flute>();
                            if (flute != null)
                            {
                                flute.Resistance = state.resistance;
                            }
                        }
                    }

                    yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    Debug.LogWarning($"ไม่พบ Peg: {pegName}");
                }
            }
            else
            {
                Debug.LogWarning($"ไม่พบ Prefab สำหรับ Tag: {state.prefabName}");
            }
        }
        Debug.Log("โหลดข้อมูล Circuit Simulation สำเร็จ");
    }

    private GameObject GetCircuitPrefabByIdentifier(string identifier)
    {
        foreach (GameObject prefab in availablePrefabs)
        {
            if (prefab.CompareTag(identifier))
                return prefab;
        }
        return null;
    }

    #endregion

    #region Loading Digital Data

    private IEnumerator LoadDigitalData(DigitalSaveData digitalData)
    {
        Dictionary<string, GameObject> loadedObjects = new Dictionary<string, GameObject>();

        // โหลด Device สำหรับ Digital Simulation
        foreach (DeviceData data in digitalData.devices)
        {
            if (prefabMapping.ContainsKey(data.deviceType))
            {
                GameObject prefab = prefabMapping[data.deviceType];
                GameObject newObj = Instantiate(prefab, data.position, data.rotation);
                newObj.name = data.objectName;

                var input = newObj.GetComponent<InputConnector>();
                var output = newObj.GetComponent<OutputConnector>();
                if (input != null)
                {
                    input.isOn = data.state;
                    input.UpdateState();
                }
                else if (output != null)
                {
                    output.isOn = data.state;
                    output.UpdateState();
                }

                var toggle = newObj.GetComponent<ToggleSwitch>();
                if (toggle != null)
                {
                    toggle.isOn = data.state;
                    if (toggle.pivot != null)
                    {
                        var rot = toggle.pivot.transform.localEulerAngles;
                        rot.y = toggle.isOn ? 15f : -15f;
                        toggle.pivot.transform.localEulerAngles = rot;
                    }
                }

                var outConnectors = newObj.GetComponentsInChildren<OutputConnector>();
                if (outConnectors.Length > 0)
                {
                    outConnectors[0].gameObject.name = data.objectName + "_OUT";
                    loadedObjects[outConnectors[0].gameObject.name] = outConnectors[0].gameObject;
                }

                var inConnectors = newObj.GetComponentsInChildren<InputConnector>();
                for (int i = 0; i < inConnectors.Length; i++)
                {
                    string inName = data.objectName + "_IN" + (i + 1);
                    inConnectors[i].gameObject.name = inName;
                    loadedObjects[inName] = inConnectors[i].gameObject;
                }

                loadedObjects[data.objectName] = newObj;
            }
            else
            {
                Debug.LogWarning("[Digital LOAD] No prefab mapping for device type: " + data.deviceType);
            }
            yield return new WaitForSeconds(loadCooldown);
        }

        // เชื่อมสายไฟ
        foreach (WireData wire in digitalData.wires)
        {
            if (wire.isConnected)
            {
                if (loadedObjects.ContainsKey(wire.outputName) && loadedObjects.ContainsKey(wire.inputName))
                {
                    GameObject outputObj = loadedObjects[wire.outputName];
                    GameObject inputObj = loadedObjects[wire.inputName];
                    OutputConnector outputCon = outputObj.GetComponent<OutputConnector>();
                    InputConnector inputCon = inputObj.GetComponent<InputConnector>();
                    if (outputCon != null && inputCon != null)
                    {
                        wireManager.SelectOutput(outputCon);
                        yield return new WaitForSeconds(0.05f);
                        wireManager.SelectInput(inputCon);
                    }
                }
            }
            yield return new WaitForSeconds(loadCooldown);
        }
        Debug.Log("โหลดข้อมูล Digital Simulation สำเร็จ");
    }

    #endregion
}
