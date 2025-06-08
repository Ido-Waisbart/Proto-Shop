// Also includes money.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct InventorySlotData
{
    public ItemData ItemData;
    public int Quantity;
}

[Serializable]
public struct JsonInventorySlotData
{
    public string itemID;
    public int quantity;
}
[Serializable]
public struct JsonInventoryData
{
    public JsonInventorySlotData[] items;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public static string initialInventoryJSONString = @"{
	""items"": [
		{
			""itemID"": ""Bowie Knife"",
			""quantity"": 4
		},
		{
			""itemID"": ""Iron"",
			""quantity"": 30
		},
		{
			""itemID"": ""Wood"",
			""quantity"": 25
		},
		{
			""itemID"": ""Leather"",
			""quantity"": 10
		},
		{
			""itemID"": ""Herbs"",
			""quantity"": 1
		},
		{
			""itemID"": ""Gems"",
			""quantity"": 4
		},
		{
			""itemID"": ""Threads"",
			""quantity"": 4
		}
	]
}";
    private Dictionary<string, InventorySlotData> inventory;
    [SerializeField] private int INITIAL_COIN_AMOUNT = 100;
    private int coinAmount = 0;

    private const string playerPrefKey_coinAmount = "Coins";
    private const string playerPrefKey_inventoryJsonString = "InventoryJsonString";
    
    // TODO OPTIONAL: Move to different file.
    private const string playerPrefKey_isPlayerNew = "IsPlayerNew";
    private bool IsPlayerNew(){
        return PlayerPrefs.GetInt(playerPrefKey_isPlayerNew, 0) == 0;
    }
    private void MakePlayerIntoNotNew(){
        PlayerPrefs.SetInt(playerPrefKey_isPlayerNew, 1);
    }

    [SerializeField, ReorderableList] private List<ItemData> definedItemDatas;
    public Dictionary<string, ItemData> idToItemData;

    void Start()
    {
        if(Instance != null)
        {
            throw new Exception();
        }
        Instance = this;

        if(IsPlayerNew()){
            coinAmount = INITIAL_COIN_AMOUNT;
            SetCoinAmountPref(coinAmount);

            SetInventoryPref(initialInventoryJSONString);
            // TODO OPTIONAL: Start a tutorial. Perhaps a cutscene.
            
            MakePlayerIntoNotNew();
        }
        else{
            coinAmount = PlayerPrefs.GetInt(playerPrefKey_coinAmount);
        }
        
        MainGameUIOverlayManager.Instance.SetCoinAmount(coinAmount);  // ASSUMPTION: MainGameUIOverlayManager runs before InventoryManager.

        idToItemData = definedItemDatas.ToDictionary(itemData => itemData.Name, itemData => itemData);
        
        SetInventoryListToPrefValue();
        DialogManager.Instance.SetInventory(inventory);
    }

    private void SetCoinAmountPref(int newCoinAmount){
        PlayerPrefs.SetInt(playerPrefKey_coinAmount, newCoinAmount);
    }

    private void SetInventoryPref(string newInventoryJsonString){
        PlayerPrefs.SetString(playerPrefKey_inventoryJsonString, newInventoryJsonString);
    }

    public void AddCoins(int coinAmountToAdd){
        coinAmount += coinAmountToAdd;
        // The calls to SetCoinAmountPref() were commented out, because the game by default resets your inventory (and as such, there's practically nothing to save except for money.)
        // SetCoinAmountPref(coinAmount);  // TODO OPTIONAL: Should this happen only in a routine once-per-minute save?
        // DialogManager.Instance.SetCoinAmount(inventory);  // TODO OPTIONAL: Will there be any dialogs with the coin amount in them? (Likely - Yes.)
        MainGameUIOverlayManager.Instance.SetCoinAmount(coinAmount);
    }

    // Despite the seeming duplicate code, it's very possible that I would want slightly or notably different logic (like VFX) to happen in this.
    public void RemoveCoins(int coinAmountToReduce){
        coinAmount -= coinAmountToReduce;
        // SetCoinAmountPref(coinAmount);  // TODO OPTIONAL: Should this happen only in a routine once-per-minute save?
        // DialogManager.Instance.SetCoinAmount(inventory);  // TODO OPTIONAL: Will there be any dialogs with the coin amount in them? (Likely - Yes.)
        MainGameUIOverlayManager.Instance.SetCoinAmount(coinAmount);
    }

    private void SetInventoryListToPrefValue(){
        string inventoryJsonString = PlayerPrefs.GetString(playerPrefKey_inventoryJsonString, null);
        if(inventoryJsonString == null)
            throw new NullReferenceException();
        
        inventory = ParseJsonStringToInventory(inventoryJsonString);
    }

    private Dictionary<string, InventorySlotData> ParseJsonStringToInventory(string inventoryJsonString){
        var jsonInventory = JsonUtility.FromJson<JsonInventoryData>(inventoryJsonString);
        var newInventory = new Dictionary<string, InventorySlotData>();
        foreach(JsonInventorySlotData slotData in jsonInventory.items){
            var itemID = slotData.itemID;
            var itemData = definedItemDatas.Find(itemData => itemData.Name == itemID);
            newInventory[itemID] = new InventorySlotData(){ItemData = itemData, Quantity = slotData.quantity};
        }
        return newInventory;
    }
    
    // TODO OPTIONAL: How often should the data be saved?
    // For the purposes of this prototype demo, I will save every time I add an item.
    public void AddItemPile(ItemPile newItemPileData){
        string newItemName = newItemPileData.itemData.name;
        int newItemQuantityToAdd = newItemPileData.quantity;
        if(inventory.ContainsKey(newItemName)){
            InventorySlotData inventorySlotData = inventory[newItemName];
            int newQuantity = inventorySlotData.Quantity + newItemQuantityToAdd;
            inventory[newItemName] = new InventorySlotData(){ ItemData = newItemPileData.itemData, Quantity = newQuantity};
        }
        else{
            inventory[newItemName] = new InventorySlotData() { ItemData = newItemPileData.itemData, Quantity = newItemQuantityToAdd };
        }
        DialogManager.Instance.SetInventory(inventory);  // TODO OPTIONAL: Should I remove this in favour of a UpdateInventoryItemPile() method? (same at RemoveItemPile())
    }

    public void RemoveItemPile(ItemData itemData)
    {
        if (!inventory.ContainsKey(itemData.Name))
        {
            throw new ArgumentException($"Can't remove an item not found in the inventory: {itemData.Name}");
        }
        
        inventory.Remove(itemData.Name);

        DialogManager.Instance.SetInventory(inventory);
    }

    public void ReduceItems(ItemData itemData, int amount){
        if(!inventory.ContainsKey(itemData.Name)){
            throw new ArgumentException($"Can't reduce the amount of an item not found in the inventory: {itemData.Name} x{amount}");
        }
        var presentItemPile = inventory[itemData.Name];
        var presentItemQuantity = presentItemPile.Quantity;
        if(presentItemQuantity < amount){
            throw new ArgumentException($"Can't reduce more than what's possible from the inventory: {itemData.Name} x{amount}");
        }

        // TODO OPTIONAL: Use ItemPile?
        InventorySlotData newItemPile = presentItemPile;
        newItemPile.Quantity -= amount;
        if(newItemPile.Quantity == 0)
        {
            inventory.Remove(itemData.Name);
        }
        else{
            inventory[itemData.Name] = newItemPile;
        }

        DialogManager.Instance.SetInventory(inventory);
    }

    internal bool HasItems(ItemPile materialNeeded)
    {
        var itemName = materialNeeded.itemData.Name;
        return inventory.ContainsKey(itemName) && inventory[itemName].Quantity >= materialNeeded.quantity;
    }

    internal int GetQuantityOfItem(ItemData itemData)
    {
        var itemName = itemData.Name;
        if(!inventory.ContainsKey(itemName)) return 0;
        return inventory[itemName].Quantity;
    }
}
