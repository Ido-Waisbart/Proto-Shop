using UnityEngine;

public class GeneralDialogCaller : MonoBehaviour, IDialogCaller
{
    public void CallDialog(DialogType dialogType)
    {
        DialogManager.Instance.CallDialog(this, dialogType);
    }
    
    public void CallDialog(DialogTypeContainer dialogTypeContainer)
    {
        DialogManager.Instance.CallDialog(this, dialogTypeContainer.DialogType);
    }
}
