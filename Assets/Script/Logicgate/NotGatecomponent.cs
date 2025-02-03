using UnityEngine;

public class NotGatecomponent : MonoBehaviour
{
    public GameObject inputObject;  // วัตถุที่ใช้เป็น input
    public GameObject outputObject; // วัตถุที่ใช้เป็น output
    private bool inputState = false; // ค่าของ input
    private bool outputState = false; // ค่าของ output
    private Renderer outputRenderer;

    void Start()
    {
        if (outputObject != null)
        {
            outputRenderer = outputObject.GetComponent<Renderer>();
            UpdateOutput(); // อัปเดตค่าเริ่มต้นของ output
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == inputObject) // ตรวจสอบว่าชนกับ inputObject เท่านั้น
        {
            Debug.Log("ชนกับ inputObject: " + other.gameObject.name);
            InputTrigger(other.gameObject);
        }
    }
   
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == inputObject)
        {
            Debug.Log("ออกจากการชน inputObject: " + other.gameObject.name);
            inputState = false; // เมื่อออกจากการชน ค่า input เป็น false
            outputState = true; // NOT Gate: Output ต้องเป็นตรงข้าม
            UpdateOutput();
        }
    }

    void InputTrigger(GameObject inputObj)
    {
        LogicInput logicInput = inputObj.GetComponent<LogicInput>();
        if (logicInput != null)
        {
            inputState = logicInput.inputState; // รับค่าจาก input
            outputState = !inputState; // NOT Gate: ค่าของ output เป็นตรงข้ามกับ input
            Debug.Log($"InputState: {inputState}, OutputState: {outputState}");
            UpdateOutput();
        }
    }

    void UpdateOutput()
    {
        if (outputRenderer != null)
        {
            outputRenderer.material.color = outputState ? Color.green : Color.red; // เปลี่ยนสีตามค่า true/false
        }
    }
}

public class LogicInput : MonoBehaviour
{
    public bool inputState = false; // ค่าของ input
}
