using UnityEngine;

public class Rocker : MonoBehaviour
{
    private bool cooldownActive = false;

    void Start() { }
    void Update() { }

    void OnTriggerEnter(Collider other)
    {
        if (!cooldownActive &&
            other.gameObject.name.Contains("Pinch"))
        {
            // Find our parent and toggle its state
            var script = transform.parent.GetComponent<CircuitComponent>();
            var switchScript = transform.parent.GetComponent<ToggleSwitch>();
            var clock = transform.parent.GetComponent<Clock>();

            if (script != null)
            {
                script.Toggle();

                // Set the rocker to the proper position by rotating the pivot
                var rotation = transform.localEulerAngles;
                rotation.y = -rotation.y;
                transform.localEulerAngles = rotation;
            }
            if (switchScript != null)
            {
                switchScript.Toggle();


            }
            
            cooldownActive = true;
            Invoke("Cooldown", 0.5f);
        }
    }

    void Cooldown()
    {
        cooldownActive = false;
    }
}
