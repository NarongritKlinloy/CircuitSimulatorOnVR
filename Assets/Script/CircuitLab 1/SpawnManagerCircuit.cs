using UnityEngine;
using System.Collections.Generic;

public class SpawnManagerCircuit : MonoBehaviour
{
    public enum ComponentTag { Wire, Battery, Switch, Motor, Bulb, Balloon, Timer, Flute, Button, Solar };

    [Header("Prefab ของอุปกรณ์ต่างๆ")]
    public GameObject wire;
    public GameObject battery;
    public GameObject swich; // Switch
    public GameObject motor;
    public GameObject bulb;
    public GameObject balloon;
    public GameObject timer;
    public GameObject flute;
    public GameObject button;
    public GameObject solar;

    [Header("ตำแหน่งเกิดของ Object แต่ละประเภท")]
    public Transform wireSpawnPoint;
    public Transform batterySpawnPoint;
    public Transform switchSpawnPoint;
    public Transform motorSpawnPoint;
    public Transform bulbSpawnPoint;
    public Transform balloonSpawnPoint;
    public Transform timerSpawnPoint;
    public Transform fluteSpawnPoint;
    public Transform buttonSpawnPoint;
    public Transform solarSpawnPoint;

    [Header("Parent Object สำหรับเก็บอุปกรณ์ที่สร้าง")]
    public Transform parentTransform;

    private int numSpawned = 1; // ใช้สำหรับตั้งชื่อ Component ตามลำดับ

    private Dictionary<ComponentTag, GameObject> componentPrefabs;
    private Dictionary<ComponentTag, Transform> componentSpawnPoints;

    private void Awake()
    {
        componentPrefabs = new Dictionary<ComponentTag, GameObject>
        {
            { ComponentTag.Wire, wire },
            { ComponentTag.Battery, battery },
            { ComponentTag.Switch, swich }, // Switch
            { ComponentTag.Motor, motor },
            { ComponentTag.Bulb, bulb },
            { ComponentTag.Balloon, balloon },
            { ComponentTag.Timer, timer },
            { ComponentTag.Flute, flute },
            { ComponentTag.Button, button },
            { ComponentTag.Solar, solar }
        };

        componentSpawnPoints = new Dictionary<ComponentTag, Transform>
        {
            { ComponentTag.Wire, wireSpawnPoint },
            { ComponentTag.Battery, batterySpawnPoint },
            { ComponentTag.Switch, switchSpawnPoint },
            { ComponentTag.Motor, motorSpawnPoint },
            { ComponentTag.Bulb, bulbSpawnPoint },
            { ComponentTag.Balloon, balloonSpawnPoint },
            { ComponentTag.Timer, timerSpawnPoint },
            { ComponentTag.Flute, fluteSpawnPoint },
            { ComponentTag.Button, buttonSpawnPoint },
            { ComponentTag.Solar, solarSpawnPoint }
        };
    }

    private void Spawn(ComponentTag componentTag)
    {
        if (componentPrefabs.ContainsKey(componentTag) && componentSpawnPoints.ContainsKey(componentTag))
        {
            GameObject prefab = componentPrefabs[componentTag];
            Transform spawnPoint = componentSpawnPoints[componentTag];

            if (prefab != null && spawnPoint != null)
            {
                GameObject spawnedObject = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
                
                // ✅ ตั้งชื่อให้เหมือน Dispenser
                spawnedObject.name = "Component" + componentTag.ToString() + numSpawned++;

                if (parentTransform != null)
                {
                    spawnedObject.transform.SetParent(parentTransform); // กำหนด Parent
                }
            }
            else
            {
                Debug.LogWarning("Prefab หรือ Spawn Point ของ " + componentTag + " ยังไม่ได้ตั้งค่า!");
            }
        }
        else
        {
            Debug.LogWarning("Prefab หรือ Spawn Point ไม่พบใน Dictionary!");
        }
    }

    // 🔥 ปุ่มสำหรับกด Spawn อุปกรณ์แต่ละชนิด
    public void SpawnWire() { Spawn(ComponentTag.Wire); }
    public void SpawnBattery() { Spawn(ComponentTag.Battery); }
    public void SpawnSwitch() { Spawn(ComponentTag.Switch); }
    public void SpawnMotor() { Spawn(ComponentTag.Motor); }
    public void SpawnBulb() { Spawn(ComponentTag.Bulb); }
    public void SpawnBalloon() { Spawn(ComponentTag.Balloon); }
    public void SpawnTimer() { Spawn(ComponentTag.Timer); }
    public void SpawnFlute() { Spawn(ComponentTag.Flute); }
    public void SpawnButton() { Spawn(ComponentTag.Button); }
    public void SpawnSolar() { Spawn(ComponentTag.Solar); }
}
