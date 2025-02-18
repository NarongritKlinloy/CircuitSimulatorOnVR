using UnityEngine;
using System.Collections.Generic;

public class NorGate : MonoBehaviour
{
    public List<InputConnector> inputs = new List<InputConnector>(); // รายชื่อ Input หลายตัว
    public OutputConnector output; // Output ของ NOR Gate

    void Start()
    {
        foreach (var input in inputs)
        {
            input.AddNorGate(this);
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

        bool newState = true; // เริ่มต้นที่ true และจะเปลี่ยนเป็น false ถ้ามี Input ใดเป็น true
        foreach (var input in inputs)
        {
            if (input.isOn)
            {
                newState = false;
                break;
            }
        }

        output.isOn = newState; // กำหนดค่าให้ Output
        output.UpdateState(); // แจ้งให้ Output ที่เชื่อมต่ออัปเดต
    }
}
