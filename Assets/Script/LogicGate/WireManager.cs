using UnityEngine;
using System.Collections.Generic;

public class WireManager : MonoBehaviour
{
    public GameObject wirePrefab; // Prefab สำหรับสายไฟ
    private OutputConnector firstOutput = null; // เก็บ Output ที่ถูกคลิกตัวแรก
    private Dictionary<(OutputConnector, InputConnector), GameObject> wireConnections = new Dictionary<(OutputConnector, InputConnector), GameObject>(); // เก็บสายไฟที่เชื่อมต่อกัน

    void Update()
    {
        // อัปเดตตำแหน่งของสายไฟให้ติดตาม Input และ Output 
        foreach (var connection in wireConnections)
        {
            OutputConnector start = connection.Key.Item1;
            InputConnector end = connection.Key.Item2;
            GameObject wire = connection.Value;

            if (wire != null && start != null && end != null)
            {
                LineRenderer lineRenderer = wire.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(0, start.outPoint.position); // ใช้ outPoint ของ Output
                    lineRenderer.SetPosition(1, end.inPoint.position); // ใช้ inPoint ของ Input

                    // เปลี่ยนสีของสายไฟตามสถานะ Output
                    Color wireColor = start.isOn ? Color.green : Color.red;
                    lineRenderer.startColor = wireColor;
                    lineRenderer.endColor = wireColor;
                }
            }
        }
    }

    public void SelectOutput(OutputConnector output)
    {
        Debug.Log("SelectOutput called");
        if (firstOutput == null)
        {
            firstOutput = output; // เลือก Output ตัวแรก

        }
    }

    public void SelectInput(InputConnector input)
    {
        if (firstOutput != null)
        {
            // ถ้า Input นี้มีสายไฟอยู่แล้ว ให้ลบก่อน
            RemoveWire(input);
            CreateWire(firstOutput, input);
            firstOutput = null;
        }
    }

    private void CreateWire(OutputConnector output, InputConnector input)
    {
        if (!wireConnections.ContainsKey((output, input)))
        {
            // สร้างเส้นเชื่อมระหว่าง Output และ Input
            GameObject wire = Instantiate(wirePrefab);
            LineRenderer lineRenderer = wire.GetComponent<LineRenderer>();

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, output.outPoint.position);
                lineRenderer.SetPosition(1, input.inPoint.position);

                lineRenderer.startWidth = 0.05f;
                lineRenderer.endWidth = 0.05f;

                Color wireColor = output.isOn ? Color.green : Color.red;
                lineRenderer.startColor = wireColor;
                lineRenderer.endColor = wireColor;
            }

            wireConnections[(output, input)] = wire; // บันทึกสายไฟลง Dictionary
            output.AddConnection(input); // เชื่อมโยงข้อมูล

            // อัปเดตค่าให้ Output และ Input ที่เชื่อมต่อใหม่
            output.UpdateState();
            input.UpdateState();
        }
    }
    public void RemoveWire(InputConnector input)
    {
        List<(OutputConnector, InputConnector)> connectionsToRemove = new List<(OutputConnector, InputConnector)>();

        foreach (var connection in wireConnections)
        {
            if (connection.Key.Item2 == input) // ✅ ใช้ connection.Key อย่างถูกต้อง
            {
                connectionsToRemove.Add(connection.Key);
            }
        }

        foreach (var key in connectionsToRemove)
        {
            OutputConnector output = key.Item1;
            output.RemoveConnection(input);
            input.ClearConnections();
            Destroy(wireConnections[key]);
            wireConnections.Remove(key);
        }

        // ตรวจสอบค่า Input ใหม่หลังจากลบสายไฟ
        input.UpdateState();
    }


}
