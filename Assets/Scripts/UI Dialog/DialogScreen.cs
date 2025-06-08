using System;
using UnityEngine;

// Specific to Tiny Shop.
// NOTE: Whenever you add an item to this, DialogManager.OnItemPanelChosen() also needs to be adjusted.
[Serializable]
public enum DialogType{
    ChooseItem,
    CraftItem,
    // ShowInventory,
    ShowMarket,
}

public class DialogScreen : MonoBehaviour
{
    [SerializeField] DialogType DialogType;

    public void OnBtnCancel(){
        DialogManager.Instance.OnCancelDialog(this);
    }
}
