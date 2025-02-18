using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// QuizManager.cs (ฉบับขยาย)
/// สำหรับตรวจสอบโจทย์การต่อวงจรลอจิก พร้อมระบบให้คะแนนอย่างง่าย
/// รองรับ AndGate, OrGate, NandGate, NorGate, XorGate, XnorGate, NotGate
/// </summary>
public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class LogicTask
    {
        [Header("คำอธิบายโจทย์")]
        [TextArea(2, 5)]
        public string description;    

        [Header("รายการ AndGate / OrGate / LogicGates ที่ต้องตรวจสอบ")]
        public List<AndGate>  andGatesToCheck;
        public List<OrGate>   orGatesToCheck;
        public List<NandGate> nandGatesToCheck;
        public List<NorGate>  norGatesToCheck;
        public List<XorGate>  xorGatesToCheck;
        public List<XnorGate> xnorGatesToCheck;
        public List<NotGate>  notGatesToCheck;
        // หากมี Gate อื่น ๆ เพิ่มเติม สามารถขยายเองได้

        [Header("เงื่อนไขค่าความถูกต้อง (คาดหวัง)")]
        public bool expectedResultForAllGates;

        [Header("คะแนนในข้อนี้")]
        public int score;
    }

    [Header("รายการโจทย์ทั้งหมด")]
    public List<LogicTask> tasks = new List<LogicTask>();

    [Header("คะแนนรวม (ดูได้ใน Inspector)")]
    public int totalScore;

    [Header("ข้อความแสดงผลการตรวจ")]
    public string resultMessage;

    // ฟังก์ชันสำหรับตรวจสอบโจทย์ทั้งหมด
    public void CheckAllTasks()
    {
        int scoreAccumulated = 0;      // รวมคะแนน
        string messageBuilder = "";    // ข้อความสรุป

        // วนลูปใน tasks ทุกข้อ
        for (int i = 0; i < tasks.Count; i++)
        {
            LogicTask currentTask = tasks[i];
            bool isTaskCorrect = true;

            // 1) ตรวจ AndGate
            foreach (AndGate andGate in currentTask.andGatesToCheck)
            {
                if (andGate == null || andGate.output == null)
                {
                    isTaskCorrect = false;
                    break;
                }
                if (andGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    isTaskCorrect = false;
                    break;
                }
            }

            // 2) ตรวจ OrGate
            foreach (OrGate orGate in currentTask.orGatesToCheck)
            {
                if (orGate == null || orGate.output == null)
                {
                    isTaskCorrect = false;
                    break;
                }
                if (orGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    isTaskCorrect = false;
                    break;
                }
            }

            // 3) ตรวจ NandGate
            foreach (NandGate nandGate in currentTask.nandGatesToCheck)
            {
                if (nandGate == null || nandGate.output == null)
                {
                    isTaskCorrect = false;
                    break;
                }
                if (nandGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    isTaskCorrect = false;
                    break;
                }
            }

            // 4) ตรวจ NorGate
            foreach (NorGate norGate in currentTask.norGatesToCheck)
            {
                if (norGate == null || norGate.output == null)
                {
                    isTaskCorrect = false;
                    break;
                }
                if (norGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    isTaskCorrect = false;
                    break;
                }
            }

            // 5) ตรวจ XorGate
            foreach (XorGate xorGate in currentTask.xorGatesToCheck)
            {
                if (xorGate == null || xorGate.output == null)
                {
                    isTaskCorrect = false;
                    break;
                }
                if (xorGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    isTaskCorrect = false;
                    break;
                }
            }

            // 6) ตรวจ XnorGate
            foreach (XnorGate xnorGate in currentTask.xnorGatesToCheck)
            {
                if (xnorGate == null || xnorGate.output == null)
                {
                    isTaskCorrect = false;
                    break;
                }
                if (xnorGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    isTaskCorrect = false;
                    break;
                }
            }

            // 7) ตรวจ NotGate
            foreach (NotGate notGate in currentTask.notGatesToCheck)
            {
                if (notGate == null || notGate.output == null)
                {
                    isTaskCorrect = false;
                    break;
                }
                if (notGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    isTaskCorrect = false;
                    break;
                }
            }

            // ตรวจสอบผลลัพธ์ของข้อนี้
            if (isTaskCorrect)
            {
                scoreAccumulated += currentTask.score;
                messageBuilder += $"โจทย์ข้อที่ {i + 1}: ถูกต้อง! +{currentTask.score} คะแนน\n";
            }
            else
            {
                messageBuilder += $"โจทย์ข้อที่ {i + 1}: ยังไม่ถูกต้อง\n";
            }
        }

        // สรุปคะแนนรวม
        totalScore = scoreAccumulated;
        resultMessage = $"คะแนนรวม: {scoreAccumulated} / {GetMaxScore()} \n\nรายละเอียด:\n{messageBuilder}";
        
        // แสดงใน Console
        Debug.Log(resultMessage);
    }

    // ฟังก์ชันหาคะแนนสูงสุด (เพื่อไว้เทียบ)
    public int GetMaxScore()
    {
        int maxScore = 0;
        foreach (var task in tasks)
        {
            maxScore += task.score;
        }
        return maxScore;
    }

#if UNITY_EDITOR
    // ตัวอย่างปุ่ม (GUI) เรียก CheckAllTasks (เฉพาะใน Editor เวลา Play)
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 40), "ตรวจสอบโจทย์"))
        {
            CheckAllTasks();
        }

        GUI.Label(new Rect(10, 60, 500, 200), resultMessage);
    }
#endif
}
