using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    [Header("Canvas ของแต่ละ EX")]
    public GameObject ex1Canvas;
    public GameObject ex2Canvas;
    public GameObject ex3Canvas;
    public GameObject ex4Canvas;

    [Header("Prefab ที่ใช้ Spawn")]
    public GameObject ex1Prefab;
    public GameObject ex2Prefab;
    public GameObject ex3Prefab;
    public GameObject ex4Prefab;

    [Header("ตำแหน่งที่ต้องการ Spawn")]
    public Transform spawnPoint;

    private GameObject currentSpawnedObject; // เก็บ Object ที่ Spawn ปัจจุบัน

    private void Spawn(GameObject prefab)
    {
        // ลบ Object ที่ Spawn มาก่อนหน้า ถ้ามี
        if (currentSpawnedObject != null)
        {
            Destroy(currentSpawnedObject);
        }

        // สร้าง Object ใหม่และกำหนดให้เป็น Object ที่ Spawn ล่าสุด
        if (prefab != null && spawnPoint != null)
        {
            currentSpawnedObject = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            currentSpawnedObject.tag = "Exam"; // กำหนด Tag ให้เป็น "Exam"
        }
    }

    // เรียกเมื่อกดปุ่ม EX1
    public void OnClickEx1()
    {
        ex1Canvas.SetActive(true);
        ex2Canvas.SetActive(false);
        ex3Canvas.SetActive(false);
        ex4Canvas.SetActive(false);

        Spawn(ex1Prefab); // Spawn Object สำหรับ EX1
    }

    // เรียกเมื่อกดปุ่ม EX2
    public void OnClickEx2()
    {
        ex1Canvas.SetActive(false);
        ex2Canvas.SetActive(true);
        ex3Canvas.SetActive(false);
        ex4Canvas.SetActive(false);

        Spawn(ex2Prefab); // Spawn Object สำหรับ EX2
    }

    // เรียกเมื่อกดปุ่ม EX3
    public void OnClickEx3()
    {
        ex1Canvas.SetActive(false);
        ex2Canvas.SetActive(false);
        ex3Canvas.SetActive(true);
        ex4Canvas.SetActive(false);

        Spawn(ex3Prefab); // Spawn Object สำหรับ EX3
    }

    // เรียกเมื่อกดปุ่ม EX4
    public void OnClickEx4()
    {
        ex1Canvas.SetActive(false);
        ex2Canvas.SetActive(false);
        ex3Canvas.SetActive(false);
        ex4Canvas.SetActive(true);

        Spawn(ex4Prefab); // Spawn Object สำหรับ EX4
    }
}
