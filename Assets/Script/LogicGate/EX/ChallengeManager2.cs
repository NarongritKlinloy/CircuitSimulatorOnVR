using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChallengeManager2 : MonoBehaviour
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
    public LED ledToCheck; // ใช้ LED ตัวเดียว
    public ToggleSwitch[] toggleSwitches = new ToggleSwitch[4]; // 4 Toggle Switch แทนเลขฐาน 2
    public List<int> targetNumbers = new List<int>(); // เลขที่ต้องให้ Toggle Switch ทำให้ LED ติด

    [Header("UI สำหรับแสดงสถานะ")]
    public Text targetNumbersText; // แสดงเลขเป้าหมายที่ต้องทำให้ถูก

    private int score = 0; // ระบบคะแนน
    private bool hasUserInteracted = false; // ตรวจสอบว่าผู้ใช้มีการสับสวิตช์หรือไม่

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (!hasUserInteracted)
        {
            hasUserInteracted = CheckUserInteraction(); // ตรวจสอบว่าผู้ใช้มีการสับสวิตช์หรือไม่
        }

        if (hasUserInteracted)
        {
            CheckChallengeCompletion();
        }
    }

    void UpdateUI()
    {
        if (targetNumbersText != null)
        {
            targetNumbersText.text = $"Target Numbers: {string.Join(", ", targetNumbers)}";
        }

        Debug.Log($"📌 เลขเป้าหมายที่ต้องแสดง: {string.Join(", ", targetNumbers)}");
    }

    bool CheckUserInteraction()
    {
        // ตรวจสอบว่ามี Toggle Switch ตัวไหนถูกเปิดบ้าง
        foreach (var toggle in toggleSwitches)
        {
            if (toggle != null && toggle.isOn)
            {
                return true; // มีการสับสวิตช์แล้ว
            }
        }
        return false; // ยังไม่มีการสับสวิตช์
    }

    void CheckChallengeCompletion()
    {
        if (toggleSwitches.Length == 4)
        {
            bool isGateCorrect = CheckGatePresence();
            bool isOutputCorrect = CheckToggleSwitches(); // ตรวจสอบว่า LED ติดถูกต้องหรือไม่
            bool isConnectionCorrect = CheckConnections(); // ตรวจสอบสายไฟ

            score = CalculateScore(isOutputCorrect, isConnectionCorrect, isGateCorrect);

            bool isComplete = isOutputCorrect && isConnectionCorrect && isGateCorrect;
            Debug.Log(isComplete ? $"✅ โจทย์สำเร็จแล้ว! คะแนน: {score}" : $"❌ ยังไม่สำเร็จ คะแนน: {score}");
        }
    }

    bool CheckToggleSwitches()
    {
        int switchValue = 0;
        for (int i = 0; i < 4; i++)
        {
            if (toggleSwitches[i] != null && toggleSwitches[i].isOn)
            {
                switchValue |= (1 << i); // คำนวณค่า Binary เป็นเลขฐาน 10
            }
        }

        bool isLEDOn = ledToCheck.input.isOn; // ตรวจสอบว่า LED ติดหรือไม่
        bool shouldLEDBeOn = targetNumbers.Contains(switchValue); // ตรวจสอบว่า LED ควรติดหรือไม่

        Debug.Log($"🔍 ค่า Toggle Switch: {switchValue} | ค่าที่ต้องการ: {string.Join(", ", targetNumbers)} | LED ติด: {isLEDOn}");

        // กรณีที่ LED ติดผิด (ไม่ได้อยู่ใน targetNumbers แต่ติด)
        if (isLEDOn && !shouldLEDBeOn)
        {
            return false;
        }

        // กรณีที่ LED ควรติด แต่ไม่ติด
        if (!isLEDOn && shouldLEDBeOn)
        {
            return false;
        }

        return true;
    }

    bool CheckConnections()
    {
        bool isConnected = false;

        // ค้นหาสายไฟทั้งหมดที่มีการเชื่อมต่อ
        WireManager[] wireManagers = FindObjectsOfType<WireManager>();

        foreach (var wireManager in wireManagers)
        {
            foreach (var connection in wireManager.GetWireConnections())
            {
                OutputConnector output = connection.Key.Item1;
                InputConnector input = connection.Key.Item2;

                // ตรวจสอบว่าสายไฟนี้ต่อไปยัง LED หรือไม่
                if (input == ledToCheck.input)
                {
                    isConnected = true;
                    break;
                }
            }
        }

        Debug.Log(isConnected ? "✅ LED เชื่อมต่อกับวงจรเรียบร้อย" : "❌ LED ยังไม่ได้เชื่อมต่อกับวงจร");

        return isConnected;
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

        return requiredGateCount == 0 || foundGateCount > 0;
    }

    int CalculateScore(bool isOutputCorrect, bool isConnectionCorrect, bool isGateCorrect)
    {
        int baseScore = 0;

        if (isOutputCorrect)
        {
            baseScore += 30; // ถ้า LED ทำงานถูกต้องให้ 30 คะแนน
        }
        else
        {
            baseScore -= 10; // ถ้า LED ผิด หัก 10 คะแนน
        }

        if (isConnectionCorrect)
        {
            baseScore += 20; // ถ้าเชื่อมต่อสายไฟถูกต้องให้ 20 คะแนน
        }
        else
        {
            baseScore -= 10; // ถ้าสายไฟผิด หัก 10 คะแนน
        }

        if (isGateCorrect)
        {
            baseScore += 15; // ถ้ามี Logic Gate อย่างน้อยหนึ่งตัวจากรายการที่กำหนดให้ 15 คะแนน
        }
        else if (requireAndGate || requireOrGate || requireNandGate || requireNorGate || requireXorGate || requireXnorGate || requireNotGate)
        {
            baseScore -= 10; // ถ้าต้องใช้ Gate แต่ไม่มีตัวที่ถูกต้องเลย หัก 10 คะแนน
        }

        return Mathf.Max(0, baseScore);
    }
}
