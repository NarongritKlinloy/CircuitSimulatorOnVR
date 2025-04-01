using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class VRResetPosition : MonoBehaviour
{
    public Transform objectToReset; // วัตถุที่ต้องการรีเซ็ต
        private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // บันทึกตำแหน่งและการหมุนเริ่มต้น
        if (objectToReset != null)
        {
            initialPosition = objectToReset.position;
            initialRotation = objectToReset.rotation;
            
        }
    }

    public void ResetPosition()
    {
        if (objectToReset != null)
        {
            objectToReset.position = initialPosition;
            objectToReset.rotation = initialRotation;
            
        }
    }
}