using UnityEngine;

public class ButtonAnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnButtonPressed()
    {
        animator.SetBool("isPressed", true);
        Invoke("ResetPressedState", 0.5f); // หน่วงเวลาแล้วกลับไป Normal
    }

    private void ResetPressedState()
    {
        animator.SetBool("isPressed", false);
    }
}
