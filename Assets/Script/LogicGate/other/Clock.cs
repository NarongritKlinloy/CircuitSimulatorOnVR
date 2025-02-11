using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Clock : MonoBehaviour
{
    [Header("ตั้งค่าความถี่ของ Clock (วินาทีต่อรอบ)")]
    public float interval = 1.0f; // Default 1 วินาทีต่อรอบ

    [Header("สถานะของ Clock (true/false)")]
    public bool isOn = false; // ค่าปัจจุบันของ Clock

    public OutputConnector output; // Output ที่เชื่อมต่อกับวงจร

    [Header("UI สำหรับการตั้งค่า")]
    public GameObject canvas; // UI Canvas ที่ใช้กรอกค่า
    public Slider intervalSlider; // Slider สำหรับปรับค่า interval
    public TMP_Text displayText; // แสดงค่าที่ตั้งไว้

    private Coroutine clockCoroutine; // เก็บ Coroutine ของ Clock
    private bool isCanvasOpen = false; // เช็คสถานะของ Canvas

    private void Start()
    {
        if (canvas != null)
        {
            canvas.SetActive(false); // ปิด Canvas ตอนเริ่ม
        }

        if (intervalSlider != null)
        {
            intervalSlider.minValue = 0.1f;  // กำหนดค่าน้อยสุดของ Slider
            intervalSlider.maxValue = 5.0f;  // กำหนดค่าสูงสุดของ Slider
            intervalSlider.value = interval; // ตั้งค่าเริ่มต้น
            intervalSlider.onValueChanged.AddListener(delegate { UpdateIntervalFromSlider(); });
        }

        if (displayText != null)
        {
            displayText.text = "1.0s"; // ตั้งค่าเริ่มต้นของ DisplayText เป็น "1.0s"
        }

        if (output != null)
        {
            output.isOn = isOn;
            output.UpdateState(); // อัปเดตค่าทันที
        }

        StartClock();
    }

    private void StartClock()
    {
        if (clockCoroutine != null)
        {
            StopCoroutine(clockCoroutine);
        }
        clockCoroutine = StartCoroutine(ToggleClock());
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
            //Debug.Log("Clock Pulse: " + (isOn ? "ON" : "OFF")); // แสดงสถานะ Clock ใน Console
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Pinch"))
        {
            ToggleCanvas(); // เรียกฟังก์ชันเปิด/ปิด Canvas
        }
    }

    private void ToggleCanvas()
    {
        isCanvasOpen = !isCanvasOpen; // สลับสถานะของ Canvas
        if (canvas != null)
        {
            canvas.SetActive(isCanvasOpen); // เปิด/ปิด Canvas ตามสถานะ
            if (isCanvasOpen && intervalSlider != null)
            {
                intervalSlider.value = interval; // ตั้งค่า Slider ให้ตรงกับค่า interval ปัจจุบัน
            }
        }
    }

    private void UpdateIntervalFromSlider()
    {
        interval = intervalSlider.value;
        displayText.text = interval.ToString("0.0") + "s"; // อัปเดตค่าแสดงผล
        StartClock(); // รีสตาร์ท Clock
    }
}
