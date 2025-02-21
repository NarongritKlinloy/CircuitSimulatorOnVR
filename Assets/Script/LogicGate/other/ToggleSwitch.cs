using UnityEngine;

public class ToggleSwitch : MonoBehaviour
{
    [Header("สถานะของ Switch (true = ON, false = OFF)")]
    public bool isOn = false; // ค่าเริ่มต้นของ Switch
    public OutputConnector output; // OutputConnector ที่ส่งค่าจากสวิตช์
    public GameObject pivot; // วัตถุที่ใช้หมุน (เช่น Rocker)

    [Header("เสียง")]
    public AudioClip toggleSound;  // เสียงที่เล่นเมื่อเปลี่ยนสถานะ
    [Range(0f, 2f)]
    public float toggleVolume = 1f; // ความดังเสียง (Volume Scale) สามารถปรับได้
    private AudioSource audioSource; // สำหรับเล่นเสียง

    private void Start()
    {
        // ตรวจสอบหรือเพิ่ม AudioSource ให้กับ GameObject นี้
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (output == null)
        {
            output = gameObject.AddComponent<OutputConnector>();
        }

        if (output != null)
        {
            output.isOn = isOn;
            output.UpdateState();

            // ตั้งชื่อให้ Output
            output.gameObject.name = $"{gameObject.name}_OUT";
        }

        UpdatePivotRotation();
    }

    private void OnMouseDown()
    {
        Toggle(); // เรียกใช้ Toggle() เมื่อคลิกด้วยเมาส์
    }

    public void Toggle()
    {
        isOn = !isOn; // สลับค่า (Toggle)

        // เล่นเสียงเมื่อเปลี่ยนสถานะ (ถ้ามีการตั้งค่า)
        if (toggleSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(toggleSound, toggleVolume);
        }

        if (output != null)
        {
            output.isOn = isOn;
            output.UpdateState(); // อัปเดตค่าทุกจุดที่เชื่อมต่อ
        }
        else
        {
            Debug.Log($"⚠️ ToggleSwitch {gameObject.name} ไม่มี Output ที่เชื่อมต่อ");
        }

        UpdatePivotRotation(); // อัปเดตการหมุนของ pivot ทันที
    }

    private void UpdatePivotRotation()
    {
        if (pivot != null)
        {
            var rotation = pivot.transform.localEulerAngles;
            rotation.y = isOn ? 15f : -15f; // หมุน Rocker ตามสถานะ
            pivot.transform.localEulerAngles = rotation;
        }
    }
}
