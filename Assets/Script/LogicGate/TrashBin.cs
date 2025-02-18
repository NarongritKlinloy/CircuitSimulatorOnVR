using UnityEngine;

public class TrashBin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่า Object ที่ชนมี Tag เป็น "Digital" หรือไม่
        if (other.CompareTag("Digital"))
        {
            Destroy(other.gameObject); // ทำลาย Object
            Debug.Log("ลบวัตถุ: " + other.gameObject.name);
        }
    }
}
