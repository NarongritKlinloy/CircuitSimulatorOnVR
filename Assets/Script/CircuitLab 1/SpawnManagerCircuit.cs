using UnityEngine;

public class SpawnManagerCircuit : MonoBehaviour
{
    [Header("Prefab ของอุปกรณ์ต่างๆ")]
    public GameObject resistor;
    public GameObject bulb;
    public GameObject motor;
    public GameObject wire;
    public GameObject swich;
    public GameObject battery;
    public GameObject balloon;
    public GameObject timer;
    public GameObject flute;
    public GameObject button;
    public GameObject solar; // แก้ชื่อจาก "solor" เป็น "solar"

    [Header("ตำแหน่งเกิดของ Object")]
    public Transform spawnPoint;

    private void Spawn(GameObject prefab)
    {
        if (prefab != null && spawnPoint != null)
        {
            Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Prefab หรือ Spawn Point ยังไม่ได้ตั้งค่า!");
        }
    }

    // แก้ชื่อฟังก์ชันให้ตรงกับอุปกรณ์ที่ใช้งาน
    public void SpawnResistor() { Spawn(resistor); }
    public void SpawnBulb() { Spawn(bulb); }
    public void SpawnMotor() { Spawn(motor); }
    public void SpawnWire() { Spawn(wire); }
    public void SpawnSwitch() { Spawn(swich); }
    public void SpawnBattery() { Spawn(battery); }
    public void SpawnBalloon() { Spawn(balloon); }
    public void SpawnTimer() { Spawn(timer); }
    public void SpawnFlute() { Spawn(flute); }
    public void SpawnButton() { Spawn(button); }
    public void SpawnSolar() { Spawn(solar); }
}
