using UnityEngine;
using TMPro;

public class ConfirmationDialog : MonoBehaviour
{
    public TMP_Text messageText;
    private SaveSlot currentSlot;

    public void Setup(SaveSlot slot)
    {
        currentSlot = slot;
    }

    public void OnConfirm()
    {
        currentSlot.ConfirmSave();
        gameObject.SetActive(false);
    }

    public void OnCancel()
    {
        gameObject.SetActive(false);
    }
}
