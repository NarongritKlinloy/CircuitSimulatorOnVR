using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChallengeManager1 : MonoBehaviour
{
    [Header("กำหนดโจทย์ (เลือก Gate ที่ต้องมี)")]
    public bool requireAndGate = false;
    public bool requireOrGate = false;
    public bool requireNandGate = false;
    public bool requireNorGate = false;
    public bool requireXorGate = false;
    public bool requireXnorGate = false;
    public bool requireNotGate = false;

    [Header("ตัวตรวจสอบผลลัพธ์")]
    public LED ledToCheck; // LED ที่ต้องตรวจสอบ

    [Header("UI สำหรับแสดงจำนวน ToggleSwitch (ถ้ามี)")]
    public Text toggleSwitchCountText; // ตัวแปรสำหรับแสดงจำนวน ToggleSwitch บน UI

    private int score = 0; // ระบบคะแนน

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        CheckChallengeCompletion();
    }

    void UpdateUI()
    {
        // อัปเดตค่าใน UI ถ้ามีการตั้งค่า Text
        int switchCount = FindObjectsOfType<ToggleSwitch>().Length;
        if (toggleSwitchCountText != null)
        {
            toggleSwitchCountText.text = $"Toggle Switch: {switchCount}";
        }
        Debug.Log($"📌 พบ ToggleSwitch ใน Scene: {switchCount} ตัว");
    }

    void CheckChallengeCompletion()
    {
        if (ledToCheck != null && ledToCheck.input != null)
        {
            bool isLEDOn = ledToCheck.input.isOn;

            // ตรวจสอบว่า Logic Gate ที่จำเป็นมีอยู่จริงหรือไม่
            bool isGateCorrect = CheckGatePresence();

            // ตรวจสอบว่า Output ถูกต้องหรือไม่ โดยไม่สนใจเส้นทางที่เชื่อมต่อ
            bool isOutputCorrect = isLEDOn;

            // คำนวณคะแนน
            score = CalculateScore(isOutputCorrect, isGateCorrect);

            bool isComplete = isOutputCorrect && isGateCorrect;
            Debug.Log(isComplete ? $"✅ โจทย์สำเร็จแล้ว! คะแนน: {score}" : $"❌ ยังไม่สำเร็จ คะแนน: {score}");
        }
    }

    bool CheckGatePresence()
    {
        int requiredGateCount = 0;
        int foundGateCount = 0;

        if (requireAndGate) { requiredGateCount++; if (FindObjectsOfType<AndGate>().Length > 0) foundGateCount++; }
        if (requireOrGate) { requiredGateCount++; if (FindObjectsOfType<OrGate>().Length > 0) foundGateCount++; }
        if (requireNandGate) { requiredGateCount++; if (FindObjectsOfType<NandGate>().Length > 0) foundGateCount++; }
        if (requireNorGate) { requiredGateCount++; if (FindObjectsOfType<NorGate>().Length > 0) foundGateCount++; }
        if (requireXorGate) { requiredGateCount++; if (FindObjectsOfType<XorGate>().Length > 0) foundGateCount++; }
        if (requireXnorGate) { requiredGateCount++; if (FindObjectsOfType<XnorGate>().Length > 0) foundGateCount++; }
        if (requireNotGate) { requiredGateCount++; if (FindObjectsOfType<NotGate>().Length > 0) foundGateCount++; }

        // ถ้าไม่ได้กำหนดให้ต้องมี Gate ใด ๆ หรือมีบางตัวที่ถูกต้อง ให้ผ่าน
        return requiredGateCount == 0 || foundGateCount > 0;
    }

    int CalculateScore(bool isOutputCorrect, bool isGateCorrect)
    {
        int baseScore = 0;

        if (isOutputCorrect)
        {
            baseScore += 30; // ถ้า Output ถูกต้องให้ 30 คะแนน
        }
        else
        {
            baseScore -= 10; // ถ้า Output ผิด หัก 10 คะแนน
        }

        if (isGateCorrect)
        {
            baseScore += 15; // ถ้ามี Logic Gate อย่างน้อยหนึ่งตัวจากรายการที่กำหนดให้ 15 คะแนน
        }
        else if (requireAndGate || requireOrGate || requireNandGate || requireNorGate || requireXorGate || requireXnorGate || requireNotGate)
        {
            baseScore -= 10; // ถ้าต้องใช้ Gate แต่ไม่มีตัวที่ถูกต้องเลย หัก 10 คะแนน
        }

        return Mathf.Max(0, baseScore); // ป้องกันคะแนนติดลบ
    }
}
