using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Prefab ของอุปกรณ์ต่างๆ")]
    public GameObject andGatePrefab;
    public GameObject orGatePrefab;
    public GameObject norGatePrefab;
    public GameObject nandGatePrefab;
    public GameObject xorGatePrefab;
    public GameObject xnorGatePrefab;
    public GameObject notGatePrefab;
    public GameObject sevenSegmentPrefab;
    public GameObject toggleSwitchPrefab;
    public GameObject binarySwitchPrefab;
    public GameObject buzzerPrefab;
    public GameObject clockPrefab;
    public GameObject jkFlipFlopPrefab;
    public GameObject ledPrefab;

    [Header("ตำแหน่งเกิดของ Object (แยกตามประเภท)")]
    public Transform andGateSpawnPoint;
    public Transform orGateSpawnPoint;
    public Transform norGateSpawnPoint;
    public Transform nandGateSpawnPoint;
    public Transform xorGateSpawnPoint;
    public Transform xnorGateSpawnPoint;
    public Transform notGateSpawnPoint;
    public Transform sevenSegmentSpawnPoint;
    public Transform toggleSwitchSpawnPoint;
    public Transform binarySwitchSpawnPoint;
    public Transform buzzerSpawnPoint;
    public Transform clockSpawnPoint;
    public Transform jkFlipFlopSpawnPoint;
    public Transform ledSpawnPoint;

    // เก็บตัวนับ ID ของแต่ละประเภท
    private Dictionary<string, int> objectCount = new Dictionary<string, int>();

    private void Spawn(GameObject prefab, Transform spawnPoint, string baseName)
    {
        if (prefab != null && spawnPoint != null)
        {
            GameObject newObj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            // เช็คว่า baseName มีการใช้หรือยัง ถ้ายังให้กำหนดค่าเริ่มต้นเป็น 1
            if (!objectCount.ContainsKey(baseName))
            {
                objectCount[baseName] = 1;
            }

            // ตั้งชื่อให้กับ Object ที่ Spawn ออกมา
            newObj.name = $"{baseName}_{objectCount[baseName]}";
            objectCount[baseName]++; // เพิ่ม ID เพื่อป้องกันชื่อซ้ำ
        }
        else
        {
            Debug.LogWarning($"Prefab หรือ Spawn Point ของ {baseName} ยังไม่ได้ตั้งค่า!");
        }
    }

    public void SpawnAndGate() { Spawn(andGatePrefab, andGateSpawnPoint, "AndGate"); }
    public void SpawnOrGate() { Spawn(orGatePrefab, orGateSpawnPoint, "OrGate"); }
    public void SpawnNorGate() { Spawn(norGatePrefab, norGateSpawnPoint, "NorGate"); }
    public void SpawnNandGate() { Spawn(nandGatePrefab, nandGateSpawnPoint, "NandGate"); }
    public void SpawnXorGate() { Spawn(xorGatePrefab, xorGateSpawnPoint, "XorGate"); }
    public void SpawnXnorGate() { Spawn(xnorGatePrefab, xnorGateSpawnPoint, "XnorGate"); }
    public void SpawnNotGate() { Spawn(notGatePrefab, notGateSpawnPoint, "NotGate"); }
    public void SpawnSevenSegment() { Spawn(sevenSegmentPrefab, sevenSegmentSpawnPoint, "SevenSegment"); }
    public void SpawnToggleSwitch() { Spawn(toggleSwitchPrefab, toggleSwitchSpawnPoint, "ToggleSwitch"); }
    public void SpawnBinarySwitch() { Spawn(binarySwitchPrefab, binarySwitchSpawnPoint, "BinarySwitch"); }
    public void SpawnBuzzer() { Spawn(buzzerPrefab, buzzerSpawnPoint, "Buzzer"); }
    public void SpawnClock() { Spawn(clockPrefab, clockSpawnPoint, "Clock"); }
    public void SpawnJKFlipFlop() { Spawn(jkFlipFlopPrefab, jkFlipFlopSpawnPoint, "JKFlipFlop"); }
    public void SpawnLED() { Spawn(ledPrefab, ledSpawnPoint, "LED"); }
}
