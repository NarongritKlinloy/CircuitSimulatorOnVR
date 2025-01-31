using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GoogleAuthen : MonoBehaviour
{
    public TMP_Text statusText;
    private string clientId = "382397535757-jlr6pk7k9ibtdja6mustqm1p426t4c1j.apps.googleusercontent.com";
    private string redirectUri = "http://localhost:3000/callback";
    private string authUrl;
    private string serverUrl = "http://localhost:3000/register";
    public string nextScene = "MainScene";
    public string loginScene = "LoginScene"; // ✅ เปลี่ยนกลับไปหน้าล็อกอิน

    void Start()
    {
        authUrl = "https://accounts.google.com/o/oauth2/auth" +
                  "?client_id=" + clientId +
                  "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                  "&response_type=token" +
                  "&scope=email%20profile%20openid" +
                  "&prompt=select_account"; // ✅ บังคับให้เลือกบัญชีใหม่ทุกครั้ง
        ;

        Application.deepLinkActivated += OnDeepLink;
    }

    public void OnSignIn()
    {
        Debug.Log("🔹 Opening Google Login: " + authUrl);
        Application.OpenURL(authUrl);
    }

    public void OnLogout()
    {
        Debug.Log("🔹 Logging out...");

        // ✅ ลบ Token ที่ถูกเก็บไว้
        PlayerPrefs.DeleteKey("accessToken");
        PlayerPrefs.Save();

        // ✅ เปิดหน้า Google Logout เพื่อบังคับออกจากระบบ
        //Application.OpenURL("https://accounts.google.com/logout");

        // ✅ ดีเลย์ 2 วินาทีก่อนเปลี่ยนกลับไปหน้าล็อกอิน
        StartCoroutine(LogoutAndSwitchScene());
    }

    IEnumerator LogoutAndSwitchScene()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(loginScene);
    }


    void OnDeepLink(string url)
    {
        Debug.Log("🔹 Received Deep Link: " + url);
        string token = ExtractTokenFromURL(url);

        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("✅ Extracted Token: " + token);
            PlayerPrefs.SetString("accessToken", token); // ✅ เก็บ Token ไว้
            PlayerPrefs.Save();
            StartCoroutine(SendUserDataToServer(token));
        }
        else
        {
            Debug.LogError("❌ Failed to extract token from URL");
            UpdateStatusText("❌ Token extraction failed.");
        }
    }

    IEnumerator SendUserDataToServer(string accessToken)
    {
        WWWForm form = new WWWForm();
        form.AddField("accessToken", accessToken);

        using (UnityWebRequest request = UnityWebRequest.Post(serverUrl, form))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Failed to send user data: " + request.error);
                UpdateStatusText("❌ Failed to send data: " + request.error);
            }
            else
            {
                Debug.Log("✅ User data sent successfully: " + request.downloadHandler.text);
                UpdateStatusText("✅ Login successful!");
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene(nextScene);
            }
        }
    }

    string ExtractTokenFromURL(string url)
    {
        Uri uri = new Uri(url);
        string fragment = uri.Fragment;

        if (fragment.StartsWith("#"))
        {
            fragment = fragment.Substring(1);
        }

        var queryParams = fragment.Split('&');
        foreach (string param in queryParams)
        {
            string[] keyValue = param.Split('=');
            if (keyValue.Length == 2 && keyValue[0] == "access_token")
            {
                return keyValue[1];
            }
        }
        return null;
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
