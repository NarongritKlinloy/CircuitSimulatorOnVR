using UnityEngine;

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

    public void SpawnAndGate() { Spawn(andGatePrefab); }
    public void SpawnOrGate() { Spawn(orGatePrefab); }
    public void SpawnNorGate() { Spawn(norGatePrefab); }
    public void SpawnNandGate() { Spawn(nandGatePrefab); }
    public void SpawnXorGate() { Spawn(xorGatePrefab); }
    public void SpawnXnorGate() { Spawn(xnorGatePrefab); }
    public void SpawnNotGate() { Spawn(notGatePrefab); }
    public void SpawnSevenSegment() { Spawn(sevenSegmentPrefab); }
    public void SpawnToggleSwitch() { Spawn(toggleSwitchPrefab); }
    public void SpawnBinarySwitch() { Spawn(binarySwitchPrefab); }
    public void SpawnBuzzer() { Spawn(buzzerPrefab); }
    public void SpawnClock() { Spawn(clockPrefab); }
    public void SpawnJKFlipFlop() { Spawn(jkFlipFlopPrefab); }
    public void SpawnLED() { Spawn(ledPrefab); }
}
