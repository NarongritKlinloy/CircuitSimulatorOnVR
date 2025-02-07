using UnityEngine;
using System.Collections.Generic;

public class NandGate : MonoBehaviour
{
    public List<InputConnector> inputs = new List<InputConnector>(); // รายชื่อ Input หลายตัว
    public OutputConnector output; // Output ของ NAND Gate

    void Start()
    {
        foreach (var input in inputs)
        {
            input.AddNandGate(this);
        }
    }

    void Update()
    {
        UpdateState(); // ตรวจสอบค่าตลอดเวลา
    }

    public void UpdateState()
    {
        if (inputs.Count == 0 || output == null) return;

        bool newState = false; // ค่าเริ่มต้นเป็น false ถ้ามี Input ใดเป็น false ให้เป็น true
        foreach (var input in inputs)
        {
            if (!input.isOn) // ถ้าเจอ false ตัวเดียว
            {
                newState = true;
                break;
            }
        }

        output.isOn = newState; // กำหนดค่าให้ Output
        output.UpdateState(); // แจ้งให้ Output ที่เชื่อมต่ออัปเดต
    }
}
