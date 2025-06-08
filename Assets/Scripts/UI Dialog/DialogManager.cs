using System;
using System.Collections.Generic;
using System.Linq;
using Dev.ComradeVanti.EnumDict;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [SerializeField] private EnumDict<DialogType, DialogScreen> dialogTypeToDialogScreen;

    private IDialogCaller _currentDialogCaller;
    private DialogType _currentlyCalledDialogType;

    [SerializeField, NotNull] private GridLayoutGroup chooseItemDialogGrid;
    [SerializeField, PrefabObjectOnly, NotNull] private UIInventoryItemPanel chooseItemDialogGrid_gridItemPrefab;
    
    [SerializeField, NotNull] private HorizontalLayoutGroup craftingRecipesDialogHBox;
    [SerializeField, NotNull] private RectTransform craftingRecipesDialogHBox_rectTransform;
    [SerializeField, PrefabObjectOnly, NotNull] private RecipePanelButton craftingRecipesDialogHBox_recipePanelPrefab;

    private Dictionary<string, RecipePanelButton> itemNameToRecipePanelDict;

    void Start()
    {
        if(Instance != null){
            throw new Exception();
        }
        Instance = this;

        itemNameToRecipePanelDict = new Dictionary<string, RecipePanelButton>();
    }

    public void CallDialog(IDialogCaller dialogCaller, DialogType dialogType){
        _currentDialogCaller = dialogCaller;
        _currentlyCalledDialogType = dialogType;
        dialogTypeToDialogScreen[dialogType].gameObject.SetActive(true);
    }

    
    // TODO OPTIONAL: Choose different function parameters? DialogScreen might be an imperfect type for the parameters.
    public void OnCancelDialog(DialogScreen dialogScreen){
        _currentDialogCaller = null;
        dialogScreen.gameObject.SetActive(false);
    }

    // This depends on the current dialog type of the calling dialog screen, stored in _dialogType.
    // TODO OPTIONAL: There's definitely a design pattern more befitting for the matching of enum to behaviour/code.
    //      It is likely the Behavior pattern.
    public void OnItemPanelChosen(ItemData itemData, int quantity){
        if(_currentDialogCaller == null){
            throw new NullReferenceException("_currentDialogCaller is null. A screen was likely active, without a dialogue caller actually calling it.");
        }

        switch(_currentlyCalledDialogType){
            case DialogType.ChooseItem:
                // ASSUMPTION: _dialogCaller has to be a SellingStandMenu.
                var sellingStandMenu = (SellingStandMenu) _currentDialogCaller;
                sellingStandMenu.MakeChoiceInGoodSelectionDialog(new SellingStandMenu.GoodSelectionDialog_OptionProps(){
                    ItemData = itemData,
                    Quantity = quantity,
                });
                OnCancelDialog(dialogTypeToDialogScreen[_currentlyCalledDialogType]);
                break;
            case DialogType.CraftItem:
                throw new ArgumentException("The crafting dialog doesn't have item panels. Only recipe panels.");
            /*case DialogType.ShowInventory:
                // ?
                OnCancelDialog(dialogTypeToDialogScreen[_dialogType]);
                break;*/
            case DialogType.ShowMarket:
                throw new ArgumentException("The show market dialog doesn't have item panels. Only ?????.");
            default:
                throw new NotImplementedException();
        }
    }
    
    public void OnRecipePanelChosen(RecipeData recipeData){
        if(_currentDialogCaller == null){
            throw new NullReferenceException("_currentDialogCaller is null. A screen was likely active, without a dialogue caller actually calling it.");
        }

        switch(_currentlyCalledDialogType){
            case DialogType.ChooseItem:
                throw new ArgumentException("The choose item dialog doesn't have recipe panels. Only item panels.");
            case DialogType.CraftItem:
                // ASSUMPTION: _dialogCaller has to be a CraftingSlot.
                var craftingSlot = (CraftingSlot) _currentDialogCaller;
                bool canCraftRecipe = true;
                foreach(var materialNeeded in recipeData.materialsNeeded){
                    if(!InventoryManager.Instance.HasItems(materialNeeded))
                    {
                        canCraftRecipe = false;
                        break;
                    }
                }
                if (canCraftRecipe)
                {
                    CraftingManager.Instance.MakeChoiceInCraftingDialog(recipeData, craftingSlot);
                    OnCancelDialog(dialogTypeToDialogScreen[_currentlyCalledDialogType]);
                }
                else
                {
                    Debug.LogWarning("Couldn't start recipe: Insufficient materials.");
                    // TODO OPTIONAL: Proper communication with the player. Tell 'em that the recipe is unstartable!
                }     
                break;
            /*case DialogType.ShowInventory:
                throw new ArgumentException();*/
            case DialogType.ShowMarket:
                throw new ArgumentException("The show market dialog doesn't have recipe panels. Only ?????.");
            default:
                throw new NotImplementedException();
        }
    }

    // Update the UI of your inventory.
    // TODO OPTIONAL: Make a set method that only updates ONE slot in the inventory. More efficient than refilling the entire inventory.
    public void SetInventory(Dictionary<string, InventorySlotData> inventory){
        // 1. Empty the inventory UI, grids and whatnot.
        // 2. Use <inventory> and populate the UI.
        // UI dialogs/objects to update:
        //      Place Item Dialog (V Items you can place)
        //      TODO OPTIONAL: Market Dialog (X Current quantities of purchasable produces)
        //      Crafting Dialog (V Current quantities of recipe produces, X material quantities)

        // Might want this to be a custom class? The inventroy dialog.
        var preexistingGridItems = new GameObject[chooseItemDialogGrid.transform.childCount];
        int i = 0;
        foreach (Transform tr in chooseItemDialogGrid.transform)
        {
            preexistingGridItems[i] = tr.gameObject;
            i++;
        }
        foreach(GameObject gridItemGO in preexistingGridItems){
            Destroy(gridItemGO);
        }

        // NOTE: UIInventoryItemPanel contains ItemPanelButton.
        UIInventoryItemPanel newGridItem;
        foreach(InventorySlotData inventorySlotData in inventory.Values){
            newGridItem = Instantiate(chooseItemDialogGrid_gridItemPrefab, chooseItemDialogGrid.transform);
            newGridItem.nameText.text = inventorySlotData.ItemData.Name;
            newGridItem.quantityText.text = "x" + inventorySlotData.Quantity.ToString();
            newGridItem.itemImage.sprite = inventorySlotData.ItemData.Sprite;
            newGridItem.itemPanelButton.InitializePanelData(inventorySlotData.ItemData, inventorySlotData.Quantity);
        }

        // itemNameToRecipePanelDict.Select(pair => pair.Key == );
        foreach(var (itemName, itemData) in inventory){
            if(!itemNameToRecipePanelDict.ContainsKey(itemName)) continue;
            var correspondingRecipePanel = itemNameToRecipePanelDict[itemName];
            if(correspondingRecipePanel.CurrentQuantityInInventory != itemData.Quantity)
            {
                correspondingRecipePanel.CurrentQuantityInInventory = itemData.Quantity;  // NOTE: This line would look different in a method named UpdateInventoryItemPile().
            }
        }
    }

    //  Unlike SetInventory, there's no need to empty the inventory UI really.
    //  I could only generate at the start of the game, and perhaps add recipes as the game progresses.
    //  
    //  Or otherwise, considering that the status of each recipe might change (can no longer make due to usage of materials, for example)?
    //      It could have merit if I updated it completely.
    //  
    //  I'm leaning towards the first idea - Updating the recipe dialog without remaking it everytime.
    //  Makes most intuitive sense.
    //  THUS: This should only be fired once, at CraftingManager.Start().
    internal void SetRecipes(List<RecipeData> recipes)
    {
        // 1. Empty the recipe UI's HBox.
        // 2. Use <inventory> and populate the UI.

        // Might want this to be a custom class? The inventroy dialog.
        var preexistingHBoxItems = new GameObject[craftingRecipesDialogHBox.transform.childCount];
        int i = 0;
        foreach (Transform tr in craftingRecipesDialogHBox.transform)
        {
            preexistingHBoxItems[i] = tr.gameObject;
            i++;
        }
        foreach(GameObject hboxItemGO in preexistingHBoxItems){
            Destroy(hboxItemGO);
        }

        // NOTE: UIInventoryItemPanel contains ItemPanelButton.
        // UIInventoryItemPanel newGridItem;
        // UIRecipePanel;
        itemNameToRecipePanelDict = new Dictionary<string, RecipePanelButton>();
        RecipePanelButton newRecipePanel;
        foreach(RecipeData recipeData in recipes){
            newRecipePanel = Instantiate(craftingRecipesDialogHBox_recipePanelPrefab, craftingRecipesDialogHBox.transform);
            newRecipePanel.InitializePanelData(recipeData);
            itemNameToRecipePanelDict.Add(recipeData.produce.itemData.Name, newRecipePanel);
        }
        // LayoutRebuilder.ForceRebuildLayoutImmediate(craftingRecipesDialogHBox_rectTransform);  // An attempted fix by ChatGPT. Failed.
    }
}
