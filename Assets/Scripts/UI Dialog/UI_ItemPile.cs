using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemPile : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemQuantityText;

    public void Initialize(Sprite newSprite, string newItemName, string newItemQuantity){
        // Some item piles, like the materials needed in a recipe, might be missing a field, like the name text.
        if(_itemImage != null) _itemImage.sprite = newSprite;
        if(_itemNameText != null) _itemNameText.text = newItemName;
        if(_itemQuantityText != null) _itemQuantityText.text = newItemQuantity;
    }
}
