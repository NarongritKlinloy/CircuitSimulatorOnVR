using UnityEngine;

public class LED : MonoBehaviour
{
    [Header("Input ที่ใช้ควบคุม LED")]
    public InputConnector input;  // รับค่าจาก InputConnector

    [Header("วัตถุที่ต้องการควบคุมสีเพิ่มเติม")]
    public GameObject targetObject; // วัตถุที่ต้องการให้เปลี่ยนสีตาม LED

    private Renderer ledRenderer;
    private Renderer targetRenderer;
    private Light targetLight; // เพิ่ม Light สำหรับแสดงแสง
    private Material targetMaterial; // เก็บ Material ของ targetObject

    void Start()
    {
        ledRenderer = GetComponent<Renderer>();
        
        if (targetObject != null)
        {
            targetRenderer = targetObject.GetComponent<Renderer>();
            targetMaterial = targetRenderer.material; // ดึง Material มาใช้

            targetLight = targetObject.GetComponent<Light>();

            // ถ้า targetObject ไม่มี Light ให้เพิ่มเข้าไป
            if (targetLight == null)
            {
                targetLight = targetObject.AddComponent<Light>();
                targetLight.type = LightType.Point; // ใช้แสงแบบ Point Light
                targetLight.range = 2.5f; // กำหนดระยะของแสง
                targetLight.intensity = 0f; // เริ่มต้นที่ 0 (ปิดแสง)
                targetLight.color = Color.red; // ตั้งค่าให้แสงเป็นสีแดง
            }
        }

        UpdateState();
    }

    void Update()
    {
        UpdateState();
    }

    public void UpdateState()
    {
        if (input != null)
        {
            bool isActive = input.isOn;

            // เปลี่ยนสีของ LED
            if (ledRenderer != null)
            {
                ledRenderer.material.color = isActive ? Color.red : Color.gray;
            }

            // เปลี่ยนสีของ targetObject และเพิ่ม Emission ให้เรืองแสง
            if (targetRenderer != null)
            {
                targetRenderer.material.color = isActive ? Color.red : Color.gray;

                if (targetMaterial != null)
                {
                    if (isActive)
                    {
                        targetMaterial.EnableKeyword("_EMISSION");
                        targetMaterial.SetColor("_EmissionColor", Color.red * 2f); // เพิ่มความเข้มของ Emission
                    }
                    else
                    {
                        targetMaterial.DisableKeyword("_EMISSION");
                    }
                }
            }

            // ควบคุมแสงของ targetObject
            if (targetLight != null)
            {
                targetLight.intensity = isActive ? 5f : 0f; // ปรับความสว่างของแสง
            }
        }
    }
}
