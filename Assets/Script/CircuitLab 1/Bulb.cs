using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Bulb : CircuitComponent, IResistor
{
    // Public members set in Unity Object Inspector
    public GameObject labelResistance;
    public TMP_Text labelResistanceText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;
    public GameObject filament;
    public AudioSource colorChangeAudio;

    float intensity = 0f;
    public GameObject explosionEffectPrefab;  // เพิ่มตัวแปรสำหรับเก็บเอฟเฟกต์ระเบิด
    bool cooldownActive = false;
    Color[] colors = { Color.red, Color.yellow, Color.green, Color.blue, Color.magenta };
    float[] resistances = { 50f, 100f, 150f, 200f, 250f }; // Resistance values for each color

    int emissionColorIdx = 1;

    public float Resistance { get; private set; }

    public Bulb()
    {
        Resistance = resistances[emissionColorIdx]; // Set initial resistance based on default color
    }

    protected override void Update()
    {
        if (this == null || filament == null) return;

        // Show/hide the labels
        labelResistance.gameObject.SetActive(IsActive && IsCurrentSignificant() && Lab.showLabels);
        labelCurrent.gameObject.SetActive(IsActive && IsCurrentSignificant() && Lab.showLabels);
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;

        if (!isActive)
            DeactivateLight();

        labelResistanceText.text = Resistance.ToString("0.#") + "Ω";
        RotateLabel(labelResistance, LabelAlignment.Top);
        RotateLabel(labelCurrent, LabelAlignment.Bottom);
    }

    private void DeactivateLight()
    {
        filament.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
    }

    private void ActivateLight()
    {
        filament.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        Color baseColor = colors[emissionColorIdx];
        Color finalColor = baseColor * Mathf.Pow(2, intensity);
        filament.GetComponent<Renderer>().material.SetColor("_EmissionColor", finalColor);
    }



    public override void SetCurrent(double current)
    {
        if (this == null || filament == null) return;

        Current = current;

        if (current * 1000 > 30)
        {
            TriggerExplosion();
            return;
        }

        if (!IsCurrentSignificant())
        {
            IsActive = false;
            DeactivateLight();
        }
        else
        {
            labelCurrentText.text = (current * 1000f).ToString("0.#") + "mA";
            float maxCurrent = 0.01f;
            float maxIntensity = 5.0f;
            float minIntensity = 3.0f;
            float pctCurrent = ((float)current > maxCurrent ? maxCurrent : (float)current) / maxCurrent;
            intensity = (pctCurrent * (maxIntensity - minIntensity)) + minIntensity;

            ActivateLight();
        }
    }

    private void TriggerExplosion()
    {
        if (filament != null)
        {
            Debug.Log("Bulb exploded due to high current!");

            filament.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red * 5.0f);

            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }

            StopAllCircuits();

            // ตรวจสอบออบเจ็กต์ก่อนทำลายเพื่อป้องกัน MissingReferenceException
            //if (gameObject != null)
            //{
            //Destroy(gameObject);
            // }
        }
    }


    private void StopAllCircuits()
    {
        Debug.Log("Stopping all circuits due to explosion.");
        CircuitLab circuitLab = FindObjectOfType<CircuitLab>();
        if (circuitLab != null)
        {
            foreach (var component in circuitLab.GetComponentsInChildren<CircuitComponent>())
            {
                if (component != null)
                {
                    component.SetActive(false, false);
                }
            }
        }
    }

    // ฟังก์ชันสำหรับแสดงเอฟเฟกต์การระเบิด
    private IEnumerator ExplosionEffect()
    {
        // ทำให้หลอดไฟกระพริบก่อนระเบิด
        for (int i = 0; i < 5; i++)
        {
            filament.SetActive(!filament.activeSelf);
            yield return new WaitForSeconds(0.1f);
        }

        // ปิดการแสดงผลของหลอดไฟ
        filament.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (!cooldownActive && IsActive &&
            other.gameObject.name.Contains("Pinch"))
        {
            // Switch the emission color to the next one in the list
            emissionColorIdx = ++emissionColorIdx % colors.Length;

            // Update resistance based on the new color index
            Resistance = resistances[emissionColorIdx];

            // Update the label to reflect the new resistance value
            labelResistanceText.text = Resistance.ToString("0.#") + "Ω";

            ActivateLight();

            StartCoroutine(PlaySound(colorChangeAudio, 0f));

            cooldownActive = true;
            Invoke("Cooldown", 0.5f);
        }
    }

    void Cooldown()
    {
        cooldownActive = false;
    }
}
