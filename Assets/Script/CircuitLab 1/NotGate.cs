using UnityEngine;
using TMPro;  // ถ้าจะใช้ TextMeshPro

public class NotGate : CircuitComponent, INotGate
{
    // (ตัวอย่าง) อ็อบเจกต์ Label สำหรับโชว์ค่าแรงดัน Output
    public GameObject labelOutput;
    public TMP_Text labelOutputText;

    protected override void Start()
    {
        base.Start();
        // ทำอะไรเพิ่มเติมถ้าจำเป็น
    }

    protected override void Update()
    {
        base.Update();

        // หากต้องการให้ Label แสดงหรือไม่แสดงตาม ShowLabels
        if (Lab != null && labelOutput != null)
        {
            labelOutput.SetActive(IsActive && Lab.showLabels);
        }
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        base.SetActive(isActive, isForward);

        // ตัวอย่าง: หมุน Label ให้ตรงกับทิศการวาง หากคุณต้องการจัดตำแหน่งตาม Direction
        if (labelOutput != null)
        {
            RotateLabel(labelOutput, LabelAlignment.Top);
        }
    }

    public override void SetVoltage(double voltage)
    {
        base.SetVoltage(voltage);
        // ‘Voltage’ ในที่นี้จะถือเป็น “แรงดันที่ขา Input” ของ NOT Gate

        // ตัวอย่าง: อาจจะอัปเดต LabelOutput ให้แสดงผล “แรงดันที่ขา Output” (เชิงอะนาล็อก)
        // แต่เพราะมันเป็น NOT (Inverter) ที่ไม่มีไฟเลี้ยงจริง 
        // เราจะปล่อยให้ SpiceSharp คำนวณ แล้วส่งค่ากลับมาในภายหลังผ่าน ExportSimulationData
        if (labelOutputText != null)
        {
            labelOutputText.text = $"Vout = (inverted)";
        }
    }

    public override void SetCurrent(double current)
    {
        base.SetCurrent(current);
        // สามารถใช้ current ไปทำอะไรได้ตามต้องการ
    }
}
