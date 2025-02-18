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
    public LED ledToCheck;

    [Header("UI สำหรับแสดงจำนวน ToggleSwitch (ถ้ามี)")]
    public Text toggleSwitchCountText;

    private int score = 0;

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
        int switchCount = FindObjectsOfType<ToggleSwitch>().Length;
        if (toggleSwitchCountText != null)
        {
            toggleSwitchCountText.text = $"Toggle Switch: {switchCount}";
        }
        Debug.Log($"📌 พบ ToggleSwitch ใน Scene: {switchCount} ตัว");
    }

    void CheckChallengeCompletion()
    {
        if (ledToCheck == null || ledToCheck.input == null)
        {
            Debug.LogError("❌ LED หรือ input ของ LED เป็น null!");
            return;
        }

        bool isLEDOn = ledToCheck.input.isOn;
        bool isGateCorrect = CheckGatePresence();
        bool isLEDConnectedToCorrectGate = IsLEDConnectedToCorrectGate();
        bool isOutputCorrect = isLEDOn && isLEDConnectedToCorrectGate;

        score = CalculateScore(isOutputCorrect, isGateCorrect);

        bool isComplete = isOutputCorrect && isGateCorrect;
        Debug.Log(isComplete ? $"✅ โจทย์สำเร็จแล้ว! คะแนน: {score}" : $"❌ ยังไม่สำเร็จ คะแนน: {score}");
    }

    bool CheckGatePresence()
    {
        Dictionary<string, bool> requiredGates = new Dictionary<string, bool>
        {
            { "AndGate", requireAndGate },
            { "OrGate", requireOrGate },
            { "NandGate", requireNandGate },
            { "NorGate", requireNorGate },
            { "XorGate", requireXorGate },
            { "XnorGate", requireXnorGate },
            { "NotGate", requireNotGate }
        };

        int requiredGateCount = 0, foundGateCount = 0;

        foreach (var gate in requiredGates)
        {
            if (gate.Value)
            {
                requiredGateCount++;
                if (HasGateWithNumberedName(gate.Key)) foundGateCount++;
            }
        }

        return requiredGateCount == 0 || foundGateCount > 0;
    }

    bool HasGateWithNumberedName(string gateName)
    {
        GameObject[] gates = GameObject.FindGameObjectsWithTag("Gate");
        foreach (GameObject gate in gates)
        {
            if (gate.name.StartsWith(gateName + "_"))
            {
                string suffix = gate.name.Substring(gateName.Length + 1);
                if (int.TryParse(suffix, out _)) return true;
            }
        }
        return false;
    }

    bool IsLEDConnectedToCorrectGate()
    {
        GameObject inputSource = ledToCheck.input.gameObject;
        return inputSource != null && IsCorrectGateName(inputSource.name);
    }

    bool IsCorrectGateName(string name)
    {
        return (requireAndGate && name.StartsWith("AndGate_")) ||
               (requireOrGate && name.StartsWith("OrGate_")) ||
               (requireNandGate && name.StartsWith("NandGate_")) ||
               (requireNorGate && name.StartsWith("NorGate_")) ||
               (requireXorGate && name.StartsWith("XorGate_")) ||
               (requireXnorGate && name.StartsWith("XnorGate_")) ||
               (requireNotGate && name.StartsWith("NotGate_"));
    }

    int CalculateScore(bool isOutputCorrect, bool isGateCorrect)
    {
        int baseScore = 0;
        if (isOutputCorrect) baseScore += 30;
        else baseScore -= 10;
        if (isGateCorrect) baseScore += 15;
        return Mathf.Max(0, baseScore);
    }
}
