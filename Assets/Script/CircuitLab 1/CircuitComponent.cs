using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum สำหรับกำหนดตำแหน่งของ Label (ด้านบน, ด้านล่าง, ตรงกลาง)
public enum LabelAlignment { Top, Bottom, Center };

// คลาสแม่สำหรับส่วนประกอบวงจร (Circuit Components)
public class CircuitComponent : MonoBehaviour
{
    // สมาชิกที่สามารถตั้งค่าได้ใน Unity Object Inspector
    public CircuitLab Lab; // ตัวเชื่อมต่อกับห้องทดลองวงจร

    // สถานะของส่วนประกอบ
    public bool IsPlaced { get; protected set; } // ระบุว่าวางลงในวงจรแล้วหรือยัง
    public bool IsHeld { get; protected set; } // ระบุว่าถูกถืออยู่หรือไม่
    public bool IsClone { get; set; } // ระบุว่าเป็นสำเนาหรือไม่
    public bool IsClosed { get; protected set; } // ระบุว่าสวิตช์ปิดอยู่หรือไม่
    public bool IsTesting { get; protected set; }

    // คุณสมบัติภายใน
    public Point StartingPeg { get; set; } // จุดเริ่มต้นของส่วนประกอบ
    protected Direction Direction { get; set; } // ทิศทางของส่วนประกอบ
    protected bool IsActive { get; set; } // ระบุว่าส่วนประกอบทำงานอยู่หรือไม่
    protected bool IsForward { get; set; } // ระบุทิศทางกระแส
    protected bool IsShortCircuit { get; set; } // ระบุว่าส่วนประกอบลัดวงจรหรือไม่
    protected double Voltage { get; set; } // ค่าแรงดันไฟฟ้า
    protected double Current { get; set; } // ค่ากระแสไฟฟ้า
    public GameObject GameObject { get; private set; }
    public object Component { get; private set; }
    public Point StartingPoint { get; private set; }
    public Point End { get; set; }
    // ค่าคงที่สำหรับกระแสที่สำคัญและการจัดตำแหน่ง Label
    const double SignificantCurrent = 0.0000001; // กระแสไฟขั้นต่ำที่ถือว่าสำคัญ
    const float LabelOffset = 0.022f; // ระยะห่างสำหรับจัดตำแหน่ง Label

    // คอนสตรัคเตอร์ตั้งค่าเริ่มต้น
    protected CircuitComponent()
    {
        IsPlaced = false;
        IsHeld = false;
        IsClone = false;
        IsClosed = true;
        IsActive = false;
        IsForward = true;
        IsTesting = false;
        IsShortCircuit = false;
        Voltage = 0f;
        Current = 0f;

    }

    // ฟังก์ชันเริ่มต้นและอัปเดต (สามารถ override ในคลาสลูกได้)
    protected virtual void Start()
    {
        StartingPeg = new Point(0, 0); // กำหนดค่าเริ่มต้น
        End = new Point(0, 0);         // กำหนดค่าเริ่มต้น
    }
    public virtual void SetEnd(Point end)
    {
        End = end;
    }
    protected virtual void Update() { }
    public virtual float GetValue()
    {
        return 0f; // ค่า default
    }


    // ฟังก์ชันวางส่วนประกอบในวงจร
    public void Place(Point start, Direction dir)
    {
        IsPlaced = true;
        StartingPeg = start;
        Direction = dir;
    }

    // ฟังก์ชันกำหนดสถานะการทำงาน
    public virtual void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;
        IsForward = isForward;
    }

    // ฟังก์ชันกำหนดสถานะลัดวงจร
    public virtual void SetShortCircuit(bool isShortCircuit, bool isForward)
    {
        IsShortCircuit = isShortCircuit;
        IsForward = isForward;
    }

    // ฟังก์ชันคืนค่าแรงดันไฟฟ้า
    public double GetVoltage()
    {
        return Voltage;
    }
    public virtual void SetValue(float value)
    {
        Debug.Log($"Setting value {value} for component: {name}");
    }

    // ฟังก์ชันตั้งค่าแรงดันไฟฟ้า
    public virtual void SetVoltage(double voltage)
    {
        Voltage = voltage;
    }

    // ฟังก์ชันตั้งค่ากระแสไฟฟ้า
    public virtual void SetCurrent(double current)
    {
        Current = current;
    }

    // ตรวจสอบว่ากระแสมีค่ามากพอหรือไม่
    protected bool IsCurrentSignificant()
    {
        return (Current > SignificantCurrent);
    }

    // ฟังก์ชันสลับสถานะ (สำหรับส่วนประกอบประเภท Binary เช่น Switch)
    public virtual void Toggle()
    {
    }

    // ฟังก์ชันปรับพฤติกรรมส่วนประกอบ
    public virtual void Adjust()
    {
    }

    // ฟังก์ชันรีเซ็ตสถานะส่วนประกอบ
    protected virtual void Reset()
    {
    }

    // ฟังก์ชันเมื่อเลือกส่วนประกอบ
    public virtual void SelectEntered()
    {
        IsHeld = true;

        // เปิดการทำงานของ Collider เพื่อให้สามารถวางใหม่ได้
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;

        if (IsPlaced)
        {
            Lab.RemoveComponent(this.gameObject, StartingPeg); // ลบออกจากวงจร
            IsPlaced = false;
        }

        // รีเซ็ตสถานะของส่วนประกอบ
        Reset();
    }

    // ฟังก์ชันเมื่อปล่อยส่วนประกอบ
    public virtual void SelectExited()
    {
        IsHeld = false;

        // เปิดการทำงานของแรงโน้มถ่วงเมื่อปล่อยส่วนประกอบ
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
    }

    // เล่นเสียงด้วย AudioSource พร้อมตั้งค่าหน่วงเวลา
    protected IEnumerator PlaySound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        source.Stop();
        source.Play();
    }

    // จัดตำแหน่งและหมุน Label ตามตำแหน่งและทิศทาง
    protected void RotateLabel(GameObject label, LabelAlignment alignment)
    {
        var rotation = label.transform.localEulerAngles;
        var position = label.transform.localPosition;

        switch (Direction)
        {
            case Direction.North:
            case Direction.East:
                rotation.z = -90f;
                position.x = alignment switch
                {
                    LabelAlignment.Top => -LabelOffset,
                    LabelAlignment.Bottom => LabelOffset,
                    _ => 0
                };
                break;
            case Direction.South:
            case Direction.West:
                rotation.z = 90f;
                position.x = alignment switch
                {
                    LabelAlignment.Top => LabelOffset,
                    LabelAlignment.Bottom => -LabelOffset,
                    _ => 0
                };
                break;
            default:
                Debug.Log("Unrecognized direction!"); // กรณีทิศทางไม่ถูกต้อง
                break;
        }

        // ตั้งค่าการหมุนและตำแหน่งของ Label
        label.transform.localEulerAngles = rotation;
        label.transform.localPosition = position;
    }
}

// Enum สำหรับกำหนดทิศทาง
public enum Direction { North, South, East, West };