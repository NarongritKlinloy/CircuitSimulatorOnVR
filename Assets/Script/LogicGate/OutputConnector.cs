using UnityEngine;
using System.Collections.Generic;

public class OutputConnector : MonoBehaviour
{
    [Header("สถานะของ Output (true/false)")]
    public bool isOn = false; // สถานะของ Output (สามารถตั้งค่าใน Inspector)

    public Transform outPoint; // จุดส่งข้อมูล (ต้องตั้งค่าใน Inspector)
    private List<InputConnector> connectedInputs = new List<InputConnector>(); // เก็บ Input ที่เชื่อมต่ออยู่

    private WireManager wireManager;
    private Renderer renderer;
    void Start()
    {
        wireManager = FindObjectOfType<WireManager>();
        renderer = GetComponent<Renderer>();
        UpdateColor();
    }

    //void OnMouseDown()
    //{//
    //if (wireManager != null)
    //{
    //    wireManager.SelectOutput(this);
    //}
    // }
    // ใช้ OnTriggerEnter แทนการคลิกเมาส์
    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collided with: " + other.gameObject.name);

        if (other.gameObject.name.Contains("Pinch"))
        {
            Debug.Log("Pinch detected! Selecting output.");
            if (wireManager != null)
            {
                wireManager.SelectOutput(this);
            }
        }
    }
    void Update()
    {
        // ตรวจสอบตลอดเวลาว่าค่ามีการเปลี่ยนแปลงหรือไม่
        UpdateState();
    }

    public void AddConnection(InputConnector input)
    {
        Debug.Log("Adding connection: " + gameObject.name + " -> " + input.gameObject.name);

        if (!connectedInputs.Contains(input))
        {
            connectedInputs.Add(input);
            input.AddConnection(this);
        }
        UpdateState();
    }
    public void RemoveConnection(InputConnector input)
    {
        if (connectedInputs.Contains(input))
        {
            connectedInputs.Remove(input);
            input.RemoveConnection(this);
        }
        UpdateState();
    }
    public void UpdateState()
    {

        foreach (var input in connectedInputs)
        {
            input.UpdateState(); // ส่งข้อมูลไปยังทุก Input ที่เชื่อมต่อ
        }
        UpdateColor();
    }
    private void UpdateColor()
    {
        if (renderer != null)
        {
            renderer.material.color = isOn ? Color.green : Color.red;
        }
    }
}
