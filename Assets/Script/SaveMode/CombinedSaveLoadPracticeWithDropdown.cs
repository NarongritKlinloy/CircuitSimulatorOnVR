using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;               // สำหรับ TMP_Dropdown และ TMP_Text
using System.Text;
using System;
using System.Linq;


/// <summary>
/// CombinedSaveLoadPracticeWithDropdown.cs
/// รวมฟังก์ชัน Save/Load/Update สำหรับ Digital Simulation พร้อม Dropdown UI สำหรับเลือกเซฟ/โหลด/ลบ
/// </summary>
public class CombinedSaveLoadPracticeWithDropdown : MonoBehaviour
{
    #region API & Save Settings
    public string apiSaveUrl = "https://smith11.ce.kmitl.ac.th/api/practice/save";
    public string apiLoadUrl = "https://smith11.ce.kmitl.ac.th/api/practice/load";
    public string apiLoadByIdUrl = "https://smith11.ce.kmitl.ac.th/api/practice/loadById";
    public string apiUpdateUrl = "https://smith11.ce.kmitl.ac.th/api/practice/Update";
    // Save type สำหรับ combined (ตัวอย่าง)
    public int saveTypeCombined = 2;
    public float loadCooldown = 0.1f;
    private bool isLoading = false;
    private bool isSaving = false;
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

    #region Dropdown & UI References
    [Header("Dropdown & UI")]
    [Tooltip("Dropdown สำหรับ Save, Load, และ Delete (หน้า Save)")]
    public TMP_Dropdown saveDropdown;
    [Tooltip("Dropdown สำหรับหน้าที่สอง (แสดงรายการเซฟเหมือนกัน)")]
    public TMP_Dropdown secondaryDropdown;
    [Tooltip("Status Text สำหรับ Save")]
    public TMP_Text statusSave;
    [Tooltip("Status Text สำหรับ Load")]
    public TMP_Text statusLoad;
    [Tooltip("Status Text สำหรับ Delete")]
    public TMP_Text statusDelete;
    #endregion

    #region Other UI & Script References
    public ManagementCanvas managementCanvas;
    public SoundManager soundManager;
    #endregion

    #region Dropdown Data
    private List<SimulatorSaveItem> allUserSaves = new List<SimulatorSaveItem>();
    private float refreshInterval = 5f;
    private SimulatorSaveItem selectedSaveForConfirm = null;
    #endregion

    void Start()
    {
        // ตั้งค่า Mapping สำหรับ SpawnManager
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
            Debug.LogError("SpawnManager not assigned in CombinedSaveLoadPracticeWithDropdown!");
        }
        if (wireManager == null)
        {
            wireManager = FindObjectOfType<WireManager>();
            if (wireManager == null)
            {
                Debug.LogError("WireManager not found in scene!");
            }
        }

        // ตั้งค่า Dropdown ทั้งสองให้มีค่าเริ่มต้น
        if (saveDropdown != null)
        {
            saveDropdown.ClearOptions();
            List<string> options = new List<string> { "New Save" };
            saveDropdown.AddOptions(options);
            saveDropdown.value = 0;
        }
        if (secondaryDropdown != null)
        {
            secondaryDropdown.ClearOptions();
            List<string> secOptions = new List<string> { "New Save" };
            secondaryDropdown.AddOptions(secOptions);
            secondaryDropdown.value = 0;
        }

        // ดึงรายการเซฟจากเซิร์ฟเวอร์ตอนเริ่มต้น
        StartCoroutine(GetAllSavesFromServer(keepSelection: false));
        StartCoroutine(AutoRefreshLoop());
    }

    private IEnumerator AutoRefreshLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            StartCoroutine(GetAllSavesFromServer(keepSelection: true));
        }
    }

    private IEnumerator GetAllSavesFromServer(bool keepSelection)
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("No userId in PlayerPrefs => cannot fetch saves from server.");
            yield break;
        }

        string url = "https://smith11.ce.kmitl.ac.th/api/simulator/listSavesDigital?userId=" + userId;
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get list of saves failed: " + req.error);
            }
            else
            {
                string jsonText = req.downloadHandler.text;
                var newList = JsonArrayHelper.FromJson<SimulatorSaveItem>(jsonText);
                if (newList == null)
                {
                    newList = new List<SimulatorSaveItem>();
                }
                allUserSaves = newList;
                UpdateDropdown(saveDropdown, keepSelection);
                UpdateDropdown(secondaryDropdown, keepSelection);
            }
        }
    }

    private void UpdateDropdown(TMP_Dropdown dropdown, bool keepSelection)
    {
        if (dropdown == null)
            return;
        int oldIndex = dropdown.value;
        long oldSelectedId = -1;
        if (oldIndex > 0 && oldIndex - 1 < allUserSaves.Count)
        {
            oldSelectedId = allUserSaves[oldIndex - 1].circuit_id;
        }
        dropdown.options.Clear();
        dropdown.options.Add(new TMP_Dropdown.OptionData("New Save"));
        foreach (var item in allUserSaves)
        {
            string displayText = $"{item.circuit_name} ({item.circuit_date})";
            dropdown.options.Add(new TMP_Dropdown.OptionData(displayText));
        }
        if (keepSelection && oldSelectedId != -1)
        {
            int newIndex = allUserSaves.FindIndex(s => s.circuit_id == oldSelectedId);
            dropdown.value = (newIndex >= 0) ? newIndex + 1 : 0;
        }
        else
        {
            dropdown.value = 0;
        }
        dropdown.RefreshShownValue();
    }

    #region Save / Load / Delete Functions (Dropdown UI)
    // ---------- Save ----------
    public void OnClick_ConfirmSave()
    {
        if (soundManager != null)
            soundManager.PlayButtonSound();
        int index = saveDropdown.value;
        if (index == 0)
        {
            Debug.Log("User chooses to create a New Save");
            SaveCombined();
            statusSave.text = "Save new data Success!";
            managementCanvas.ShowUiNotifySaveSuccess();
        }
        else
        {
            int dataIndex = index - 1;
            if (dataIndex >= 0 && dataIndex < allUserSaves.Count)
            {
                selectedSaveForConfirm = allUserSaves[dataIndex];
                Debug.Log($"User chooses to overwrite: {selectedSaveForConfirm.circuit_name} (ID={selectedSaveForConfirm.circuit_id})");
                managementCanvas.ShowUiNotifyConfrimSave();
            }
        }
    }

    public void OnConfirmOverwriteSave()
    {
        if (selectedSaveForConfirm != null)
        {
            UpdateCombined(selectedSaveForConfirm.circuit_id);
            statusSave.text = "Overwriting save data Success!";
            managementCanvas.ShowUiNotifySaveSuccess();
            selectedSaveForConfirm = null;
        }
    }

    // ---------- Load ----------
    public void OnClick_ConfirmLoad()
    {
        if (soundManager != null)
            soundManager.PlayButtonSound();
        int index = saveDropdown.value;
        if (index == 0)
        {
            Debug.Log("No save chosen to load.");
            statusLoad.text = "No save selected for loading.";
            return;
        }
        int dataIndex = index - 1;
        if (dataIndex >= 0 && dataIndex < allUserSaves.Count)
        {
            var chosen = allUserSaves[dataIndex];
            Debug.Log($"User chooses to load: {chosen.circuit_name} (ID={chosen.circuit_id})");
            LoadCombinedById(chosen.circuit_id);
            statusLoad.text = "Loading save data Success!";
            managementCanvas.ShowUiNotifyLoadSuccess();
        }
    }

    // ---------- Delete ----------
    public void OnClick_ConfirmDelete()
    {
        if (soundManager != null)
            soundManager.PlayButtonSound();
        int index = saveDropdown.value;
        if (index == 0)
        {
            Debug.Log("No save chosen to delete.");
            statusDelete.text = "No save selected for deletion.";
            return;
        }
        int dataIndex = index - 1;
        if (dataIndex >= 0 && dataIndex < allUserSaves.Count)
        {
            selectedSaveForConfirm = allUserSaves[dataIndex];
            Debug.Log($"User chooses to delete: {selectedSaveForConfirm.circuit_name} (ID={selectedSaveForConfirm.circuit_id})");
            managementCanvas.ShowUiNotifyConfrimDelete();
        }
    }

    public void OnConfirmDelete()
    {
        if (selectedSaveForConfirm != null)
        {
            StartCoroutine(DeleteSaveFromServer(selectedSaveForConfirm.circuit_id));
            statusDelete.text = "Delete save data Success!";
            managementCanvas.ShowUiNotifyDelete();
            selectedSaveForConfirm = null;
        }
    }

    private IEnumerator DeleteSaveFromServer(long saveId)
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("No userId => cannot delete save from server.");
            yield break;
        }
        string url = $"https://smith11.ce.kmitl.ac.th/api/simulator/deleteById?userId={userId}&saveId={saveId}";
        using (UnityWebRequest req = UnityWebRequest.Delete(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Delete save failed: " + req.error);
                statusDelete.text = "Delete failed: " + req.error;
            }
            else
            {
                Debug.Log("Delete success => " + req.downloadHandler.text);
                statusDelete.text = "Delete successful!";
                StartCoroutine(GetAllSavesFromServer(keepSelection: false));
            }
        }
    }
    #endregion

    #region Unified Save / Update Functions
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
        yield return new WaitForSeconds(3f);
        isSaving = true;

        UnifiedSaveData unifiedData = new UnifiedSaveData();
        DigitalSaveData digitalData = new DigitalSaveData();

        // เซฟ Device สำหรับ Digital Simulation
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

        string json = JsonUtility.ToJson(unifiedData, true);
        Debug.Log("Unified Save JSON: " + json);

        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("No valid userId in PlayerPrefs");
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
        yield return new WaitForSeconds(3f);
        isSaving = true;

        UnifiedSaveData unifiedData = new UnifiedSaveData();
        DigitalSaveData digitalData = new DigitalSaveData();

        // เซฟ Device สำหรับ Digital Simulation
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

        string json = JsonUtility.ToJson(unifiedData, true);
        Debug.Log("Unified Update JSON: " + json);

        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("No valid userId in PlayerPrefs");
        }

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
            }
        }
        isSaving = false;
    }
    #endregion

    #region Unified Load Functions
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
                ClearDigitalDevices();

              
                if (unifiedData.digitalData != null)
                {
                    yield return StartCoroutine(LoadDigitalData(unifiedData.digitalData));
                }
            }
        }
        isLoading = false;
    }

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
    #endregion

   

    private void ClearDigitalDevices()
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
