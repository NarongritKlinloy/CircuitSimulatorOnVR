using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public bool stateOutput = true; // กำหนดค่าถูกต้องให้ตัวแปร

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Sending {stateOutput} to {other.gameObject.name}");
        other.gameObject.SendMessage("ReceiveTrigger", stateOutput, SendMessageOptions.DontRequireReceiver);
    }
}
