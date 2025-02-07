using UnityEngine;
using System.Collections.Generic;

public class XnorGate : MonoBehaviour
{
    public List<InputConnector> inputs = new List<InputConnector>(); // รายชื่อ Input (ต้องมี 2 ตัว)
    public OutputConnector output; // Output ของ XNOR Gate

    void Start()
    {
        foreach (var input in inputs)
        {
            input.AddXnorGate(this);
        }
    }

    void Update()
    {
        UpdateState(); // ตรวจสอบค่าตลอดเวลา
    }

    public void UpdateState()
    {
        if (inputs.Count != 2 || output == null) return; // XNOR Gate ต้องมี 2 Input เท่านั้น

        bool newState = !(inputs[0].isOn ^ inputs[1].isOn); // XNOR = NOT XOR
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
                wireManager.SelectOutput(output); // ให้ XNOR Gate สามารถสร้างสายไฟจาก Output ได้
            }
        }
    }
}
