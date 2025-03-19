using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public class GoogleAuthen : MonoBehaviour
{
    public TMP_Text statusText;
    private string clientId = "536241701089-ej2lkeskgljs17a9dp6d3eeorfhb2f2e.apps.googleusercontent.com";
    private string redirectUri = "https://smith11.ce.kmitl.ac.th/callback";
    private string authUrl;
    private string serverUrl = "https://smith11.ce.kmitl.ac.th/register";

    public ManagementCanvas managementCanvas;
    public CombinedSaveLoadManager combinedSaveLoadManager;

    [Header("XR Origin")]
    public GameObject xrOriginObject;

    [Header("Object พิเศษ")]
    public GameObject simulatorObject1;
    public GameObject simulatorObject2;

    void Start()
    {
        authUrl = "https://accounts.google.com/o/oauth2/auth" +
                  "?client_id=" + clientId +
                  "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                  "&response_type=token" +
                  "&scope=email%20profile%20openid" +
                  "&prompt=select_account";

        Application.deepLinkActivated += OnDeepLink;

        // ✅ ตรวจสอบว่ามี userId เก็บไว้ใน PlayerPrefs หรือไม่
        if (PlayerPrefs.HasKey("userId"))
        {
            Debug.Log("✅ Found userId in PlayerPrefs: " + PlayerPrefs.GetString("userId"));
        }
        else
        {
            Debug.LogWarning("⚠️ No userId found in PlayerPrefs");
        }
    }

    public void OnSignIn()
    {
        Debug.Log("🔹 Opening Google Login: " + authUrl);
        Application.OpenURL(authUrl);

        // เรียกเชื่อมต่อ WebSocket อีกครั้ง
        WebSocketManager webSocketManager = FindObjectOfType<WebSocketManager>();
        if (webSocketManager != null)
        {
            webSocketManager.ConnectWebSocket();
        }
    }

    public void OnLogout()
    {
        Debug.Log("🔹 Logging out...");
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.Save();
        StartCoroutine(LogoutAndSwitchScene());
    }

    IEnumerator LogoutAndSwitchScene()
    {
        yield return new WaitForSeconds(2);
        if (xrOriginObject != null)
        {
            xrOriginObject.transform.position = new Vector3(-206.8364f, -93f, 241.2679f);
        }

        if (simulatorObject1 != null) simulatorObject1.SetActive(true);
        if (simulatorObject2 != null) simulatorObject2.SetActive(true);
        combinedSaveLoadManager.ClearDigitalDevices();
        combinedSaveLoadManager.ClearCircuitDevices();
        managementCanvas.ShowLoginGoogle();
    }

    void OnDeepLink(string url)
    {
        Debug.Log("🔹 Received Deep Link: " + url); // ✅ Log URL ที่ได้รับจาก Google

        string token = ExtractTokenFromURL(url);

        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("✅ Extracted Token: " + token);
            PlayerPrefs.SetString("accessToken", token);
            PlayerPrefs.Save();
            StartCoroutine(SendUserDataToServer(token));
        }
        else
        {
            Debug.LogError("❌ Failed to extract token from URL");
        }
    }

    string ExtractTokenFromURL(string url)
    {
        try
        {
            Debug.Log("🔍 Checking URL: " + url); // ✅ Log URL ก่อนแยกค่า

            // ✅ ใช้ Regex ค้นหา access_token ทั้งจาก Query String และ Fragment
            Match match = Regex.Match(url, @"access_token=([^&]+)");
            if (match.Success)
            {
                string extractedToken = match.Groups[1].Value;
                Debug.Log("✅ Extracted access_token: " + extractedToken);
                return extractedToken;
            }

            Debug.LogWarning("⚠️ No access_token found in URL.");
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ Error extracting token: " + ex.Message);
        }

        return null;
    }


    IEnumerator SendUserDataToServer(string accessToken)
    {
        string jsonPayload = JsonUtility.ToJson(new { accessToken });
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Failed to send user data: " + request.error);
                UpdateStatusText("❌ Failed to send data: " + request.error);
            }
            else
            {
                Debug.Log("✅ Server response: " + request.downloadHandler.text);
                UserResponse userResponse = JsonUtility.FromJson<UserResponse>(request.downloadHandler.text);

                if (userResponse != null && !string.IsNullOrEmpty(userResponse.userId))
                {
                    PlayerPrefs.SetString("userId", userResponse.userId);
                    PlayerPrefs.Save();

                    Debug.Log("🔹 Stored userId in PlayerPrefs: " + userResponse.userId);
                    UpdateStatusText("✅ Login successful! Welcome " + userResponse.userId);
                    StartCoroutine(SendLogToServer(userResponse.userId));
                }
                else
                {
                    Debug.LogError("❌ Login failed: Invalid server response.");
                    UpdateStatusText("❌ Login failed: Invalid server response.");
                }
            }
        }
    }

    public IEnumerator SendLogToServer(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("❌ SendLogToServer() called with EMPTY userId!");
            yield break;
        }

        string logUrl = "https://smith11.ce.kmitl.ac.th/api/log/visitunity";

        LogData logData = new LogData { uid = userId, log_type = 0, practice_id = 0 };
        string jsonPayload = JsonUtility.ToJson(logData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(logUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Failed to send log data: {request.error}");
            }
            else
            {
                //Debug.Log($"✅ testttt : {userId}");

                Debug.Log($"✅ Log data sent successfully: {request.downloadHandler.text}");
            }
        }
    }

    [Serializable]
    public class LogData
    {
        public string uid;
        public int log_type;
        public int practice_id;
    }

    [Serializable]
    public class UserResponse
    {
        public string message;
        public string userId;
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    public void OpenFeedbackUser()
    {
        Debug.Log("🔹 Opening FeedBackUser in external browser");
        Application.OpenURL("https://cozy-druid-ddd4b6.netlify.app/feedbackuser");
    }
}
