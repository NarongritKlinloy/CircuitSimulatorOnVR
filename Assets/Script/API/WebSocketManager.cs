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
}

public class WebSocketManager : MonoBehaviour
{
    private ClientWebSocket ws;
    public TMP_Text statusText;

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

                // พยายามแปลง JSON
                WebSocketMessage wsData = null;
                try
                {
                    wsData = JsonUtility.FromJson<WebSocketMessage>(message);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Could not parse WebSocket JSON: " + ex.Message);
                }

                // ถ้า parse สำเร็จ และมี userId
                if (wsData != null && !string.IsNullOrEmpty(wsData.userId))
                {
                    // เก็บ userId ลง PlayerPrefs
                    PlayerPrefs.SetString("userId", wsData.userId);
                    PlayerPrefs.Save();
                    Debug.Log("🟢 WebSocket set userId to: " + wsData.userId);

                    if (statusText != null)
                        statusText.text = "✅ Login Successful via WebSocket!";

                    // ถ้าต้องการเรียก ManagementCanvas ก็ทำได้
                    // ตัวอย่าง:
                    ManagementCanvas managementCanvas = FindObjectOfType<ManagementCanvas>();
                    if (managementCanvas != null)
                    {
                        managementCanvas.ShowMainMenu();
                        Debug.Log("ShowMainMenu() called from WebSocketManager after userId set.");
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
