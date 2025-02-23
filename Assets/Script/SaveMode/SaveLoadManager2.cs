using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// SaveLoadManager2.cs
// จัดการการบันทึกและโหลดข้อมูลของอุปกรณ์ (ตำแหน่ง, การหมุน, สถานะ และการเชื่อมต่อสายไฟ)
// โดยใช้ข้อมูลจาก SpawnManager และ WireManager

public class SaveLoadManager2 : MonoBehaviour
{
    public float loadCooldown = 0.1f;
    private bool isLoading = false;
    private bool isSaving = false;

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
        public string outputName;  // เช่น "ToggleSwitch_1_OUT1"
        public string inputName;   // เช่น "AndGate_1_IN2"
        public bool isConnected;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<DeviceData> devices = new List<DeviceData>();
        public List<WireData> wires = new List<WireData>();
    }

    public SpawnManager spawnManager;
    public WireManager wireManager;

    // รายชื่อประเภทของอุปกรณ์ที่ถูก Spawn
    public string[] deviceTypes = new string[] {
        "AndGate", "OrGate", "NorGate", "NandGate", "XorGate", "XnorGate", "NotGate",
        "SevenSegment", "ToggleSwitch", "BinarySwitch", "Buzzer", "Clock", "JKFlipFlop", "LED"
    };

    private string saveFilePath;
    private Dictionary<string, GameObject> prefabMapping = new Dictionary<string, GameObject>();
    private Dictionary<string, Transform> spawnPointMapping = new Dictionary<string, Transform>();

    void Start()
    {
        saveFilePath = Application.persistentDataPath + "/saveFile.json";

        if (spawnManager != null)
        {
            // แมป Prefab กับชนิดอุปกรณ์
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

            // แมป SpawnPoint กับชนิดอุปกรณ์ (หากต้องการ)
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
            Debug.LogError("SpawnManager not assigned in SaveLoadManager2!");
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

    /// <summary>
    /// เรียกฟังก์ชั่น Save แบบหน่วงเวลา 3 วินาที
    /// </summary>
    public void Save()
    {
        if (isSaving)
        {
            Debug.LogWarning("Save operation is already in progress.");
            return;
        }
        StartCoroutine(DelayedSave());
    }

    private IEnumerator DelayedSave()
    {
        yield return new WaitForSeconds(3f);
        isSaving = true;

        SaveData saveData = new SaveData();
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        // เก็บข้อมูลอุปกรณ์ (หลัก)
        foreach (GameObject obj in allObjects)
        {
            foreach (string type in deviceTypes)
            {
                // เฉพาะอุปกรณ์หลักที่ชื่อเริ่มด้วย type + "_" และไม่ประกอบด้วย "_IN" หรือ "_OUT"
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
                        if (input != null)
                            state = input.isOn;
                        else if (output != null)
                            state = output.isOn;
                        data.state = state;
                    }
                    saveData.devices.Add(data);
                    break;
                }
            }
        }

        // Debug: แสดงจำนวนสายไฟใน WireManager
        var wireDict = wireManager.GetWireConnections();
        Debug.Log("[Save] Number of wires in dictionary: " + wireDict.Count);

        // เก็บข้อมูลสายไฟ (Child Connector)
        foreach (var connection in wireDict)
        {
            if (connection.Key.Item1 != null && connection.Key.Item2 != null)
            {
                // ใช้ชื่อ Child Connector จริง ๆ (เช่น "ToggleSwitch_1_OUT1", "AndGate_1_IN2")
                WireData wireData = new WireData();
                wireData.outputName = connection.Key.Item1.gameObject.name;
                wireData.inputName = connection.Key.Item2.gameObject.name;
                wireData.isConnected = true;
                saveData.wires.Add(wireData);
            }
        }

        Debug.Log("[Save] Total devices saved: " + saveData.devices.Count);
        Debug.Log("[Save] Total wires saved: " + saveData.wires.Count);

        // เขียนไฟล์ JSON
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("[Save] Saved data to " + saveFilePath);

        isSaving = false;
    }

    /// <summary>
    /// เรียกฟังก์ชั่น Load แบบหน่วงเวลา 3 วินาที
    /// </summary>
    public void Load()
    {
        if (isLoading)
        {
            Debug.LogWarning("Load operation is already in progress.");
            return;
        }
        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("Save file not found: " + saveFilePath);
            return;
        }
        isLoading = true;
        StartCoroutine(DelayedLoad());
    }

    private IEnumerator DelayedLoad()
    {
        yield return Wait();

        // อ่านไฟล์ JSON
        string json = File.ReadAllText(saveFilePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"[Load] Read file: {saveFilePath}");
        Debug.Log($"[Load] Devices in file: {saveData.devices.Count}, Wires in file: {saveData.wires.Count}");
        foreach (var w in saveData.wires)
        {
            Debug.Log($"[Load] wire => output={w.outputName}, input={w.inputName}, isConnected={w.isConnected}");
        }

        // ลบอุปกรณ์เก่าที่มีชื่อขึ้นต้นด้วย deviceType + "_"
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
        yield return new WaitForSeconds(0.1f);

        // โหลดอุปกรณ์และสายไฟ
        yield return StartCoroutine(LoadSequence(saveData));
        isLoading = false;
    }

    /// <summary>
    /// รอเวลา 3 วินาที
    /// </summary>
    public IEnumerator Wait()
    {
        yield return new WaitForSeconds(3f);
    }

    /// <summary>
    /// สร้างอุปกรณ์ตามข้อมูลที่เซฟไว้ แล้วจำลองการ pinch เพื่อสร้างสายไฟ
    /// </summary>
    private IEnumerator LoadSequence(SaveData saveData)
    {
        Dictionary<string, GameObject> loadedObjects = new Dictionary<string, GameObject>();

        // โหลดอุปกรณ์ทีละตัว
        foreach (DeviceData data in saveData.devices)
        {
            if (prefabMapping.ContainsKey(data.deviceType))
            {
                // สร้างอุปกรณ์
                GameObject prefab = prefabMapping[data.deviceType];
                GameObject newObj = Instantiate(prefab, data.position, data.rotation);
                newObj.name = data.objectName;

                Debug.Log("[LoadSequence] Loaded device: " + data.objectName);

                // ตั้งค่าสถานะ (isOn)
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

                // ถ้าเป็น ToggleSwitch ให้หมุน pivot ให้ตรง
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

                // 1) หาชื่อ Child Connector (OutputConnector / InputConnector หลายตัว)
                // 2) ตั้งชื่อให้ตรงกับที่เคยตั้งตอนเซฟ (ถ้าต้องการ)
                //    หรือใช้ index ช่วยนับ เช่น out1, out2, in1, in2
                // 3) บันทึกลง loadedObjects ด้วยชื่อเดียวกัน

                // ตัวอย่าง (สมมติ prefab มี 1 output, 2 input)
                // ถ้าคุณต้องการตั้งชื่อเอง เช่น AndGate_1_OUT, AndGate_1_IN1, AndGate_1_IN2
                int outIndex = 1;
                var outConnectors = newObj.GetComponentsInChildren<OutputConnector>();
                foreach (var oc in outConnectors)
                {
                    // สมมติให้ชื่อเป็น <parentName>_OUT<index>
                    string outName = $"{data.objectName}_OUT{outIndex}";
                    oc.gameObject.name = outName;
                    loadedObjects[outName] = oc.gameObject;
                    outIndex++;
                }

                int inIndex = 1;
                var inConnectors = newObj.GetComponentsInChildren<InputConnector>();
                foreach (var ic in inConnectors)
                {
                    string inName = $"{data.objectName}_IN{inIndex}";
                    ic.gameObject.name = inName;
                    loadedObjects[inName] = ic.gameObject;
                    inIndex++;
                }

                // นอกจากนี้ ถ้าอุปกรณ์ไม่มี child connector แต่ใช้ตัวเดียวกัน
                // (เช่น ToggleSwitch มี OutputConnector ติดอยู่ที่ root)
                // ก็อาจใช้ logic คล้ายๆ กัน

                // สุดท้าย บันทึก root object เผื่อใช้งาน
                loadedObjects[data.objectName] = newObj;
            }
            else
            {
                Debug.LogWarning("[LoadSequence] No prefab mapping for device type: " + data.deviceType);
            }
            yield return new WaitForSeconds(loadCooldown);
        }

        // โหลดสายไฟโดยใช้ simulate pinch
        foreach (WireData wire in saveData.wires)
        {
            if (wire.isConnected)
            {
                Debug.Log($"[LoadSequence] Processing wire: {wire.outputName} -> {wire.inputName}");
                if (loadedObjects.ContainsKey(wire.outputName) && loadedObjects.ContainsKey(wire.inputName))
                {
                    Debug.Log("[LoadSequence] Found devices for wire connection: " + wire.outputName + " -> " + wire.inputName);
                    GameObject outputObj = loadedObjects[wire.outputName];
                    GameObject inputObj = loadedObjects[wire.inputName];
                    OutputConnector outputCon = outputObj.GetComponent<OutputConnector>();
                    InputConnector inputCon = inputObj.GetComponent<InputConnector>();
                    if (outputCon != null && inputCon != null)
                    {
                        Debug.Log("[LoadSequence] Simulating pinch for wire: " + wire.outputName + " -> " + wire.inputName);
                        wireManager.SelectOutput(outputCon);
                        yield return new WaitForSeconds(0.05f);
                        wireManager.SelectInput(inputCon);
                        Debug.Log("[LoadSequence] Wire created for: " + wire.outputName + " -> " + wire.inputName);
                    }
                    else
                    {
                        Debug.LogWarning("[LoadSequence] Missing connector components for wire: " + wire.outputName + " -> " + wire.inputName);
                    }
                }
                else
                {
                    Debug.LogWarning("[LoadSequence] Could not find devices for wire connection: " + wire.outputName + " -> " + wire.inputName);
                }
            }
            else
            {
                Debug.Log($"[LoadSequence] Wire isConnected=false for: {wire.outputName} -> {wire.inputName}");
            }
            yield return new WaitForSeconds(loadCooldown);
        }
    }
}
