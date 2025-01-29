using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement; // ✅ เพิ่ม SceneManager

public class GoogleAuthen : MonoBehaviour
{
    public TMP_Text statusText;
    private string clientId = "382397535757-jlr6pk7k9ibtdja6mustqm1p426t4c1j.apps.googleusercontent.com";
    private string redirectUri = "https://ec0a-161-246-4-151.ngrok-free.app/";
    private string authUrl;
    private string serverUrl = "https://ec0a-161-246-4-151.ngrok-free.app/register";
    public string nextScene = "MainSence"; // ✅ ชื่อ Scene ที่ต้องการเปลี่ยน

    void Start()
    {
        // สร้าง URL สำหรับ Google OAuth
        authUrl = "https://accounts.google.com/o/oauth2/auth" +
                  "?client_id=" + clientId +
                  "&redirect_uri=" + redirectUri +
                  "&response_type=token" +
                  "&scope=email%20profile%20openid";

        // รองรับ Deep Linking
        Application.deepLinkActivated += OnDeepLink;
    }

    public void OnSignIn()
    {
        Debug.Log("Opening Google Login...");
        //SceneManager.LoadScene(nextScene);
        Application.OpenURL(authUrl);
    }

    void OnDeepLink(string url)
    {
        string token = ExtractTokenFromURL(url);
        Debug.Log("Received Token: " + token);

        // ส่ง Token ไปที่เซิร์ฟเวอร์
        StartCoroutine(SendUserDataToServer(token));
    }

    IEnumerator SendUserDataToServer(string idToken)
    {
        WWWForm form = new WWWForm();
        form.AddField("idToken", idToken);

        using (UnityWebRequest request = UnityWebRequest.Post(serverUrl, form))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to send user data: " + request.error);
                UpdateStatusText("Failed to send data: " + request.error);
            }
            else
            {
                Debug.Log("User data sent successfully: " + request.downloadHandler.text);
                UpdateStatusText("Login successful!");

                // ✅ เปลี่ยน Scene เมื่อ Login สำเร็จ
                SceneManager.LoadScene(nextScene);
            }
        }
    }

    string ExtractTokenFromURL(string url)
    {
        Uri uri = new Uri(url);
        string token = uri.Query.Split('=')[1];
        return token;
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
