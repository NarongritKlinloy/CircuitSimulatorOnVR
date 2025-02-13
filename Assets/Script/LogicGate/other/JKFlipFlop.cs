using UnityEngine;
using System.Collections.Generic;

public class JKFlipFlop : MonoBehaviour
{
    [Header("Input J และ K")]
    public InputConnector J; // อินพุต J
    public InputConnector K; // อินพุต K

    [Header("Clock Signal")]
    public InputConnector clock; // นาฬิกา (Clock Pulse)

    [Header("Output Q และ Q'")]
    public OutputConnector Q;  // เอาต์พุต Q
    public OutputConnector Q_Not; // เอาต์พุต Q'

    private bool lastClockState = false; // เก็บค่าก่อนหน้าของ Clock

    void Update()
    {
        if (clock == null || J == null || K == null || Q == null || Q_Not == null) return;

        bool newQ = Q.isOn; // สำรองค่า Q ก่อนอัปเดต

        // ตรวจจับขอบขาขึ้น (rising edge) ของ Clock
        if (!lastClockState && clock.isOn)
        {
            if (!J.isOn && !K.isOn)
            {
                // J = 0, K = 0 → Hold State (คงค่าเดิม)
            }
            else if (!J.isOn && K.isOn)
            {
                // J = 0, K = 1 → Reset (Q = 0, Q' = 1)
                newQ = false;
            }
            else if (J.isOn && !K.isOn)
            {
                // J = 1, K = 0 → Set (Q = 1, Q' = 0)
                newQ = true;
            }
            else if (J.isOn && K.isOn)
            {
                // J = 1, K = 1 → Toggle (กลับค่า Q)
                newQ = !Q.isOn;
            }

            // อัปเดต Q และ Q' หลังจากคำนวณเสร็จ
            Q.isOn = newQ;
            Q_Not.isOn = !newQ;

            Q.UpdateState();
            Q_Not.UpdateState();
        }

        lastClockState = clock.isOn; // บันทึกค่าก่อนหน้าของ Clock
    }
}
