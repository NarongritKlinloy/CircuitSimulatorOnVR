using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    [Header("Canvas ของแต่ละ EX")]
    //public GameObject ex1Canvas;
    public GameObject ex2Canvas;
    public GameObject ex3Canvas;
    public GameObject ShowSaveMode;
    public GameObject ShowButtonPage;

    public void ResetMenu()
    {
        //ex1Canvas.SetActive(false);
        ex2Canvas.SetActive(false);
        ex3Canvas.SetActive(false);
        ShowSaveMode.SetActive(false);
        ShowButtonPage.SetActive(false);

    }

    // เรียกเมื่อกดปุ่ม EX1
    // public void ShowExample1()
    // {
    //     ResetMenu();
    //     //ex1Canvas.SetActive(true);

    // }

    // เรียกเมื่อกดปุ่ม EX2
    public void ShowExample2()
    {
        ResetMenu();
        ex2Canvas.SetActive(true);

    }

    // เรียกเมื่อกดปุ่ม EX3
    public void ShowExample3()
    {
        ResetMenu();
        ex3Canvas.SetActive(true);

    }

    public void ShowsaveMode()
    {
        ResetMenu();
        ShowSaveMode.SetActive(true);
    }

    public void Showbuttonpage()
    {
        ResetMenu();
        ShowButtonPage.SetActive(true);

    }
}