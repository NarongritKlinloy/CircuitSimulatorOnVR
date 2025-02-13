using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlot : MonoBehaviour
{
    public TMP_Text saveText;
    private int slotID;
    private bool hasData;
    private SaveLoadUI saveLoadUI;

    public void SetData(int id, bool hasSave, SaveLoadUI ui)
    {
        slotID = id;
        hasData = hasSave;
        saveLoadUI = ui;
        saveText.text = hasSave ? $"Slot {id}: มีข้อมูล" : $"Slot {id}: ว่างเปล่า";
    }

    public void OnSaveButtonClick()
    {
        if (hasData)
        {
            saveLoadUI.ShowConfirmation($"มีข้อมูลอยู่แล้ว ต้องการแทนที่หรือไม่?", this);
        }
        else
        {
            saveLoadUI.ShowConfirmation($"ต้องการบันทึกข้อมูลใน Slot {slotID} หรือไม่?", this);
        }
    }

    public void ConfirmSave()
    {
        saveLoadUI.SaveGame(slotID);
    }

    public void OnLoadButtonClick()
    {
        saveLoadUI.LoadGame(slotID);
    }
}
