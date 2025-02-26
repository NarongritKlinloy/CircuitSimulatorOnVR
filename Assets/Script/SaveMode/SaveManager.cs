using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; // สำหรับ UnityWebRequest
using System.Text;

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
    
    // ฟิลด์เพิ่มเติมสำหรับบันทึกค่าเฉพาะของอุปกรณ์
    public double resistance;
    public double batteryVoltage;
}

[System.Serializable]
public class SaveData
{
    public List<DeviceState> deviceStates;
}

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
    public SaveData saveJson;
    public string simulateName;
    public string simulateDate;
}

public class SaveManager : MonoBehaviour
{
    public GameObject[] availablePrefabs;
    private int numSpawned = 1;
    private CircuitLab circuitLab;

    // URL สำหรับ API
    public string apiSaveUrl = "http://localhost:5000/api/simulator/save";
    public string apiLoadUrl = "http://localhost:5000/api/simulator/load";
    public string apiLoadByIdUrl = "http://localhost:5000/api/simulator/loadById";
    public int saveTypeCircuit = 1; // กำหนดประเภทการเซฟ (ถ้ามีการใช้งาน)

    private bool isLoading = false;

    void Start()
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

    // SaveGame จะรวบรวมข้อมูลจาก PlacedComponent ใน CircuitLab แล้วส่งข้อมูลไปยัง API
    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.deviceStates = new List<DeviceState>();

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

                // บันทึกค่าความต้านทาน ถ้าอุปกรณ์นั้นมี IResistor
                var resistor = comp.GameObject.GetComponent<IResistor>();
                if (resistor != null)
                {
                    state.resistance = resistor.Resistance;
                }
                // บันทึกค่าแรงดัน ถ้าอุปกรณ์นั้นมี IBattery
                var battery = comp.GameObject.GetComponent<IBattery>();
                if (battery != null)
                {
                    state.batteryVoltage = battery.BatteryVoltage;
                }

                data.deviceStates.Add(state);
            }
        }

        // ส่งข้อมูลไปยัง API แทนการเซฟลงเครื่อง
        StartCoroutine(SaveToDatabaseCoroutine(data));
    }

    private IEnumerator SaveToDatabaseCoroutine(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogWarning("ไม่พบ userId ที่ถูกต้องใน PlayerPrefs");
        }

        ServerSaveRequest requestBody = new ServerSaveRequest
        {
            userId = userId,
            saveJson = json,
            save_type = this.saveTypeCircuit
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
                Debug.Log("[SaveToDatabase] Success => " + request.downloadHandler.text);
                ServerSaveResponse serverResp = JsonUtility.FromJson<ServerSaveResponse>(request.downloadHandler.text);
                if (serverResp != null)
                {
                    Debug.Log("Server message: " + serverResp.message +
                              ", insertId: " + serverResp.insertId +
                              ", simulateName: " + serverResp.simulateName);
                }
            }
        }
    }

    // LoadGame ดึงข้อมูลจาก API (apiLoadUrl) แล้วทำการลบอุปกรณ์เก่าก่อนโหลดข้อมูลใหม่
    public void LoadGame()
    {
        if (isLoading)
        {
            Debug.LogWarning("Load operation is already in progress.");
            return;
        }
        isLoading = true;
        StartCoroutine(LoadFromDatabaseCoroutine());
    }

    private IEnumerator LoadFromDatabaseCoroutine()
    {
        string userId = PlayerPrefs.GetString("userId", "unknown");
        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            Debug.LogError("ไม่พบ userId สำหรับดึงข้อมูลจาก DB");
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
                Debug.Log("[LoadFromDatabase] => " + request.downloadHandler.text);
                ServerLoadResponse serverResp = JsonUtility.FromJson<ServerLoadResponse>(request.downloadHandler.text);
                if (serverResp == null || serverResp.saveJson == null)
                {
                    Debug.LogWarning("ไม่พบข้อมูล save จาก server");
                    isLoading = false;
                    yield break;
                }

                SaveData saveData = serverResp.saveJson;
                // ลบอุปกรณ์เก่าก่อนโหลด
                ClearCurrentDevices();
                yield return StartCoroutine(ResetAndLoad(saveData));
                isLoading = false;
            }
        }
    }

    // ฟังก์ชัน Load โดยใช้ saveId ผ่าน apiLoadByIdUrl
    public void LoadGameById(long saveId)
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

            Debug.Log("[LoadById] => " + request.downloadHandler.text);
            ServerLoadResponse serverResp = JsonUtility.FromJson<ServerLoadResponse>(request.downloadHandler.text);

            if (serverResp == null || serverResp.saveJson == null)
            {
                Debug.LogWarning("ไม่พบข้อมูล save จาก server สำหรับ saveId=" + saveId);
                isLoading = false;
                yield break;
            }

            SaveData saveData = serverResp.saveJson;
            if (saveData == null)
            {
                Debug.LogError("ไม่สามารถแปลงข้อมูล saveJson");
                isLoading = false;
                yield break;
            }

            ClearCurrentDevices();
            yield return StartCoroutine(ResetAndLoad(saveData));
            isLoading = false;
        }
    }

    // ลบอุปกรณ์ปัจจุบันทั้งหมดที่มีใน CircuitLab
    private void ClearCurrentDevices()
    {
        List<PlacedComponent> currentComponents = circuitLab.GetPlacedComponents();
        foreach (PlacedComponent comp in currentComponents)
        {
            if (comp.GameObject != null)
            {
                Destroy(comp.GameObject);
            }
        }
    }

    public IEnumerator ResetAndWait()
    {
        circuitLab.Reset();
        yield return new WaitForSeconds(3f);
    }

    private IEnumerator ResetAndLoad(SaveData data)
    {
        yield return StartCoroutine(ResetAndWait());
        yield return StartCoroutine(LoadGameSequentially(data));
    }

    private IEnumerator LoadGameSequentially(SaveData data)
    {
        numSpawned = 1;

        foreach (DeviceState state in data.deviceStates)
        {
            GameObject prefab = GetPrefabByIdentifier(state.prefabName);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                instance.name = "Component" + state.prefabName + numSpawned++;

                string pegName = "Peg_" + state.pegY + "_" + state.pegX;
                GameObject pegObj = GameObject.Find(pegName);
                if (pegObj != null)
                {
                    instance.transform.parent = pegObj.transform;
                    instance.transform.localPosition = new Vector3(state.positionX, state.positionY, state.positionZ);
                    instance.transform.localEulerAngles = new Vector3(state.rotationX, state.rotationY, state.rotationZ);

                    Debug.Log($"[LOAD] อุปกรณ์: {instance.name}, Position: {instance.transform.position}, LocalPosition: {instance.transform.localPosition}, Peg: {pegObj.name}");

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
                        Debug.Log($"[LOAD] RegisterComponent เรียกใช้สำเร็จ: {instance.name} กับ {pegObj.name}");
                        
                        yield return new WaitForEndOfFrame();

                        rb.isKinematic = true;
                        rb.useGravity = false;
                        Physics.Simulate(0.1f);
                    }
                    else
                    {
                        Debug.LogError($"[ERROR] ไม่พบ PegMgr บน Peg: {pegObj.name}");
                    }

                    // ตั้งค่าแรงดันของแบตเตอรี่ ถ้าอุปกรณ์นั้นเป็น IBattery
                    var battery = instance.GetComponent<IBattery>();
                    if (battery != null)
                    {
                        battery.BatteryVoltage = state.batteryVoltage;
                    }

                    // ตั้งค่าความต้านทาน ถ้าอุปกรณ์นั้นเป็น Bulb, Motor หรือ Flute
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

        Debug.Log("โหลดข้อมูลสำเร็จ");
    }

    private GameObject GetPrefabByIdentifier(string identifier)
    {
        foreach (GameObject prefab in availablePrefabs)
        {
            if (prefab.CompareTag(identifier))
                return prefab;
        }
        return null;
    }
}
