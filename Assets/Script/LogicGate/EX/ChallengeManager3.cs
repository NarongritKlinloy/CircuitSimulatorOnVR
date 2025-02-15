using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChallengeManager3 : MonoBehaviour
{
    [Header("อุปกรณ์ที่ต้องใช้")]
    public Clock clock; // ตัว Clock ควบคุมการนับ
    public SevenSegmentDisplay sevenSegmentDisplay; // 7-Segment Display สำหรับแสดงผล

    [Header("ค่าเป้าหมายที่ต้องการแสดงผล")]
    public List<int> targetNumbers = new List<int>(); // เลขที่ 7-Segment ต้องแสดงผล

    [Header("UI สำหรับแสดงสถานะ")]
    public Text targetNumbersText; // แสดงเลขเป้าหมายที่ต้องทำให้ถูก
    public Text scoreText; // แสดงคะแนนแบบเรียลไทม์

    private int score = 0; // ระบบคะแนน
    private HashSet<int> achievedNumbers = new HashSet<int>(); // เก็บเลขที่เคยแสดงแล้ว

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (CheckClockUsage()) // ตรวจสอบ Clock ทุกครั้ง
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
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}"; // อัปเดตคะแนนบน UI
        }

        Debug.Log($"📌 เลขเป้าหมายที่ต้องแสดง: {string.Join(", ", targetNumbers)}");
    }

    bool CheckClockUsage()
    {
        // ✅ ตรวจสอบว่า Clock มีการเชื่อมต่อและทำงานจริง
        if (clock == null || clock.output == null || !clock.output.IsConnected())
        {
            Debug.Log("❌ Clock ยังไม่ได้เชื่อมต่อหรือไม่มีการใช้งาน!");
            return false;
        }
        Debug.Log("✅ Clock ถูกใช้งานในวงจร");
        return true;
    }

    void CheckChallengeCompletion()
    {
        if (sevenSegmentDisplay != null)
        {
            bool isOutputCorrect = CheckSevenSegmentOutput(); // ✅ ตรวจสอบค่าที่แสดง
            score = CalculateScore(isOutputCorrect);
            UpdateUI(); // อัปเดตคะแนนแบบเรียลไทม์

            Debug.Log(isOutputCorrect
                ? $"✅ โจทย์สำเร็จแล้ว! คะแนน: {score}"
                : $"❌ ยังไม่สำเร็จ คะแนน: {score}");
        }
    }

    bool CheckSevenSegmentOutput()
    {
        if (sevenSegmentDisplay == null) return false;

        int displayedValue = sevenSegmentDisplay.GetCurrentValue(); // ดึงค่าที่ 7-Segment แสดงผล
        achievedNumbers.Add(displayedValue); // บันทึกค่าที่เคยแสดง

        Debug.Log($"🔍 ค่าแสดงผลบน 7-Segment: {displayedValue} | ค่าที่ต้องการ: {string.Join(", ", targetNumbers)}");
        Debug.Log($"📊 ค่าเลขที่เคยแสดงแล้ว: {string.Join(", ", achievedNumbers)}");

        // ✅ ตรวจสอบว่าแสดงครบทุกเลขใน targetNumbers หรือไม่
        return achievedNumbers.SetEquals(targetNumbers);
    }

    int CalculateScore(bool isOutputCorrect)
    {
        int newScore = 0;

        if (isOutputCorrect)
        {
            newScore += 50; // ถ้าแสดงผลถูกต้องให้ 50 คะแนน
        }
        else
        {
            newScore -= 10; // ถ้าแสดงผลผิด หัก 10 คะแนน
        }

        Debug.Log($"💯 คะแนนที่อัปเดต: {newScore}");
        return Mathf.Max(0, newScore);
    }
}
