using UnityEngine;
using System;
using System.Threading;
using System.Text;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;

public class WebSocketManager : MonoBehaviour
{
    private ClientWebSocket ws;
    public TMP_Text statusText;
    public string nextScene = "MainScene";

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

                if (message.Contains("accessToken"))
                {
                    statusText.text = "✅ Login Successful!";
                    await Task.Delay(1000);
                    SceneManager.LoadScene(nextScene);
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
