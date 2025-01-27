using UnityEngine;

public class ICInverter : CircuitComponent
{
    public string InverterModelName = "INV1";
    public float ThresholdVoltage = 2.5f;  // ระดับแรงดันที่เปลี่ยนสถานะ
    public float HighVoltage = 5.0f;       // ระดับแรงดันสูงสุด
    public float LowVoltage = 0.0f;        // ระดับแรงดันต่ำสุด

    public override void SetVoltage(double voltage)
    {
        base.SetVoltage(voltage);
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        Debug.Log($"Inverter - Input: {Voltage:0.00}V, Output: {(Voltage > ThresholdVoltage ? LowVoltage : HighVoltage):0.00}V");
    }
}
