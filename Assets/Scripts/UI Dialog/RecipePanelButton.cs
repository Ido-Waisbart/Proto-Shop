using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipePanelButton : MonoBehaviour
{
    private RecipeData _recipeData;  // NOTE: You can add [SerializeField], for the sake of debugging without calling InitializePanelData().
    
    [SerializeField, NotNull] private Image _itemImage;
    [SerializeField, NotNull] private TMP_Text _itemNameText;
    [SerializeField, NotNull] private TMP_Text _itemQuantityInInventoryText;
    private int _currentQuantityInInventory;
    public int CurrentQuantityInInventory
    {
        get { return _currentQuantityInInventory; }
        set
        {
            _currentQuantityInInventory = value;
            _itemQuantityInInventoryText.text = "x" + value.ToString();  // x4
        }
    }
    [SerializeField, NotNull] private TMP_Text _itemPriceText;
    [SerializeField, NotNull] private Transform _neededMaterialsContainer;
    [SerializeField, NotNull, PrefabObjectOnly] private UI_ItemPile _materialPanelPrefab;

    public void InitializePanelData(RecipeData recipeData){
        _recipeData = recipeData;
        var produce = recipeData.produce;
        _itemImage.sprite = produce.itemData.Sprite;
        _itemNameText.text = produce.itemData.Name;
        // TODO OPTIONAL: Make use of produce.quantity.
        _currentQuantityInInventory = InventoryManager.Instance.GetQuantityOfItem(recipeData.produce.itemData);
        _itemQuantityInInventoryText.text = "x" + _currentQuantityInInventory.ToString();  // x4
        _itemPriceText.text = produce.itemData.Price.ToString();

        foreach(var material in recipeData.materialsNeeded)
        {
            var newMaterialPanel = Instantiate(_materialPanelPrefab, _neededMaterialsContainer);
            newMaterialPanel.Initialize(material.itemData.Sprite, material.itemData.Name, "x" + material.quantity.ToString());
        }
    }

    // UI Button Click Event
    public void OnClick(){
        DialogManager.Instance.OnRecipePanelChosen(_recipeData);
    }
}
