using UnityEngine;
using System.Collections.Generic;

public class WireManager : MonoBehaviour
{
    public GameObject wirePrefab; // Prefab สำหรับสายไฟ

    // เพิ่มตัวแปรสำหรับเสียง
    [Header("เสียง")]
    public AudioClip selectSound;  // เสียงเมื่อเลือกสายไฟ (Select)
    public AudioClip removeSound;  // เสียงเมื่อสายไฟถูกลบ (Remove)
    private AudioSource audioSource; // สำหรับเล่นเสียง

    private OutputConnector firstOutput = null; // เก็บ Output ที่ถูกคลิกตัวแรก
    private Dictionary<(OutputConnector, InputConnector), GameObject> wireConnections = new Dictionary<(OutputConnector, InputConnector), GameObject>(); // เก็บสายไฟที่เชื่อมต่อกัน

    void Start()
    {
        // ตรวจสอบหรือเพิ่ม AudioSource ให้กับ GameObject นี้
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // อัปเดตตำแหน่งและสีของสายไฟ
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
                    lineRenderer.SetPosition(0, start.outPoint.position);
                    lineRenderer.SetPosition(1, end.inPoint.position);
                    lineRenderer.startColor = start.isOn ? Color.red : Color.black;
                    lineRenderer.endColor = end.isOn ? Color.red : Color.black;
                }
            }
        }

        // ตรวจสอบและลบสายไฟที่เชื่อมต่อกับอุปกรณ์ที่ถูกทำลายแล้ว (null)
        List<(OutputConnector, InputConnector)> connectionsToRemove = new List<(OutputConnector, InputConnector)>();
        foreach (var connection in wireConnections)
        {
            // Unity จะคืนค่าเป็น null หาก GameObject ถูกทำลายแล้ว
            if (connection.Key.Item1 == null || connection.Key.Item2 == null)
            {
                connectionsToRemove.Add(connection.Key);
            }
        }
        foreach (var key in connectionsToRemove)
        {
            if (wireConnections[key] != null)
            {
                // เล่นเสียงลบสายไฟก่อนทำลาย (ถ้ามี)
                if (removeSound != null)
                {
                    audioSource.PlayOneShot(removeSound);
                }
                Destroy(wireConnections[key]);
            }
            // อัปเดตสถานะของ InputConnector ที่ยังมีอยู่
            if (key.Item2 != null)
            {
                // ลบการเชื่อมต่อที่อ้างอิงถึง Output ที่ถูกทำลาย
                key.Item2.RemoveConnection(key.Item1);
                // เรียก UpdateState() เพื่อให้ InputConnector รีเซ็ตค่าเป็น false หากไม่มีการเชื่อมต่อ
                key.Item2.UpdateState();
            }
            wireConnections.Remove(key);
        }
    }

    public void SelectOutput(OutputConnector output)
    {
        Debug.Log("SelectOutput called");
        if (firstOutput == null)
        {
            firstOutput = output;
            // เล่นเสียงเมื่อเลือก Output
            if (selectSound != null)
            {
                audioSource.PlayOneShot(selectSound);
            }
        }
    }

    public void SelectInput(InputConnector input)
    {
        if (firstOutput != null)
        {
            RemoveWire(input);
            CreateWire(firstOutput, input);
            // เล่นเสียงเลือก (Select) อีกครั้งเมื่อเชื่อมต่อเสร็จ
            if (selectSound != null)
            {
                audioSource.PlayOneShot(selectSound);
            }
            firstOutput = null;
        }
    }

    public Dictionary<(OutputConnector, InputConnector), GameObject> GetWireConnections()
    {
        return wireConnections;
    }

    private void CreateWire(OutputConnector output, InputConnector input)
    {
        if (!wireConnections.ContainsKey((output, input)))
        {
            GameObject wire = Instantiate(wirePrefab);
            LineRenderer lineRenderer = wire.GetComponent<LineRenderer>();

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, output.outPoint.position);
                lineRenderer.SetPosition(1, input.inPoint.position);
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;

                Color wireColor = output.isOn ? Color.green : Color.red;
                lineRenderer.startColor = wireColor;
                lineRenderer.endColor = wireColor;
            }

            wireConnections[(output, input)] = wire;
            output.AddConnection(input);

            output.UpdateState();
            input.UpdateState();
        }
    }

    public void UpdateWireColor(OutputConnector output)
    {
        foreach (var connection in wireConnections)
        {
            if (connection.Key.Item1 == output)
            {
                LineRenderer lineRenderer = connection.Value.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    Color wireColor = output.isOn ? Color.green : Color.red;
                    lineRenderer.startColor = wireColor;
                    lineRenderer.endColor = wireColor;
                }
            }
        }
    }

    public void RemoveWire(InputConnector input)
    {
        List<(OutputConnector, InputConnector)> connectionsToRemove = new List<(OutputConnector, InputConnector)>();

        foreach (var connection in wireConnections)
        {
            if (connection.Key.Item2 == input)
            {
                connectionsToRemove.Add(connection.Key);
            }
        }

        foreach (var key in connectionsToRemove)
        {
            OutputConnector output = key.Item1;
            if (output != null)
            {
                output.RemoveConnection(input);
            }
            input.ClearConnections();
            if (wireConnections[key] != null)
            {
                // เล่นเสียงลบสายไฟก่อนทำลาย (ถ้ามี)
                if (removeSound != null)
                {
                    audioSource.PlayOneShot(removeSound);
                }
                Destroy(wireConnections[key]);
            }
            wireConnections.Remove(key);
        }

        input.UpdateState();
    }
}
