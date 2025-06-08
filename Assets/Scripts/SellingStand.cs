using Tarodev_Pathfinding._Scripts.Grid;
using Tarodev_Pathfinding._Scripts.Tiles;
using UnityEngine;
using UnityEngine.EventSystems;

public class SellingStand : MonoBehaviour
{
    // [SerializeField, NotNull] private Transform soldGoodContainer;
    [SerializeField, NotNull] private SpriteRenderer soldGoodRenderer;
    [SerializeField, NotNull] private SellingStandMenu sellingStandMenu;
    [SerializeField, NotNull] private Transform standingPoint;  // Where do the customers stand?
    [SerializeField, NotNull] private IsoNode standingPointNode;  // Where do the customers stand?

    private ItemData _itemData;
    private int _soldGoodQuantity = 0;

    

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        sellingStandMenu.ToggleVisiblity();
    }

    /*
    NOTE: New issue: Selecting an item when there's already an item on the table would generally mean re-adding the placed items to the inventory.
    But what if it's the same item?
    What if I have 7 knives on the table, and I select the inventory's pile of 2 knives?



    Tiny Shop's solution is that you don't place an *amount* of items. Not an item pile.

    But rather, you place an item *type*. You select a table to contain knives, for example, and it would try to sell out all of the knives.

    I'm not sure if there's a certain cap to how many of the same item can be sold in Tiny Shop, however.

    The answer is likely that there's no cap.
    And even if not - This shall be my approach.

    The entire pile of the same item will be sold, when you select said item on a table.
    */
    public void OnPlayerPlacedGoods(ItemData itemData, int amount){
        if (_itemData != null && itemData.Name == _itemData.Name)
        {
            // There is already an item, and it's the same item as the newly placed one!

            _soldGoodQuantity += amount;
        }
        else
        {
            bool wasSellingStandEmpty = _itemData == null;

            if(!wasSellingStandEmpty){
                InventoryManager.Instance.AddItemPile(new ItemPile(){
                    itemData = _itemData,
                    quantity = _soldGoodQuantity,
                });
            }

            soldGoodRenderer.sprite = itemData.Sprite;
            _soldGoodQuantity = amount;
            _itemData = itemData;
            if(wasSellingStandEmpty)
            {
                // Needs to happen specifically AFTER _itemData is updated.
                ShopManager.Instance.OnSellingStandWasFedItemPile();
            }
        }

        InventoryManager.Instance.RemoveItemPile(itemData);
        // InventoryManager.Instance.ReduceItems(itemData, amount);
    }

    public void OnPlayerEmptyGoods(){
        if (_itemData == null) return;
        ShopManager.Instance.OnSellingStandWasEmptied(_itemData, _soldGoodQuantity);
        _itemData = null;
        soldGoodRenderer.sprite = null;
        _soldGoodQuantity = 0;
    }

    public void OnCustomerTakeItem(){
        _soldGoodQuantity--;
        if(_soldGoodQuantity == 0){
            soldGoodRenderer.sprite = null;
            _itemData = null;
            ShopManager.Instance.OnSellingStandWasEmptied(null, 0);  // No need to return items to the inventory.
        }
    }

    // Used by Customers to hold the item and buy it in the line.
    public ItemData GetItemData(){
        return _itemData;
    }

    public Vector2 GetCustomerStandingPoint(){
        return standingPoint.position;
    }

    public IsoNode GetCustomerStandingPointNode(){
        return standingPointNode;
    }
}
