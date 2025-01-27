using UnityEngine;
using TMPro;  // ถ้าใช้ TextMeshPro

public class Diode : CircuitComponent, IDiode
{
    // --------------------------------------------------
    // ส่วนของการแสดงผลใน Inspector (Unity)
    // --------------------------------------------------

    [Header("Diode Label Objects (Optional)")]
    public GameObject labelVoltage;
    public TMP_Text labelVoltageText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;

    [Header("Model Name (ใช้ใน SpiceSharp)")]
    [Tooltip("ชื่อโมเดลที่ไดโอดจะอ้างอิงใน SpiceSharp เช่น DDefault")]
    public string diodeModelName = "DDefault";
    
    /// <summary>
    /// บังคับใช้จาก IDiode
    /// </summary>
    public string DiodeModelName => diodeModelName;
    
    // --------------------------------------------------
    // ฟังก์ชันหลัก
    // --------------------------------------------------
    
    protected override void Start()
    {
        base.Start();
        // หากต้องการทำอะไรเพิ่มเติมตอนเริ่มต้น
    }

    protected override void Update()
    {
        base.Update();

        if (Lab != null)
        {
            // ควบคุมการแสดง label ว่าจะโชว์หรือไม่
            // (อ้างอิงจากโค้ดใน Flute.cs)
            bool show = IsActive && Lab.showLabels;

            if (labelVoltage != null) labelVoltage.SetActive(show);
            if (labelCurrent != null) labelCurrent.SetActive(show);
        }
    }

    /// <summary>
    /// เมื่อ SpiceSharp จำลองค่าออกมา จะเรียก SetVoltage / SetCurrent
    /// เราสามารถนำค่าที่ได้ไปอัพเดตใน Text ได้
    /// </summary>
    public override void SetVoltage(double voltage)
    {
        base.SetVoltage(voltage); // เก็บลง field ของ CircuitComponent

        if (labelVoltageText != null)
        {
            // สมมติแสดงหน่วยเป็น V (โวลต์)
            labelVoltageText.text = $"{voltage:0.###} V";
        }
    }

    public override void SetCurrent(double current)
    {
        base.SetCurrent(current); // เก็บลง field ของ CircuitComponent

        if (labelCurrentText != null)
        {
            // สมมติแสดงหน่วยเป็น mA
            labelCurrentText.text = $"{(current * 1000.0):0.###} mA";
        }
    }

    /// <summary>
    /// ถูกเรียกจาก CircuitLab เพื่อบอกว่าไดโอดกำลัง Active หรือไม่
    /// และกระแสไหลทิศทางไหน (isForward)
    /// </summary>
    public override void SetActive(bool isActive, bool isForward)
    {
        base.SetActive(isActive, isForward);

        // หมุน label ให้หันถูกทิศ (เหมือนใน Flute.cs) ก็ได้
        if (labelVoltage != null)
            RotateLabel(labelVoltage, LabelAlignment.Top);
        
        if (labelCurrent != null)
            RotateLabel(labelCurrent, LabelAlignment.Bottom);
    }

    /// <summary>
    /// ถ้ามีการ Reset เองใน class ลูก
    /// </summary>
    protected override void Reset()
    {
        base.Reset();
        // รีเซ็ตค่าอะไรก็ว่าไป (ถ้าต้องการ)
    }
}
