using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class GoogleAuthen : MonoBehaviour
{
    public TMP_Text statusText;
    private string clientId = "536241701089-ej2lkeskgljs17a9dp6d3eeorfhb2f2e.apps.googleusercontent.com";
    private string redirectUri = "https://smith11.ce.kmitl.ac.th/callback";
    private string authUrl;
    private string serverUrl = "https://smith11.ce.kmitl.ac.th/register";

    public string loginScene = "LoginScene"; // ✅ ไม่ต้องเปลี่ยน Scene
    public ManagementCanvas managementCanvas;

    [Header("XR Origin")]
    [Tooltip("ลาก GameObject ที่เป็น XR Origin (หรือ XR Rig) มาใส่")]
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
    }

    public void OnSignIn()
    {
        StartCoroutine(Wiat());
        Debug.Log("🔹 Opening Google Login: " + authUrl);
        Application.OpenURL(authUrl);
    }

    IEnumerator Wiat()
    {
        yield return new WaitForSeconds(2);
    }

    public void OnLogout()
    {
        Debug.Log("🔹 Logging out...");
        PlayerPrefs.DeleteKey("userId"); // ✅ ลบ userId ออกจาก PlayerPrefs
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

        managementCanvas.ShowLoginGoogle();
    }

    void OnDeepLink(string url)
    {
        Debug.Log("🔹 Received Deep Link: " + url);
        string token = ExtractTokenFromURL(url);

        if (!string.IsNullOrEmpty(token))
        {   
            ReturnToUnityApp(); // 🔄 กลับเข้าเกมด้วย Android Intent
            Debug.Log("✅ Extracted Token: " + token);

            StartCoroutine(SendUserDataToServer(token));
        }
        else
        {
            Debug.LogError("❌ Failed to extract token from URL");
            UpdateStatusText("❌ Token extraction failed.");
        }
    }

    // ✅ ฟังก์ชันนี้ช่วยปิด Browser และสลับกลับเกม
    void ReturnToUnityApp()
    {
#if UNITY_ANDROID
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            string packageName = currentActivity.Call<string>("getPackageName");
            AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);

            if (launchIntent != null)
            {
                currentActivity.Call("startActivity", launchIntent);
                Debug.Log("🔄 Returning to Unity App...");
            }
            else
            {
                Debug.LogError("❌ Could not create launch intent for package: " + packageName);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error returning to Unity App: " + e.Message);
        }
#endif
    }

    IEnumerator SendUserDataToServer(string accessToken)
    {
        string jsonPayload = JsonUtility.ToJson(new { accessToken = accessToken });
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
                    // ✅ เปลี่ยนเป็นใช้ PlayerPrefs แทน
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

        LogData logData = new LogData
        {
            uid = userId,
            log_type = 0,
            practice_id = 0
        };

        string jsonPayload = JsonUtility.ToJson(logData);
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

    string ExtractTokenFromURL(string url)
    {
        try
        {
            Uri uri = new Uri(url);
            if (!string.IsNullOrEmpty(uri.Fragment))
            {
                string fragment = uri.Fragment.StartsWith("#") ? uri.Fragment.Substring(1) : uri.Fragment;
                var queryParams = fragment.Split('&');
                foreach (string param in queryParams)
                {
                    string[] keyValue = param.Split('=');
                    if (keyValue.Length == 2 && keyValue[0] == "access_token")
                    {
                        return keyValue[1];
                    }
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing URL: " + ex.Message);
            return null;
        }
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    public void OpenGoogle()
    {
        Debug.Log("🔹 Opening Google in external browser");
        Application.OpenURL("https://www.google.com");
    }
}
