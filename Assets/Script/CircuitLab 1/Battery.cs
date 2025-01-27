using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Battery : CircuitComponent, IBattery
{
    // เพิ่ม [Header] และ [Tooltip] เพื่อความสวยงามใน Inspector
    [Header("Battery Settings")]
    [Tooltip("Battery voltage (Volts)")]
    public float batteryVoltage = 10f;  // กำหนดค่าเริ่มต้นเป็น 10V

    // interface IBattery ต้องการ property ชื่อ BatteryVoltage
    // เราก็ให้มันอ่านจาก batteryVoltage
    public float BatteryVoltage => batteryVoltage;

    [Header("UI / Labels")]
    public GameObject labelVoltage;
    public TMP_Text labelVoltageText;

    // ------------------------
    // Unity Lifecycle Methods
    // ------------------------

    protected override void Start()
    {
        base.Start();

        // อัปเดต Text เป็นค่าแรงดันตามที่กรอกไว้ใน Inspector
        if (labelVoltageText != null)
        {
            labelVoltageText.text = batteryVoltage.ToString("0.#") + "V";
        }
    }

    protected override void Update()
    {
        base.Update();

        // Show/hide the labels ตามการเปิด-ปิด Active และโชว์ Labels หรือไม่
        if (labelVoltage != null && Lab != null)
        {
            labelVoltage.gameObject.SetActive(IsActive && Lab.showLabels);
        }
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        base.SetActive(isActive, isForward);

        // จัดทิศทางของฉลาก (Label) ให้เหมาะสมตาม Direction
        if (labelVoltage != null)
        {
            var rotationVoltage = labelVoltage.transform.localEulerAngles;
            switch (Direction)
            {
                case Direction.North:
                    rotationVoltage.z = 180f;
                    break;
                case Direction.South:
                    rotationVoltage.z = 0f;
                    break;
                case Direction.East:
                    rotationVoltage.z = -90f;
                    break;
                case Direction.West:
                    rotationVoltage.z = 90f;
                    break;
                default:
                    Debug.Log("Unrecognized direction!");
                    break;
            }
            labelVoltage.transform.localEulerAngles = rotationVoltage;
        }
    }
}
