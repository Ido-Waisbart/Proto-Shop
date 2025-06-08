using UnityEngine;

public enum ItemCategory{
        Sellable,
        Decoration,
}
// TODO OPTIONAL: Only show the appropriate item types if the chosen category matches.
public enum ItemType{
        Material,
        Gear,
        Consumable,

        Special,
        Decorataion,
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Tiny Shop/Item Data")]
public class ItemData : ScriptableObject
{
    public int Price;
    public string Name => this.name;
    // public string Description;
    public Sprite Sprite;
    public ItemCategory ItemCategory;
    public ItemType ItemType;
}
