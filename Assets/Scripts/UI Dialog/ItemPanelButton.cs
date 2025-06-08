using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanelButton : MonoBehaviour
{
    /*[SerializeField] */private ItemData _itemData;
    /*[SerializeField] */private int _quantity = 1;

    [SerializeField, NotNull] private Image itemImage;
    [SerializeField, NotNull] private TMP_Text itemNameText;
    [SerializeField, NotNull] private TMP_Text itemQuantityText;

    public void InitializePanelData(ItemData itemData, int quantity){
        _itemData = itemData;
        _quantity = quantity;
        itemImage.sprite = itemData.Sprite;
        itemNameText.text = itemData.Name;
        itemQuantityText.text = "x" + quantity.ToString();  // x4
    }

    // UI Button Click Event
    public void OnClick(){
        DialogManager.Instance.OnItemPanelChosen(_itemData, _quantity);
    }
}
