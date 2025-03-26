using System.Collections;
using UnityEngine;
using TMPro;

public class CircuitTesterUIController : MonoBehaviour
{
    [Header("อ้างอิง CircuitTesterWithScore")]
    public CircuitTesterWithScore circuitTester;

    [Header("UI สำหรับแสดงผลลัพธ์")]
    public TMP_Text resultText;

    // เรียกเมื่อผู้ใช้กดปุ่ม "ตรวจสอบวงจร" (OnClick)
    public void OnCheckCircuitClicked()
    {
        // เรียกใช้ Coroutine เพื่อทำงานต่อเนื่อง
        
        StartCoroutine(DoCheckCircuit());
    }
    public void OnsubmitCircuit()
    {
        // เรียกใช้ Coroutine เพื่อทำงานต่อเนื่อง
        resultText.text = $"ส่งคะแนนสำเร็จ";
        circuitTester.SubmitScore();
    }
    private IEnumerator DoCheckCircuit()
    {
        // สั่งตรวจสอบวงจร
        circuitTester.CheckScore();

        // รอจนกว่าตัวตรวจสอบใน CircuitTesterWithScore จะเสร็จสมบูรณ์
        yield return new WaitUntil(() => circuitTester.IsCheckComplete);

        // ตอนนี้แน่ใจแล้วว่าการตรวจสอบจบจริง → อ่านคะแนน/ข้อความ
        int finalScore = circuitTester.Score;
        int maxScore = circuitTester.maxScore;
        string errorReasons = circuitTester.ErrorMessage;

        // อัปเดต UI
        if (resultText != null)
        {
            // ถ้าคะแนนเต็ม ให้ซ่อนข้อความเหตุผล
            if (finalScore == maxScore)
            {
                resultText.text = $"คะแนนรวม: {finalScore} / {maxScore}\n\n";

            }
            else
            {
                resultText.text = $"คะแนนรวม: {finalScore} / {maxScore}\n\n" +
                                  $"เหตุผลที่ไม่ถูกต้อง:\n{errorReasons}";
            }
        }
    }
}
