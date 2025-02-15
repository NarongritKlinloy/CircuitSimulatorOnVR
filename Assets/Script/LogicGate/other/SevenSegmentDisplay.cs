using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SevenSegmentDisplay : MonoBehaviour
{
    [Header("Input Connectors (4-bit Binary)")]
    public List<InputConnector> inputs = new List<InputConnector>();

    [Header("Segments (A-G)")]
    public GameObject[] segments;

    [Header("Display")]
    public TMP_Text displayText;

    private int lastDisplayedValue = -1; // ค่าล่าสุดที่แสดงผล เพื่อลดการอัปเดตซ้ำ

    private readonly Dictionary<int, bool[]> segmentMap = new Dictionary<int, bool[]>
    {
        { 0,  new bool[] { true, true, true, true, true, true, false } },
        { 1,  new bool[] { false, true, true, false, false, false, false } },
        { 2,  new bool[] { true, true, false, true, true, false, true } },
        { 3,  new bool[] { true, true, true, true, false, false, true } },
        { 4,  new bool[] { false, true, true, false, false, true, true } },
        { 5,  new bool[] { true, false, true, true, false, true, true } },
        { 6,  new bool[] { true, false, true, true, true, true, true } },
        { 7,  new bool[] { true, true, true, false, false, false, false } },
        { 8,  new bool[] { true, true, true, true, true, true, true } },
        { 9,  new bool[] { true, true, true, true, false, true, true } },
        { 10, new bool[] { true, true, true, false, true, true, true } },
        { 11, new bool[] { false, false, true, true, true, true, true } },
        { 12, new bool[] { true, false, false, true, true, true, false } },
        { 13, new bool[] { false, true, true, true, true, false, true } },
        { 14, new bool[] { true, false, false, true, true, true, true } },
        { 15, new bool[] { true, false, false, false, true, true, true } }
    };

    void Update()
    {
        int currentValue = GetCurrentValue();

        // ✅ อัปเดตค่าเฉพาะเมื่อค่ามีการเปลี่ยนแปลง
        if (currentValue != lastDisplayedValue)
        {
            lastDisplayedValue = currentValue;
            UpdateDisplay(currentValue);
        }
    }

    public int GetCurrentValue()
    {
        int binaryValue = 0;
        string debugBinary = "";

        // ✅ คำนวณค่าทศนิยมจากบิตโดยเรียงจาก LSB → MSB
        for (int i = 0; i < inputs.Count && i < 4; i++)
        {
            if (inputs[i].isOn)
            {
                binaryValue += (1 << i); // คำนวณค่าทศนิยม
            }
            debugBinary = (inputs[i].isOn ? "1" : "0") + debugBinary; // สร้างสตริงบิต
        }

        //Debug.Log($"🔹 SevenSegment[{gameObject.name}] GetCurrentValue() -> Binary: {debugBinary} -> Decimal: {binaryValue}");
        return binaryValue;
    }

    public void UpdateDisplay(int binaryValue)
    {
        if (inputs.Count != 4 || segments.Length != 7)
            return;

        // ✅ ตรวจสอบว่ามีค่าอยู่ใน segmentMap
        if (segmentMap.ContainsKey(binaryValue))
        {
            bool[] segmentState = segmentMap[binaryValue];

            for (int i = 0; i < segments.Length; i++)
            {
                Renderer renderer = segments[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = segmentState[i] ? Color.red : Color.black;
                }
            }

            displayText.text = binaryValue.ToString("X");
            //Debug.Log($"✅ SevenSegment[{gameObject.name}] UpdateDisplay() -> Showing: {binaryValue}");
        }
        else
        {
            Debug.Log($"❌ SevenSegment[{gameObject.name}] ไม่มีค่าที่ตรงกับ {binaryValue} ใน segmentMap");
        }
    }
}
