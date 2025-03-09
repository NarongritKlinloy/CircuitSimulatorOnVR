using UnityEngine;
using System;
using System.Threading;
using System.Text;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TMPro;

// คลาสสำหรับแปลง JSON ที่ส่งมาจาก WebSocket
[Serializable]
public class WebSocketMessage
{
    public string accessToken;
    public string userId;
    public string error;
}

public class WebSocketManager : MonoBehaviour
{
    private ClientWebSocket ws;
    public TMP_Text statusText;
    public GoogleAuthen googleAuthen; // เพิ่ม GoogleAuthen เพื่อเรียกใช้ SendLogToServer()

    async void Start()
    {
        ws = new ClientWebSocket();
        try
        {
            await ws.ConnectAsync(new Uri("ws://localhost:8080"), CancellationToken.None);
            Debug.Log("✅ Connected to WebSocket Server");
            await ListenForMessages();
        }
        catch (Exception e)
        {
            Debug.LogError("❌ WebSocket Error: " + e.Message);
        }
    }

    private async Task ListenForMessages()
    {
        var buffer = new byte[1024];
        while (ws.State == WebSocketState.Open)
        {
            try
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log("📡 Received from Server: " + message);

                WebSocketMessage wsData = null;
                try
                {
                    wsData = JsonUtility.FromJson<WebSocketMessage>(message);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("❌ Could not parse WebSocket JSON: " + ex.Message);
                }

                if (wsData != null)
                {
                    Debug.Log("✅ Parsed Data: userId=" + wsData.userId + ", error=" + (wsData.error ?? "null"));

                    if (!string.IsNullOrEmpty(wsData.error))
                    {
                        Debug.LogError("❌ WebSocket received error: " + wsData.error);
                        ManagementCanvas managementCanvas = FindObjectOfType<ManagementCanvas>();
                        if (managementCanvas != null)
                        {
                            managementCanvas.ShowUiNotifyErrorLogin();
                            Debug.Log("🔹 ShowUiNotifyErrorLogin() called.");
                        }
                    }
                    else if (!string.IsNullOrEmpty(wsData.userId))
                    {
                        PlayerPrefs.SetString("userId", wsData.userId);
                        PlayerPrefs.Save();
                        Debug.Log("✅ User logged in via WebSocket: " + wsData.userId);

                        if (statusText != null)
                            statusText.text = "Login Successful via WebSocket!";

                        ManagementCanvas managementCanvas = FindObjectOfType<ManagementCanvas>();
                        if (managementCanvas != null)
                        {
                            managementCanvas.ShowUiNotifyLogin();
                            Debug.Log("🔹 ShowUiNotifyLogin() called.");
                        }

                        // ✅ เรียก GoogleAuthen เพื่อส่ง Log
                        if (googleAuthen != null)
                        {
                            Debug.Log("📌 Calling SendLogToServer() from WebSocketManager...");
                            googleAuthen.StartCoroutine(googleAuthen.SendLogToServer(wsData.userId));
                        }
                        else
                        {
                            Debug.LogError("❌ googleAuthen is NULL, cannot send log.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("❌ Error receiving WebSocket message: " + e.Message);
                break;
            }
        }
    }
}
