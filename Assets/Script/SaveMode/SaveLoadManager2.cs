using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking; // สำหรับ UnityWebRequest
using System.Text;           // สำหรับ Encoding
using System.IO;
using System;
using System.Linq;

public class SaveLoadManager2 : MonoBehaviour
{
    [Header("Save Type (ตัวเลข)")]
    public int saveTypeDigital = 0;
    public float loadCooldown = 0.1f;
    private bool isLoading = false;
    private bool isSaving = false;

    [SerializeField]
    private string saveFileName = "saveFileDigital.json";
    // ^ ไม่ได้ใช้แล้วแต่เก็บไว้เฉย ๆ

    private string apiSaveUrl = "https://smith11.ce.kmitl.ac.th/api/simulator/save";
    private string apiLoadUrl = "https://smith11.ce.kmitl.ac.th/api/simulator/load";
    // เพิ่มใหม่ไว้ใช้สำหรับ loadById
    private string apiLoadByIdUrl = "https://smith11.ce.kmitl.ac.th/api/simulator/loadById";

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
    public class SaveData
    {
        public List<DeviceData> devices = new List<DeviceData>();
        public List<WireData> wires = new List<WireData>();
    }

    [Serializable]
    public class ServerSaveRequest
    {
        public string userId;
        public string saveJson;
        public int save_type;
    }

    [Serializable]
    public class ServerSaveResponse
    {
        public string message;
        public long insertId;
        public string simulateName;
    }

    [Serializable]
    public class ServerLoadResponse
    {
        public string message;
        public SaveData saveJson;
        public string simulateName;
        public string simulateDate;
    }

    public SpawnManager spawnManager;
    public WireManager wireManager;

    // รายชื่อ Prefab Device
    public string[] deviceTypes = new string[] {
        "AndGate", "OrGate", "NorGate", "NandGate", "XorGate", "XnorGate", "NotGate",
        "SevenSegment", "ToggleSwitch", "BinarySwitch", "Buzzer", "Clock", "JKFlipFlop", "LED"
    };

    private Dictionary<string, GameObject> prefabMapping = new Dictionary<string, GameObject>();
    private Dictionary<string, Transform> spawnPointMapping = new Dictionary<string, Transform>();

    void Start()
    {
        if (spawnManager != null)
        {
            // แมป Prefab
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

            // แมป SpawnPoint
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

    // เรียก Save แบบหน่วงเวลา 3 วินาที
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

        // 1) เก็บข้อมูลลง SaveData
        SaveData saveData = new SaveData();
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        // เก็บ Device
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
                    saveData.devices.Add(data);
                    break;
                }
            }
        }

        // เก็บสายไฟ
        var wireDict = wireManager.GetWireConnections();
        Debug.Log("[Save] Number of wires in dictionary: " + wireDict.Count);

        foreach (var connection in wireDict)
        {
            if (connection.Key.Item1 != null && connection.Key.Item2 != null)
            {
                WireData wireData = new WireData();
                wireData.outputName = connection.Key.Item1.gameObject.name;
                wireData.inputName = connection.Key.Item2.gameObject.name;
                wireData.isConnected = true;
                saveData.wires.Add(wireData);
            }
        }

        // 2) เรียก API Save to Database
        yield return StartCoroutine(SaveToDatabaseCoroutine(saveData));

        isSaving = false;
    }

    private IEnumerator SaveToDatabaseCoroutine(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("No valid userId found in PlayerPrefs. Save might not work properly.");
        }


        ServerSaveRequest requestBody = new ServerSaveRequest
        {
            userId = userId,
            saveJson = json,
            save_type = this.saveTypeDigital

        };
        Debug.Log($"test Save _ type : {requestBody.save_type}");

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
                Debug.Log("[SaveToDatabase] Success => " + request.downloadHandler.text);
                ServerSaveResponse serverResp =
                    JsonUtility.FromJson<ServerSaveResponse>(request.downloadHandler.text);
                if (serverResp != null)
                {
                    Debug.Log("Server message: " + serverResp.message +
                              ", insertId: " + serverResp.insertId +
                              ", simulateName: " + serverResp.simulateName);
                }
            }
        }
    }

    // เรียก Load แบบหน่วง 3 วินาที
    public void Load()
    {
        if (isLoading)
        {
            Debug.LogWarning("Load operation is already in progress.");
            return;
        }
        isLoading = true;
        StartCoroutine(DelayedLoad());
    }

    private IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(LoadFromDatabaseCoroutine());
        isLoading = false;
    }

    private IEnumerator LoadFromDatabaseCoroutine()
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogError("No userId found. Cannot load data from DB.");
            yield break;
        }

        string urlWithParam = apiLoadUrl + "?userId=" + userId;
        using (UnityWebRequest request = UnityWebRequest.Get(urlWithParam))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Load from DB failed: " + request.error);
                yield break;
            }
            else
            {
                Debug.Log("[LoadFromDatabase] => " + request.downloadHandler.text);
                ServerLoadResponse serverResp =
                    JsonUtility.FromJson<ServerLoadResponse>(request.downloadHandler.text);
                if (serverResp == null || serverResp.saveJson == null)
                {
                    Debug.LogWarning("No save data returned from server.");
                    yield break;
                }

                SaveData saveData = serverResp.saveJson;
                if (saveData == null)
                {
                    Debug.LogError("Failed to parse saveJson");
                    yield break;
                }

                ClearCurrentDevices();
                yield return StartCoroutine(LoadSequence(saveData));
            }
        }
    }

    // เพิ่มเมธอดสำหรับ Load ตาม ID
    public void LoadById(long saveId)
    {
        if (isLoading)
        {
            Debug.LogWarning("Load operation is already in progress.");
            return;
        }
        isLoading = true;
        StartCoroutine(LoadByIdCoroutine(saveId));
    }

    private IEnumerator LoadByIdCoroutine(long saveId)
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogError("No userId found. Cannot load data from DB.");
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

            Debug.Log("[LoadById] => " + request.downloadHandler.text);
            ServerLoadResponse serverResp =
                JsonUtility.FromJson<ServerLoadResponse>(request.downloadHandler.text);

            if (serverResp == null || serverResp.saveJson == null)
            {
                Debug.LogWarning("No save data returned from server for saveId=" + saveId);
                isLoading = false;
                yield break;
            }

            SaveData saveData = serverResp.saveJson;
            if (saveData == null)
            {
                Debug.LogError("Failed to parse saveJson");
                isLoading = false;
                yield break;
            }

            ClearCurrentDevices();

            Debug.Log($"test 2 : {saveData} ");

            yield return StartCoroutine(LoadSequence(saveData));
            isLoading = false;
        }
    }

    private void ClearCurrentDevices()
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

    private IEnumerator LoadSequence(SaveData saveData)
    {
        Dictionary<string, GameObject> loadedObjects = new Dictionary<string, GameObject>();

        // โหลด Device
        foreach (DeviceData data in saveData.devices)
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
                Debug.LogWarning("[LoadSequence] No prefab mapping for device type: " + data.deviceType);
            }
            yield return new WaitForSeconds(loadCooldown);
        }

        // เชื่อมสายไฟ
        foreach (WireData wire in saveData.wires)
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
    }
}
