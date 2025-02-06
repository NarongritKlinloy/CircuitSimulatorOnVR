using UnityEngine;
using System.Collections;

public class Clock : MonoBehaviour
{
    [Header("ตั้งค่าความถี่ของ Clock (วินาทีต่อรอบ)")]
    public float interval = 1.0f; // Default 1 วินาทีต่อรอบ

    [Header("สถานะของ Clock (true/false)")]
    public bool isOn = false; // ค่าปัจจุบันของ Clock

    public OutputConnector output; // Output ที่เชื่อมต่อกับวงจร

    private void Start()
    {
        if (output != null)
        {
            output.isOn = isOn; // ตั้งค่าสถานะเริ่มต้น
            output.UpdateState(); // อัปเดตค่าทันที
        }
        StartCoroutine(ToggleClock()); // เริ่มต้น Clock
    }

    private IEnumerator ToggleClock()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval); // รอเวลาตาม Interval
            isOn = !isOn; // Toggle ค่า true/false
            if (output != null)
            {
                output.isOn = isOn;
                output.UpdateState(); // ส่งค่าออกไปยังวงจร
            }
            Debug.Log("Clock Pulse: " + (isOn ? "ON" : "OFF")); // แสดงสถานะ Clock ใน Console
        }
    }
}
