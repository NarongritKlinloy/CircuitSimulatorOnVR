using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [Header("เสียงเมื่อวัตถุถูกลบ")]
    public AudioClip deleteSound;  // เสียงที่ต้องการเล่นเมื่อวัตถุถูกลบ
    private AudioSource audioSource; // สำหรับเล่นเสียง

    void Start()
    {
        // ตรวจสอบหรือเพิ่ม AudioSource ให้กับ GameObject นี้
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่า Object ที่ชนมี Tag เป็น "Digital" หรือไม่
        if (other.CompareTag("Digital"))
        {
            // เล่นเสียงก่อนลบวัตถุ (ถ้ามีการตั้งค่า AudioClip ไว้)
            if (deleteSound != null)
            {
                audioSource.PlayOneShot(deleteSound);
            }
            
            Debug.Log("ลบวัตถุ: " + other.gameObject.name);
            Destroy(other.gameObject);
        }
    }
}
