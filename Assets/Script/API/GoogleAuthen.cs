using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class GoogleAuthen : MonoBehaviour
{
    public TMP_Text statusText;
    private string clientId = "382397535757-jlr6pk7k9ibtdja6mustqm1p426t4c1j.apps.googleusercontent.com";
    private string redirectUri = "http://localhost:5000/callback";
    private string authUrl;
    private string serverUrl = "http://localhost:5000/register";
    // ไม่ต้องเปลี่ยน sceneอีกต่อไป
    public string loginScene = "LoginScene"; // สำหรับ logout

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
        // ไม่เปลี่ยน scene แต่สามารถทำการ reset UI ได้ตามต้องการ
    }
    public void OnLogout()
    {
        Debug.Log("🔹 Logging out...");
        PlayerPrefs.DeleteKey("accessToken");
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.Save();
        StartCoroutine(LogoutAndSwitchScene());
    }

    IEnumerator LogoutAndSwitchScene()
    {
        yield return new WaitForSeconds(2);
        // ไม่เปลี่ยน scene แต่สามารถทำการ reset UI ได้ตามต้องการ
    }

    void OnDeepLink(string url)
    {
        Debug.Log("🔹 Received Deep Link: " + url);
        string token = ExtractTokenFromURL(url);
        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("✅ Extracted Token: " + token);
            // เก็บ accessToken ชั่วคราว (ถ้าอยากใช้)
            PlayerPrefs.SetString("accessToken", token);
            PlayerPrefs.Save();
            // เรียก /register เพื่อให้ server ลงทะเบียน/อัปเดต user
            StartCoroutine(SendUserDataToServer(token));
        }
        else
        {
            Debug.LogError("❌ Failed to extract token from URL");
            UpdateStatusText("❌ Token extraction failed.");
        }
    }

    // สำหรับจำลอง deep link ใน Editor
    void SimulateDeepLink(string url)
    {
        Debug.Log("Simulating deep link: " + url);
        OnDeepLink(url);
    }

    // ส่ง accessToken ไปยังเซิร์ฟเวอร์เป็น JSON payload (เพื่อ register user)
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

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
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
                    // เก็บ userId จาก HTTP response ไว้ใน PlayerPrefs ได้อีกทาง (เผื่อ fallback)
                    PlayerPrefs.SetString("userId", userResponse.userId);
                    PlayerPrefs.Save();
                    Debug.Log("🔹 Stored userId in PlayerPrefs: " + PlayerPrefs.GetString("userId"));
                    UpdateStatusText("✅ Login successful! Welcome " + userResponse.userId);
                }
                else
                {
                    Debug.LogError("❌ Login failed: Invalid server response.");
                    UpdateStatusText("❌ Login failed: Invalid server response.");
                }
            }
        }
    }

    [Serializable]
    public class UserResponse
    {
        public string message;
        public string userId;
    }

    // ดึง accessToken จาก URL
    string ExtractTokenFromURL(string url)
    {
        try
        {
            Uri uri = new Uri(url);
            // parse fragment (#)
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
            // parse query (?)
            if (!string.IsNullOrEmpty(uri.Query))
            {
                string query = uri.Query.TrimStart('?');
                var queryParams = query.Split('&');
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
}
