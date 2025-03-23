using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Linq;

public class CombinedSaveLoadPractice : MonoBehaviour
{
    // API endpoints
    public string apiSaveUrl = "https://smith11.ce.kmitl.ac.th/api/practice/save";
    public string apiLoadUrl = "https://smith11.ce.kmitl.ac.th/api/practice/load";
    public string apiLoadByIdUrl = "https://smith11.ce.kmitl.ac.th/api/practice/loadById";

    // Save type สำหรับ combined (เช่น ใช้ค่า 2 เพื่อระบุว่าข้อมูลเป็นแบบผสม)
    public int saveTypeCombined = 2;

    public SpawnManager spawnManager;
    public WireManager wireManager;
    public string[] deviceTypes = new string[] {
        "AndGate", "OrGate", "NorGate", "NandGate", "XorGate", "XnorGate", "NotGate",
        "SevenSegment", "ToggleSwitch", "BinarySwitch", "Buzzer", "Clock", "JKFlipFlop", "LED"
    };

    private Dictionary<string, GameObject> prefabMapping = new Dictionary<string, GameObject>();
    private Dictionary<string, Transform> spawnPointMapping = new Dictionary<string, Transform>();
   

    private bool isLoading = false;
    private bool isSaving = false;
    public float loadCooldown = 0.1f;
    

    void Start()
    {
        
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


    // ฟังก์ชัน Unified Save  Digital Simulation
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

                // ลบอุปกรณ์เก่าของ Digital Simulatio
                ClearDigitalDevices();

    
                // โหลดข้อมูล Digital หากมี
                if (unifiedData.digitalData != null)
                {
                    yield return StartCoroutine(LoadDigitalData(unifiedData.digitalData));
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

           
            ClearDigitalDevices();

            if (unifiedData.digitalData != null)
            {
                yield return StartCoroutine(LoadDigitalData(unifiedData.digitalData));
            }
        }
        isLoading = false;
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

}