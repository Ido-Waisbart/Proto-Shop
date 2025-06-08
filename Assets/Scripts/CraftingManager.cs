// -- Neo Blawnode-style Crafting
//  For each crafting the player has begun, count the time left in this way:
//      Store the elapsed time, advance the time only when the player is playing.
//  Better for games where you're meant to play actively, when the game is designed to be finite.
//  
// -- Classic Shop Heroes-style Crafting
//  For each crafting the player has begun, count the time left in this way:
//      Store the start time, and only based on that deduce how much time is left.
//  Perfect for games where you're allowed to quit, wait for the game to finish the crafting.
//  Although, it's associated for me with grindy mechanics.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Note: Identical (?) to another struct, InventorySlotData.
[Serializable]
public struct ItemPile{
    public ItemData itemData;
    public int quantity;
}

[Serializable]
public struct ItemPile_Json{
    public string itemID;
    public int quantity;
}

// Recipes are found in a .json file, and extracted/loaded from it.
[Serializable]
public struct RecipeData{
    public ItemPile[] materialsNeeded;  // // May be gears/consumables, similarly to some recipes in Shop Heroes.
    public ItemPile produce;
    public int craftingTimeInSeconds;
}

// Recipes are found in a .json file, and extracted/loaded from it.
[Serializable]
public struct RecipeData_Json{
    public ItemPile_Json[] materialsNeeded;  // // May be gears/consumables, similarly to some recipes in Shop Heroes.
    public ItemPile_Json produce;
    public int craftingTimeInSeconds;
}

[Serializable]
public struct RecipeJsonFileData{
    public RecipeData_Json[] craftingRecipes;
}

public struct CraftingSlotData{
    public RecipeData recipe;
    public int craftingRepetitions;  // How many times do we craft this item?
    public float elapsedTimeInMiliseconds;  // Float is enough - It can pretty much store up to ~16,777,216 milliseconds (~4.66 hours)
}

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [SerializeField] private List<CraftingSlot> craftingSlots;
    private int activeCraftingSlotCounter = 0;
    [SerializeField] private TextAsset recipeJsonFile;
    private List<RecipeData> recipes;  // TODO OPTIONAL: Dictionary<string, RecipeData>? (There's also the potential of having multiple recipes for the same product ID.)

    void Start()
    {
        if(Instance != null)
        {
            throw new Exception();
        }
        Instance = this;

        var recipeJson = JsonUtility.FromJson<RecipeJsonFileData>(recipeJsonFile.text);
        //  Convert recipe .json file (with items represented by string IDs) =,
        //      to recipe list (with items represented by itemDatas)
        //  ASSUMPTION: InventoryManager.cs runs BEFORE CraftingManager.cs.
        var itemIDToItemData = InventoryManager.Instance.idToItemData;
        recipes = recipeJson.craftingRecipes.ToList().Select(craftingRecipeJson => new RecipeData(){
            materialsNeeded = craftingRecipeJson.materialsNeeded.Select(materialNeeded => new ItemPile{
                itemData = itemIDToItemData[materialNeeded.itemID],
                quantity = materialNeeded.quantity,
            }).ToArray(),
            produce = new ItemPile(){
                itemData = itemIDToItemData[craftingRecipeJson.produce.itemID],
                quantity = craftingRecipeJson.produce.quantity,
            },
            craftingTimeInSeconds = craftingRecipeJson.craftingTimeInSeconds,
        }).ToList();
        
        DialogManager.Instance.SetRecipes(recipes);

        /*StartCraftingSlot(new CraftingSlotData(){
            recipe = recipes[0],
            craftingRepetitions = 5,  // <- For debugging purposes.
            elapsedTimeInMiliseconds = 0,
            });
        StartCraftingSlot(new CraftingSlotData(){
            recipe = recipes[1],
            craftingRepetitions = 5,  // <- For debugging purposes.
            elapsedTimeInMiliseconds = 0,
            });*/
    }

    void Update()
    {
        foreach(CraftingSlot craftingSlot in craftingSlots){
            if(craftingSlot.IsInactive){
                continue;
            }
            CraftingSlotData slotData = (CraftingSlotData)craftingSlot.CraftingSlotData;
            slotData.elapsedTimeInMiliseconds += Time.deltaTime * 1000;
            craftingSlot.CraftingSlotData = slotData;
            
            craftingSlot.UpdateSlider();
            if(craftingSlot.IsDone){
                OnCraftingSlotFinished(craftingSlot, slotData);
            }
        }
    }

    void OnCraftingSlotFinished(CraftingSlot craftingSlot, CraftingSlotData slotData)
    {
        InventoryManager.Instance.AddItemPile(slotData.recipe.produce);
        if (slotData.craftingRepetitions > 1)
        {
            // Restart!
            slotData.craftingRepetitions -= 1;
            slotData.elapsedTimeInMiliseconds = 0;
            craftingSlot.CraftingSlotData = slotData;
            craftingSlot.SetAmountLeftToProduce(slotData.craftingRepetitions);
        }
        else
        {
            // No more of the same item to craft - This slot can be hidden/made inactive.
            // It's not necessary though - I've made it so that the slot remains empty.
            activeCraftingSlotCounter--;
            craftingSlot.CraftingSlotData = null;
            craftingSlot.SetImage(null);
            //  NOTE: If there's no more crafting slot repetitions, then (like Tiny Shop) it disappears immediately. Not even a fade away animation.
            //      It's of course changable, but I'll go with this approach too.
            // craftingSlots[activeCraftingSlotCounter].SetProgressSliderVisibility(false);
            craftingSlot.SetProgressSliderVisibility(false);
        }
    }

    // An implementation appropriate to Shop Heroes, but not Tiny Shop.
    // TODO OPTIONAL: The variable activeCraftingSlotCounter might be redundant in this implementation.
    public void StartCraftingSlot(CraftingSlotData newCraftingSlotData, CraftingSlot craftingSlot){
        if(activeCraftingSlotCounter >= craftingSlots.Count){
            throw new Exception("Too many crafting slots are active.");
        }

        craftingSlot.CraftingSlotData = newCraftingSlotData;
        craftingSlot.SetProgressSliderVisibility(true);
        craftingSlot.SetImage(newCraftingSlotData.recipe.produce.itemData.Sprite);
        craftingSlot.SetAmountLeftToProduce(newCraftingSlotData.craftingRepetitions);
        activeCraftingSlotCounter++;
    }

    // An implementation appropriate to Tiny Shop, but not Shop Heroes.
    // I wanted a crafting slot layout like Shop Heroes, where there's several ever-apparent slots - And slot 3 can be active, while slots 1 and 2 can't.
    // THAT'S the difference. There's an associated with a specific slot.
    /*public void StartCraftingSlot(CraftingSlotData newCraftingSlotData){
        if(activeCraftingSlotCounter >= craftingSlots.Count){
            throw new Exception("Too many crafting slots are active.");
        }

        var craftingSlot = craftingSlots[activeCraftingSlotCounter];
        craftingSlot.CraftingSlotData = newCraftingSlotData;
        craftingSlot.SetProgressSliderVisibility(true);
        craftingSlot.SetImage(newCraftingSlotData.recipe.produce.itemData.Sprite);
        craftingSlot.SetAmountLeftToProduce(newCraftingSlotData.craftingRepetitions);
        activeCraftingSlotCounter++;
    }*/

    // ASSUMPTION: The needed materials exist in the inventory.
    internal void MakeChoiceInCraftingDialog(RecipeData recipeData, CraftingSlot craftingSlot)
    {
        // 1. Reduce items from inventory.
        // 2. Start crafting.
        foreach(var material in recipeData.materialsNeeded)
        {
            InventoryManager.Instance.ReduceItems(material.itemData, material.quantity);
        }

        // TODO OPTIONAL: What if this recipe already is in progress in one of the slots? Should I allow duplicates?
        StartCraftingSlot(new CraftingSlotData(){
            recipe = recipeData,
            craftingRepetitions = 1,
            elapsedTimeInMiliseconds = 0
        }, craftingSlot);
    }
}
