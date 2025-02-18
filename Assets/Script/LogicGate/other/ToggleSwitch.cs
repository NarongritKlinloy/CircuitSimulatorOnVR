using UnityEngine;

public class ToggleSwitch : MonoBehaviour
{
    [Header("สถานะของ Switch (true = ON, false = OFF)")]
    public bool isOn = false; // ค่าเริ่มต้นของ Switch
    public OutputConnector output; // OutputConnector ที่ส่งค่าจากสวิตช์
    public GameObject pivot; // วัตถุที่ใช้หมุน (เช่น Rocker)

    private void Start()
    {
        if (output == null)
        {
            output = gameObject.AddComponent<OutputConnector>();
        }

        if (output != null)
        {
            output.isOn = isOn;
            output.UpdateState();

            // ตั้งชื่อให้ Output
            output.gameObject.name = $"{gameObject.name}_OUT";
        }

        UpdatePivotRotation();
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
            //Debug.Log($"🔄 ToggleSwitch {gameObject.name} กำลังส่งค่า {isOn} ไปยัง Output {output.gameObject.name}");
            output.UpdateState(); // อัปเดตค่าทุกจุดที่เชื่อมต่อ
        }
        else
        {
            Debug.Log($"⚠️ ToggleSwitch {gameObject.name} ไม่มี Output ที่เชื่อมต่อ");
        }

        UpdatePivotRotation(); // อัปเดตการหมุนของ pivot ทันที
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
