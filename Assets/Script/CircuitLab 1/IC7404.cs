using System.Collections.Generic;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;


// คลาส IC7404 สืบทอดจาก CircuitComponent (ซึ่งเป็นคลาสพื้นฐานที่ใช้ในระบบ breadboard ของคุณ)
public class IC7404 : CircuitComponent
{
    // กำหนดจำนวนขา: 14 ขา (index 0 ถึง 13)
    // ผู้ใช้/นักออกแบบจะต้องเซตค่า connection points (peg หรือ Node) สำหรับแต่ละขาใน Inspector หรือผ่าน code
    public Point[] Pins = new Point[14];

    // กำหนด mapping ของ inverter gates 6 ตัว (index ใน array 0-based)
    // โดยการแมปให้แต่ละ gate มีคู่ [input, output]
    // ตัวอย่างการแมปที่ใช้กันทั่วไป:
    //   Gate 1: Input = pin1 (index 0), Output = pin2 (index 1)
    //   Gate 2: Input = pin3 (index 2), Output = pin4 (index 3)
    //   Gate 3: Input = pin5 (index 4), Output = pin6 (index 5)
    //   Gate 4: Input = pin9 (index 8), Output = pin8 (index 7)
    //   Gate 5: Input = pin11 (index 10), Output = pin12 (index 11)
    //   Gate 6: Input = pin13 (index 12), Output = pin14 (index 13)
    private readonly (int input, int output)[] gateMapping = new (int, int)[]
    {
        (0, 1),   // Gate 1: pin1 -> pin2
        (2, 3),   // Gate 2: pin3 -> pin4
        (4, 5),   // Gate 3: pin5 -> pin6
        (8, 7),   // Gate 4: pin9 -> pin8
        (10, 11), // Gate 5: pin11 -> pin12
        (12, 13)  // Gate 6: pin13 -> pin14
    };

    // กำหนดขาสำหรับ Vcc และ GND (0-indexed)
    // ตาม convention ใน 7404: pin 14 คือ Vcc, pin 7 คือ GND
    public int VccPinIndex = 13; // pin14
    public int GndPinIndex = 6;  // pin7

    // กำหนดค่า threshold สำหรับ inverter (หน่วยเป็น Volt)
    // ค่า typical TTL threshold จะอยู่ที่ประมาณ 1.5 - 2.0 V
    public double ThresholdVoltage = 2.0;

    // เมธอดนี้จะถูกเรียกในขั้นตอนการสร้าง netlist ในการจำลอง
    // เราจะสร้าง behavioral voltage sources สำหรับแต่ละ gate โดยพิจารณาจาก condition:
    //     ถ้า V(input) > Threshold  => output = 0V
    //     ถ้า V(input) <= Threshold => output = V(Vcc) (เอาค่า Vcc จากขา Vcc ที่เชื่อมอยู่)
    public void AddSpiceSharpEntities(List<SpiceSharp.Entities.Entity> entities)
    {
        // ตรวจสอบว่าได้กำหนดขา Vcc และ GND เรียบร้อยแล้ว
        string vccNode = Pins[VccPinIndex].ToString();
        string gndNode = Pins[GndPinIndex].ToString();

        // สำหรับแต่ละ gate ให้สร้าง BehavioralVoltageSource
        for (int i = 0; i < gateMapping.Length; i++)
        {
            var mapping = gateMapping[i];
            string inputNode = Pins[mapping.input].ToString();
            string outputNode = Pins[mapping.output].ToString();

            // สร้างชื่อที่ไม่ซ้ำสำหรับ gate นี้
            string gateName = $"{this.gameObject.name}_gate{i + 1}";

            // นิพจน์สำหรับ behavioral source:
            // ถ้า V(inputNode) > ThresholdVoltage ให้กำหนด 0V (logic low)
            // มิฉะนั้น ให้กำหนดเป็น V(Vcc) (logic high)
            // รูปแบบ expression ใน SpiceSharp (หากเวอร์ชันที่ใช้งานรองรับ) จะเป็น:
            //    if(V(inputNode) > ThresholdVoltage, 0, V(vccNode))
            string expression = $"if(V({inputNode})>{ThresholdVoltage},0,V({vccNode}))";

            // สร้าง BehavioralVoltageSource ใหม่
            var bvs = new BehavioralVoltageSource(gateName, outputNode, gndNode, expression);

            // เพิ่ม entity นี้ลงใน netlist
            entities.Add(bvs);
        }
    }

    // (หากต้องการแสดงผลค่า logic หรืออื่นๆ สามารถ override เมธอด SetVoltage, SetCurrent ได้ตามต้องการ)
    public override void SetVoltage(double voltage)
    {
        // อาจไม่จำเป็นสำหรับ digital chip แต่สามารถใส่การแสดงผลให้กับ label หรือ Debug ได้
    }

    public override void SetCurrent(double current)
    {
        // ไม่จำเป็นสำหรับ digital chip
    }
}
