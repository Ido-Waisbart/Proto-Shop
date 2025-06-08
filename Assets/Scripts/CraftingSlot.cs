// Unintuitively, CraftingSlot doesn't extend IDialogCaller. In this case, it uses GeneralDialogCaller.
// ... hey wait. Isn't there a massive issue?
//  How does CraftingManager know which CraftingSlot to fill? Am I going wrong about this??
//  I'm starting to think GeneralDialogCaller, while still being handy to have, is NOT fit to be used here.

//  To test this hypothesis... let's not start crafting immediately...
//  And try crafting an item in different slots.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour, IDialogCaller
{
    [NonSerialized] private CraftingSlotData? craftingSlotData;
    public CraftingSlotData? CraftingSlotData
    {
        get { return craftingSlotData; }
        set
        {
            craftingSlotData = value;
            craftingButton.enabled = IsInactive;
            // craftingButton.enabled = value == null;
        }
    }
    public bool IsInactive => craftingSlotData == null;
    [SerializeField, NotNull] private Image produceImage;
    [SerializeField, NotNull] private Slider progressSlider;
    [SerializeField, NotNull] private TMP_Text amountLeftToProduceText;
    [SerializeField, NotNull] private Button craftingButton;
    private float Progress => craftingSlotData == null ? float.NaN
    : (float)(craftingSlotData?.elapsedTimeInMiliseconds /
        (craftingSlotData?.recipe.craftingTimeInSeconds * 1000));
    
    public void UpdateSlider(){
        progressSlider.value = Progress;
    }
    public bool IsDone => Progress >= 1;

    public void SetImage(Sprite sprite){
        if(sprite == null)
            produceImage.enabled = false;
        else
        {
            produceImage.enabled = true;
            produceImage.sprite = sprite;
        }
    }

    public void SetProgressSliderVisibility(bool visibility){
        progressSlider.gameObject.SetActive(visibility);
    }

    public void SetAmountLeftToProduce(int amount){
        amountLeftToProduceText.text = amount > 1 ? "x" + amount.ToString() : "";
    }

    public void OnBtnStartRecipe(){
        CallDialog(DialogType.CraftItem);
    }

    public void CallDialog(DialogType dialogType)
    {
        DialogManager.Instance.CallDialog(this, dialogType);
    }
}