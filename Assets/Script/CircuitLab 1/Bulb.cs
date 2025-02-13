using System.Collections;
using UnityEngine;
using TMPro;

public class Bulb : CircuitComponent, IResistor
{
    [Header("UI สำหรับการตั้งค่า")]
    public GameObject labelResistance;
    public TMP_Text labelResistanceText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;
    public GameObject filament;
    public AudioSource colorChangeAudio;

    public Light bulbLight;

    private bool cooldownActive = false;
    private bool canReuse = true; // ✅ หลอดไฟใช้ได้ตอนเริ่มต้น
    private float intensity = 0f;

    private Color[] colors = { Color.red, Color.yellow, Color.green, Color.blue, Color.magenta };
    private float[] resistances = { 50f, 100f, 150f, 200f, 250f };

    private int emissionColorIdx = 1;

    public double Resistance { get; private set; }

    public Bulb()
    {
        Resistance = resistances[emissionColorIdx];
    }

    protected override void Update()
    {
        if (this == null || filament == null) return;

        labelResistance.gameObject.SetActive(IsActive && IsCurrentSignificant() && Lab.showLabels);
        labelCurrent.gameObject.SetActive(IsActive && IsCurrentSignificant() && Lab.showLabels);
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        if (!canReuse) return; // ✅ ป้องกันเปิดใหม่ถ้าหลอดขาด

        IsActive = isActive;

        if (!isActive)
            DeactivateLight();

        labelResistanceText.text = Resistance.ToString("0.#") + "Ω";
        RotateLabel(labelResistance, LabelAlignment.Top);
        RotateLabel(labelCurrent, LabelAlignment.Bottom);
    }

    private void DeactivateLight()
    {
        if (filament != null)
        {
            filament.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            if (bulbLight != null)
            {
                bulbLight.enabled = false;
            }
        }
    }

    private void ActivateLight()
    {
        if (!canReuse) return; // ✅ ป้องกันเปิดใหม่ถ้าหลอดขาด

        if (filament != null)
        {
            Renderer filamentRenderer = filament.GetComponent<Renderer>();
            Material filamentMaterial = filamentRenderer.material;

            filamentMaterial.EnableKeyword("_EMISSION");
            Color baseColor = colors[emissionColorIdx];
            Color finalColor = baseColor * Mathf.Pow(2, intensity);

            filamentMaterial.SetColor("_EmissionColor", finalColor);
            filamentMaterial.SetFloat("_EmissionScale", 5.0f);

            if (bulbLight != null)
            {
                bulbLight.enabled = true;
                bulbLight.color = baseColor;
                bulbLight.intensity = 4f;
                bulbLight.range = 1.5f;
                bulbLight.bounceIntensity = 1.5f;
            }
        }
    }

    public override void SetCurrent(double current)
    {
        if (!canReuse) 
        {
            Current = 0; // ✅ หลอดขาด กระแสเป็น 0 เสมอ
            if (labelCurrentText != null)
            {
                labelCurrentText.text = "0.0 mA";
            }
            return;
        }

        if (this == null || filament == null) return;

        Current = current;

        if (current * 1000 > 30)
        {
            StartCoroutine(TriggerExplosion());
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

    private IEnumerator TriggerExplosion()
    {
        if (filament == null) yield break;

        Debug.Log("🔥 Bulb exploded and is now broken!");

        // ✅ กระพริบหลอดไฟเร็วขึ้นก่อนดับ
        for (int i = 0; i < 3; i++)
        {
            filament.SetActive(!filament.activeSelf);
            yield return new WaitForSeconds(0.1f);
        }

        // ✅ เปลี่ยนสีไส้หลอดไฟให้เป็นดำ (ไหม้)
        filament.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
        if (bulbLight != null)
        {
            bulbLight.enabled = false; // ✅ ปิดไฟ
        }

        // ✅ ปิดการทำงานของหลอดไฟทั้งหมด
        IsActive = false;
        canReuse = false; // ✅ หลอดขาด ไม่สามารถใช้งานต่อได้
        Resistance = double.PositiveInfinity; // ✅ หลอดขาด → ความต้านทานเป็นอนันต์ (กระแสเป็น 0)
        Current = 0;

        if (labelCurrentText != null)
        {
            labelCurrentText.text = "0.0 mA"; // ✅ แสดงว่ากระแสเป็น 0
        }

        StopAllCircuits();
    }

    private void StopAllCircuits()
    {
        Debug.Log("⚠️ Stopping all circuits due to explosion.");
        CircuitLab circuitLab = FindObjectOfType<CircuitLab>();
        if (circuitLab != null)
        {
            foreach (var component in circuitLab.GetComponentsInChildren<CircuitComponent>())
            {
                if (component != null)
                {
                    component.SetCurrent(0); // ✅ ตัดกระแสทุกคอมโพเนนต์
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!cooldownActive && IsActive && canReuse &&
            other.gameObject.name.Contains("Pinch"))
        {
            emissionColorIdx = ++emissionColorIdx % colors.Length;
            Resistance = resistances[emissionColorIdx];

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
