using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// สมมุติว่าอิมพอร์ทอินเตอร์เฟซสำหรับ Toggle Switch
public interface IToggleSwitch
{
    bool IsClosed { get; }
    // SetState(true) = ปิด (closed) ให้วงจรต่อ, SetState(false) = เปิด (open) ให้วงจรตัด
    void SetState(bool state);
}

public class CircuitTesterWithScore : MonoBehaviour
{
    // อ้างอิงไปที่ CircuitLab ที่มีใน Scene (กำหนดใน Inspector)
    public CircuitLab circuitLab;

    // ใช้เก็บรายการอุปกรณ์ที่ถูกวางลงบน Breadboard
    private List<PlacedComponent> placedComponents;

    // ตัวแปรสำหรับคะแนน
    private int score = 0;
    private int maxScore = 60; // คะแนนเต็ม

    void Start()
    {
        // ดึงรายการอุปกรณ์จาก CircuitLab
        placedComponents = circuitLab.GetPlacedComponents();

        // ตัวแปรนับจำนวนอุปกรณ์ตามประเภทที่ต้องการ
        int countWire = 0;
        int countToggle = 0;
        int countBattery = 0;
        int countResistor = 0;

        // ตรวจสอบประเภทของแต่ละอุปกรณ์ในวงจร
        foreach (PlacedComponent comp in placedComponents)
        {
            var circuitComponent = comp.Component;
            if (circuitComponent is IBattery)
            {
                countBattery++;
            }
            if (circuitComponent is IResistor)
            {
                countResistor++;
            }
            // สำหรับ toggle switch สมมุติว่าใช้ IToggleSwitch interface
            if (circuitComponent is IToggleSwitch)
            {
                countToggle++;
            }
            // สำหรับสายไฟ (wire) สมมุติว่าอุปกรณ์ที่เป็น conductor แต่ไม่ใช่ toggle switch
            if ((circuitComponent is IConductor) && !(circuitComponent is IToggleSwitch))
            {
                countWire++;
            }
        }

        Debug.Log("Circuit Test Summary:");
        Debug.Log("Battery count: " + countBattery);
        Debug.Log("Resistor count: " + countResistor);
        Debug.Log("Toggle Switch count: " + countToggle);
        Debug.Log("Wire count: " + countWire);

        // ตรวจสอบเงื่อนไขขั้นต่ำและให้คะแนน
        bool pass = true;
        if (countBattery >= 1)
        {
            score += 10;
            Debug.Log("ผ่าน: Battery (+10)");
        }
        else
        {
            Debug.LogError("Test Failed: ต้องมี Battery อย่างน้อย 1 ชิ้น");
            pass = false;
        }
        if (countResistor >= 1)
        {
            score += 10;
            Debug.Log("ผ่าน: Resistor (+10)");
        }
        else
        {
            Debug.LogError("Test Failed: ต้องมี Resistor อย่างน้อย 1 ชิ้น");
            pass = false;
        }
        if (countToggle >= 1)
        {
            score += 10;
            Debug.Log("ผ่าน: Toggle Switch (+10)");
        }
        else
        {
            Debug.LogError("Test Failed: ต้องมี Toggle Switch 1 ชิ้น");
            pass = false;
        }
        if (countWire >= 1)
        {
            score += 10;
            Debug.Log("ผ่าน: Wire (+10)");
        }
        else
        {
            Debug.LogError("Test Failed: ต้องมี Wire อย่างน้อย 1 ชิ้น");
            pass = false;
        }

        if (pass)
        {
            Debug.Log("Circuit composition ผ่านเงื่อนไขพื้นฐานแล้ว");
        }
        else
        {
            Debug.LogError("Circuit composition ไม่ผ่านเงื่อนไขที่กำหนด");
        }

        // ตรวจสอบการทำงานของ Toggle Switch และให้คะแนนเพิ่มเติมหากผ่านการทดสอบ
        IToggleSwitch toggleSwitch = FindToggleSwitch();
        if (toggleSwitch != null)
        {
            // สั่งเปิด (close) toggle switch ให้วงจรต่อ แล้วจำลองวงจร
            toggleSwitch.SetState(true);
            Debug.Log("Toggle Switch: เปิด (closed) - จำลองวงจร");
            circuitLab.SimulateCircuit();

            // หน่วงเวลาแล้วสลับสถานะ เพื่อให้เห็นผลการทำงาน
            StartCoroutine(TestToggleSwitch(toggleSwitch));
        }
        else
        {
            Debug.LogError("ไม่พบ Toggle Switch ในวงจร");
        }
    }

    // ฟังก์ชันค้นหา toggle switch จากรายการอุปกรณ์
    private IToggleSwitch FindToggleSwitch()
    {
        foreach (PlacedComponent comp in placedComponents)
        {
            if (comp.Component is IToggleSwitch)
            {
                return comp.Component as IToggleSwitch;
            }
        }
        return null;
    }

    // Coroutine เพื่อสลับสถานะของ Toggle Switch แล้วจำลองวงจรซ้ำ
    IEnumerator TestToggleSwitch(IToggleSwitch toggleSwitch)
    {
        // รอ 2 วินาที แล้วสั่งปิด toggle switch (open circuit)
        yield return new WaitForSeconds(2f);
        toggleSwitch.SetState(false);
        Debug.Log("Toggle Switch: ปิด (open) - จำลองวงจร");
        circuitLab.SimulateCircuit();

        // รออีก 2 วินาที แล้วสั่งเปิด toggle switch อีกครั้ง
        yield return new WaitForSeconds(2f);
        toggleSwitch.SetState(true);
        Debug.Log("Toggle Switch: เปิด (closed) อีกครั้ง - จำลองวงจร");
        circuitLab.SimulateCircuit();

        // หาก Toggle Switch ทดสอบผ่านการสลับสถานะทั้งหมด ให้เพิ่มคะแนน
        score += 20;
        Debug.Log("ผ่าน: การสลับสถานะ Toggle Switch (+20)");

        // แสดงคะแนนรวม
        Debug.Log("คะแนนรวม: " + score + " / " + maxScore);
    }

    // ฟังก์ชันตรวจสอบคะแนน (สามารถเรียกจากภายนอกเพื่อแสดงคะแนนได้)
    public int CheckScore()
    {
        Debug.Log("คะแนนปัจจุบัน: " + score + " / " + maxScore);
        return score;
    }
}
