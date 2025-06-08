using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public static class InventoryPlayerPrefsTools
{
    private const string playerPrefKey_inventoryJsonString = "InventoryJsonString";
    private const string playerPrefKey_isPlayerNew = "IsPlayerNew";

    // TODO OPTIONAL: Move to an aptly named script.
    // Makes the player new, reset inventory and prompts a re-viewing of the intro/tutorial.
    [MenuItem("Tiny Shop/Reset Save Data")]
    public static void ResetSaveData()
    {
        PlayerPrefs.SetInt(playerPrefKey_isPlayerNew, 0);
        PlayerPrefs.SetString(playerPrefKey_inventoryJsonString, InventoryManager.initialInventoryJSONString);
        Debug.Log("Successfully reset save data!");
    }
    
    [MenuItem("Tiny Shop/Print Inventory State")]
    public static void PrintInventoryState()
    {
        Debug.Log(PlayerPrefs.GetString(playerPrefKey_inventoryJsonString, "[INVENTORY UNSET]"));
    }
    
    [MenuItem("Tiny Shop/Add x5 Iron To Inventory")]
    public static void Add5IronInventoryState()
    {
        string inventoryJsonString = PlayerPrefs.GetString(playerPrefKey_inventoryJsonString, null);
        if(inventoryJsonString == null)
        {
            throw new NullReferenceException();
        }

        var jsonInventory = JsonUtility.FromJson<JsonInventoryData>(inventoryJsonString);
        var inventoryItemsList = jsonInventory.items.ToList();
        var ironInventorySlot = inventoryItemsList.FirstOrDefault(itemSlotData => itemSlotData.itemID == "Iron");
        if(!string.IsNullOrEmpty(ironInventorySlot.itemID))
        {
            // There is iron in the player's inventory.
            int index = inventoryItemsList.FindIndex(slot => slot.itemID == "Iron");
            ironInventorySlot.quantity += 5;
            inventoryItemsList[index] = ironInventorySlot;
        }
        else{
            inventoryItemsList.Add(new JsonInventorySlotData(){
                itemID = "Iron",
                quantity = 5,});
        }

        jsonInventory.items = inventoryItemsList.ToArray();

        string newInventoryJsonString = JsonUtility.ToJson(jsonInventory);
        PlayerPrefs.SetString(playerPrefKey_inventoryJsonString, newInventoryJsonString);
    }
}
