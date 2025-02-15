using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChallengeManager2_1 : MonoBehaviour
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
    public List<ToggleSwitch> toggleSwitches = new List<ToggleSwitch>(); // ✅ เปลี่ยนเป็น List<ToggleSwitch>
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
        if (toggleSwitches.Count > 0) // ✅ เปลี่ยนจาก 4 เป็นตรวจสอบว่า Toggle มีอยู่จริง
        {
            bool isGateCorrect = CheckGatePresence();
            bool isOutputCorrect = CheckToggleSwitches();
            bool isConnectionCorrect = CheckConnections();

            score = CalculateScore(isOutputCorrect, isConnectionCorrect, isGateCorrect);

            bool isComplete = isOutputCorrect && isConnectionCorrect && isGateCorrect;
            Debug.Log(isComplete ? $"✅ โจทย์สำเร็จแล้ว! คะแนน: {score}" : $"❌ ยังไม่สำเร็จ คะแนน: {score}");
        }
    }

    bool CheckToggleSwitches()
    {
        int switchValue = 0;
        for (int i = 0; i < toggleSwitches.Count; i++) // ✅ รองรับจำนวน Toggle Switch แบบไดนามิก
        {
            if (toggleSwitches[i] != null && toggleSwitches[i].isOn)
            {
                switchValue |= (1 << i);
            }
        }

        bool isLEDOn = ledToCheck.input.isOn;
        bool shouldLEDBeOn = targetNumbers.Contains(switchValue);

        Debug.Log($"🔍 ค่า Toggle Switch: {switchValue} | ค่าที่ต้องการ: {string.Join(", ", targetNumbers)} | LED ติด: {isLEDOn}");

        if (isLEDOn != shouldLEDBeOn)
        {
            return false;
        }

        return true;
    }

    bool CheckConnections()
    {
        bool isConnected = false;
        WireManager[] wireManagers = FindObjectsOfType<WireManager>();

        foreach (var wireManager in wireManagers)
        {
            foreach (var connection in wireManager.GetWireConnections())
            {
                OutputConnector output = connection.Key.Item1;
                InputConnector input = connection.Key.Item2;

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
            baseScore += 30;
        }
        else
        {
            baseScore -= 10;
        }

        if (isConnectionCorrect)
        {
            baseScore += 20;
        }
        else
        {
            baseScore -= 10;
        }

        if (isGateCorrect)
        {
            baseScore += 15;
        }
        else if (requireAndGate || requireOrGate || requireNandGate || requireNorGate || requireXorGate || requireXnorGate || requireNotGate)
        {
            baseScore -= 10;
        }

        return Mathf.Max(0, baseScore);
    }
}
