using System.Collections;
using UnityEngine;
using TMPro;

public class Flute : CircuitComponent, IResistor
{
    // Public members set in Unity Object Inspector
    public GameObject labelResistance;
    public TMP_Text labelResistanceText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;

    public float resistanceValue = 220f; // ค่าความต้านทานเริ่มต้น

    public float Resistance
    {
        get => resistanceValue;
        set
        {
            resistanceValue = Mathf.Max(0, value); // ป้องกันค่าต่ำกว่า 0
            UpdateResistanceLabel(); // อัปเดต Label เมื่อค่าเปลี่ยน
        }
    }

    public Flute()
    {
        Resistance = 220f; // ค่าเริ่มต้น
    }

    

    private void UpdateResistanceLabel()
    {
        if (labelResistanceText != null)
        {
            labelResistanceText.text = $"{Resistance:0.#} Ω"; // อัปเดตข้อความใน Label
        }
    }

    protected override void Update()
    {
        if (Lab != null)
        {
            // Show/hide the labels
            labelResistance.gameObject.SetActive(IsActive && Lab.showLabels);
            labelCurrent.gameObject.SetActive(IsActive && Lab.showLabels);
        }
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;

        if (labelResistanceText != null)
        {
            // Set resistance label text
            labelResistanceText.text = $"{Resistance:0.#} Ω";
            RotateLabel(labelResistance, LabelAlignment.Top);
        }

        if (labelCurrentText != null)
        {
            RotateLabel(labelCurrent, LabelAlignment.Bottom);
        }
    }

    public override void SetCurrent(double current)
    {
        Current = current;

        if (labelCurrentText != null)
        {
            // Update current label text
            labelCurrentText.text = $"{(current * 1000f):0.#} mA";
        }
    }
}