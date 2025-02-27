using UnityEngine;
using System.Collections.Generic;

public class NotGate : MonoBehaviour
{
    public InputConnector input; // Input ของ NOT Gate
    public OutputConnector output; // Output ของ NOT Gate

    void Start()
    {
        if (input != null && output != null)
        {
            input.AddNotGate(this);
        }

        // ตั้งชื่อ Input และ Output
        if (input != null)
        {
            input.gameObject.name = $"{gameObject.name}_IN1";
        }

        if (output != null)
        {
            output.gameObject.name = $"{gameObject.name}_OUT1";
        }
    }


    void Update()
    {
        UpdateState(); // ให้ NOT Gate ตรวจสอบค่าทุกเฟรม
    }

    public void UpdateState()
    {
        if (input != null && output != null)
        {
            output.isOn = !input.isOn; // ตรงข้ามกับค่าของ Input
        }
    }
}
