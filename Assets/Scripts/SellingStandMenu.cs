// ASSUMPTION: You cannot click outside the menu's region to hide the menu.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO OPTIONAL: Will there be a need for several dialogs to be plugged in?
//  A menu may connect not only to a item selection dialog,
//      but possibly, a confirmation dialog for removing the current item.
public class SellingStandMenu : MonoBehaviour, IDialogCaller
{
    [SerializeField, ReorderableList] private List<GameObject> buttons; 
    [SerializeField, NotNull] private SellingStand sellingStand; 
    // [SerializeField, NotNull] GameObject goodSelectionDialog; 

    private bool _isActive = false;

    // TODO OPTIONAL: Should this be removed in favour of the next InventorySlotData struct?
    [Serializable]
    public struct GoodSelectionDialog_OptionProps{
        public ItemData ItemData;
        public int Quantity;
    }

    public void SetVisible(bool visibility){
        StartCoroutine(ISetVisibility(visibility));
    }

    public void ToggleVisiblity(){
        SetVisible(!_isActive);
    }

    private IEnumerator ISetVisibility(bool visibility){
        yield return null;
        foreach(GameObject buttonGO in buttons){
            buttonGO.SetActive(visibility);
        }
        _isActive = visibility;
    }

    public void OnBtnChooseGoodsToPlace(){
        // SetVisible(false);
        CallDialog(DialogType.ChooseItem);
    }

    public void OnBtnRemovePlacedGoods(){
        SetVisible(false);
        sellingStand.OnPlayerEmptyGoods();
    }

    public void MakeChoiceInGoodSelectionDialog(GoodSelectionDialog_OptionProps props){
        print($"Selected good: `{props.ItemData}` x{props.Quantity}");
        sellingStand.OnPlayerPlacedGoods(props.ItemData, props.Quantity);
        SetVisible(false);
    }

    public void CallDialog(DialogType dialogType)
    {
        DialogManager.Instance.CallDialog(this, dialogType);
    }
}
