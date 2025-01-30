using UnityEngine;
using TMPro;
using System;

public class GoogleAuthen : MonoBehaviour
{
    public TMP_Text statusText;
    private string clientId = "382397535757-jlr6pk7k9ibtdja6mustqm1p426t4c1j.apps.googleusercontent.com";
    private string redirectUri = "http://localhost:3000/callback";
    private string authUrl;

    void Start()
    {
        authUrl = "https://accounts.google.com/o/oauth2/auth" +
                  "?client_id=" + clientId +
                  "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                  "&response_type=token" +
                  "&scope=email%20profile%20openid";

        Application.deepLinkActivated += OnDeepLink;
    }

    public void OnSignIn()
    {
        Debug.Log("🔹 Opening Google Login: " + authUrl);
        Application.OpenURL(authUrl);
    }

    void OnDeepLink(string url)
    {
        Debug.Log("🔹 Received Deep Link: " + url);
    }
}
