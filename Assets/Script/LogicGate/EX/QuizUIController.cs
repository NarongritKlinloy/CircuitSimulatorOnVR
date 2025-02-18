using UnityEngine;
using TMPro;

public class QuizUIController : MonoBehaviour
{
    public QuizManager quizManager;

    [Header("ตำแหน่ง TMP_Text ที่อยู่ใน Content ของ ScrollView")]
    public TMP_Text tasksScrollText; 
    // ให้คุณเตรียม TextMeshPro ไว้ใน Content ของ ScrollView เพียงตัวเดียว 
    // แล้วลากมาวางช่องนี้

    [Header("Text แสดงผลลัพธ์ (อยู่นอก ScrollView)")]
    public TMP_Text resultText;

    private void Start()
    {
        // แสดงโจทย์ทั้งหมดลงใน TextScroll ทันที
        UpdateTasksDescription();
    }

    // ฟังก์ชันสร้างสตริงโจทย์ทุกข้อ แล้วแสดงใน tasksScrollText
    private void UpdateTasksDescription()
    {
        if (quizManager == null || quizManager.tasks == null || quizManager.tasks.Count == 0)
        {
            if (tasksScrollText != null)
                tasksScrollText.text = "ไม่มีโจทย์ใน QuizManager";
            return;
        }

        string allTasks = "";
        for (int i = 0; i < quizManager.tasks.Count; i++)
        {
            var task = quizManager.tasks[i];
            // ต่อข้อความลงมาเรื่อย ๆ
            allTasks += $"ข้อที่ {i + 1}: {task.description}\n";
        }

        if (tasksScrollText != null)
        {
            tasksScrollText.text = allTasks;
        }
    }

    // เรียกตอนกดปุ่ม หรือ VR Event อะไรก็ตาม
    public void CheckAllTasksAndShowResult()
    {
        if (quizManager == null) return;

        // ให้ QuizManager ตรวจโจทย์
        quizManager.CheckAllTasks();

        // นำข้อความผลลัพธ์ไปแสดง
        if (resultText != null)
        {
            resultText.text = quizManager.resultMessage;
        }
    }
}
