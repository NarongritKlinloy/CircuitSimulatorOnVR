using UnityEngine;

public class ToggleSwitch : MonoBehaviour
{
    [Header("สถานะของ Switch (true = ON, false = OFF)")]
    public bool isOn = false; // ค่าเริ่มต้นของ Switch
    public OutputConnector output; // OutputConnector ที่ส่งค่าจากสวิตช์
    public GameObject pivot; // วัตถุที่ใช้หมุน (เช่น Rocker)

    private void Start()
    {
        // ถ้ายังไม่มี OutputConnector ให้สร้างขึ้นใหม่
        if (output == null)
        {
            output = gameObject.AddComponent<OutputConnector>(); // สร้างใหม่สำหรับแต่ละ ToggleSwitch
        }

        if (output != null)
        {
            output.isOn = isOn; // ตั้งค่าสถานะเริ่มต้น
            output.UpdateState(); // อัปเดตค่าทันที
        }

        UpdatePivotRotation(); // อัปเดตการหมุนของ pivot ทันทีที่เริ่มต้น
    }


    private void OnMouseDown()
    {
        Toggle(); // เรียกใช้ Toggle()
    }

    public void Toggle()
    {
        isOn = !isOn; // สลับค่า (Toggle)

        if (output != null)
        {
            output.isOn = isOn;
            output.UpdateState(); // อัปเดตค่าทุกจุดที่เชื่อมต่อ
        }

        UpdatePivotRotation(); // อัปเดตการหมุนของ pivot ทันที
        //Debug.Log("Toggle Switch: " + (isOn ? "ON" : "OFF")); // แสดงสถานะใน Console
    }

    private void UpdatePivotRotation()
    {
        if (pivot != null)
        {
            var rotation = pivot.transform.localEulerAngles;
            rotation.y = isOn ? 15f : -15f; // หมุน Rocker ตามสถานะ
            pivot.transform.localEulerAngles = rotation;
        }
    }
}
