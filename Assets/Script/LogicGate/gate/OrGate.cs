using UnityEngine;
using System.Collections.Generic;

public class OrGate : MonoBehaviour
{
    public List<InputConnector> inputs = new List<InputConnector>(); // รายชื่อ Input หลายตัว
    public OutputConnector output; // Output ของ OR Gate

    void Start()
    {
        foreach (var input in inputs)
        {
            input.AddOrGate(this);
        }

        // ตั้งชื่อ Input และ Output
        for (int i = 0; i < inputs.Count; i++)
        {
            if (inputs[i] != null)
            {
                inputs[i].gameObject.name = $"{gameObject.name}_IN{i + 1}";
            }
        }

        if (output != null)
        {
            output.gameObject.name = $"{gameObject.name}_OUT";
        }
    }


    void Update()
    {
        UpdateState(); // ตรวจสอบค่าตลอดเวลา
    }

    public void UpdateState()
    {
        if (inputs.Count == 0 || output == null) return;

        // ค่าเริ่มต้นเป็น false ถ้ามี Input เป็น true อย่างน้อย 1 ตัว ให้เปลี่ยนเป็น true
        bool newState = false;
        foreach (var input in inputs)
        {
            if (input.isOn) // ถ้าเจอ true ตัวเดียว
            {
                newState = true;
                break;
            }
        }

        output.isOn = newState; // กำหนดค่าให้ Output
        output.UpdateState(); // แจ้งให้ Output ที่เชื่อมต่ออัปเดต
    }

    void OnMouseDown()
    {
        if (output != null)
        {
            WireManager wireManager = FindObjectOfType<WireManager>();
            if (wireManager != null)
            {
                wireManager.SelectOutput(output); // ให้ OR Gate สามารถสร้างสายไฟจาก Output ได้
            }
        }
    }
}
