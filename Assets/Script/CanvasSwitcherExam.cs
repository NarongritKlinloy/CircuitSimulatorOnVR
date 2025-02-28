using UnityEngine;

public class CanvasSwitcherExam : MonoBehaviour
{
    [Header("Canvas ของแต่ละ EX")]
    public GameObject ShowDetailExam;

    public GameObject Exam;

    public GameObject Showsavegame;

    public void ResetAllPage()
    {
        ShowDetailExam.SetActive(false);

        Exam.SetActive(false);
         Showsavegame.SetActive(false);
    }
    // เรียกเมื่อกดปุ่ม EX1
    public void ShowDetailExamPage()
    {   ResetAllPage();
        ShowDetailExam.SetActive(true);
    }
  
    public void examPage()
    {   ResetAllPage();
        Exam.SetActive(true);
    }

 public void ShowsaveGame()
    {   ResetAllPage();
        Showsavegame.SetActive(true);
    }
    
}