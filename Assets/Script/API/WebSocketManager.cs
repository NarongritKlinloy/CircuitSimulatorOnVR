using UnityEngine;
using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TMPro;

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
    public GoogleAuthen googleAuthen; 

    // ฟังก์ชันไว้ให้เรียกตอน OnSignIn หรือจุดที่เราต้องการเชื่อมต่อใหม่
    public async void ConnectWebSocket()
    {
        // ถ้าเปิด WebSocket ค้างไว้แล้ว ให้ปิดก่อน
        if (ws != null && ws.State == WebSocketState.Open)
        {
            Debug.Log("🔹 WebSocket is already open. Closing existing connection...");
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnecting", CancellationToken.None);
        }

        ws = new ClientWebSocket();
        try
        {
            Debug.Log("🌐 Connecting to WebSocket...");
            await ws.ConnectAsync(new Uri("ws://smith11.ce.kmitl.ac.th:8282"), CancellationToken.None);
            Debug.Log("✅ Connected to WebSocket Server");

            // เริ่มรับข้อความจากเซิร์ฟเวอร์
            _ = ListenForMessages();
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

                    // กรณีได้รับ error จาก WebSocket
                    if (!string.IsNullOrEmpty(wsData.error))
                    {
                        Debug.LogError("❌ WebSocket received error: " + wsData.error);

                        // เรียก UI แจ้งเตือน
                        try
                        {
                            ManagementCanvas managementCanvas = FindObjectOfType<ManagementCanvas>();
                            if (managementCanvas != null)
                            {
                                managementCanvas.ShowUiNotifyErrorLogin();
                                Debug.Log("🔹 ShowUiNotifyErrorLogin() called.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("⚠️ ManagementCanvas not found: " + ex.Message);
                        }
                    }
                    // กรณีล็อกอินสำเร็จและได้ userId กลับมา
                    else if (!string.IsNullOrEmpty(wsData.userId))
                    {
                        Debug.Log("✅ User logged in via WebSocket: " + wsData.userId);

                        // อัปเดต UI ข้อความสถานะ
                        if (statusText != null)
                        {
                            statusText.text = "Login Successful via WebSocket!";
                        }

                        // อัปเดต ManagementCanvas เพื่อเก็บ userId และแสดง UI
                        try
                        {
                            ManagementCanvas managementCanvas = FindObjectOfType<ManagementCanvas>();
                            if (managementCanvas != null)
                            {
                                // อัปเดต userId ลงในตัว ManagementCanvas
                                managementCanvas.UpdateUserId(wsData.userId);

                                // แสดง Pop-Up แจ้งว่าล็อกอินสำเร็จ
                                managementCanvas.ShowUiNotifyLogin();
                                Debug.Log("🔹 ShowUiNotifyLogin() called.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("⚠️ ManagementCanvas not found: " + ex.Message);
                        }

                        // ส่ง Log กลับไปยังเซิร์ฟเวอร์
                        if (googleAuthen != null)
                        {
                            Debug.Log("📌 Calling SendLogToServer() from WebSocketManager...");

                            // googleAuthen จะจัดการส่ง log (พร้อม userId) ไปให้เซิร์ฟเวอร์
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

            // ป้องกัน CPU overload ใน while loop
            await Task.Delay(10);
        }
    }
}
