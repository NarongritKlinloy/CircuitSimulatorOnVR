using UnityEngine;

public class ToggleSwitch : MonoBehaviour
{
    [Header("สถานะของ Switch (true = ON, false = OFF)")]
    public bool isOn = false; // ค่าเริ่มต้นของ Switch

    public OutputConnector output; // OutputConnector ที่ส่งค่าจากสวิตช์

    private void Start()
    {
        if (output != null)
        {
            output.isOn = isOn; // ตั้งค่าสถานะเริ่มต้น
            output.UpdateState(); // อัปเดตค่าทันที
        }
    }

    private void OnMouseDown()
    {
        isOn = !isOn; // สลับค่า (Toggle)
        if (output != null)
        {
            output.isOn = isOn;
            output.UpdateState(); // อัปเดตค่าทุกจุดที่เชื่อมต่อ
        }
        Debug.Log("Toggle Switch: " + (isOn ? "ON" : "OFF")); // แสดงสถานะใน Console
    }
}
