using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO OPTIONAL: Might be unneeded, due to the existence of ItemPanelButton, which MIGHT fulfil the same purpose.
public class UIInventoryItemPanel : MonoBehaviour
{
    [SerializeField, NotNull] public Image itemImage;
    [SerializeField, NotNull] public TMP_Text nameText;
    [SerializeField, NotNull] public TMP_Text quantityText;
    [SerializeField, NotNull] public ItemPanelButton itemPanelButton;
}
