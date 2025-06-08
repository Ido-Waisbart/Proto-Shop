// Could potentially manage gems, xp, other UI elements like in Shop Heroes,
//  and could control the flow of bottom bar menu buttons.
//  For example, just like Tiny Shop, it could have some buttons be locked, until a certain point in the game.

using System;
using TMPro;
using UnityEngine;

public class MainGameUIOverlayManager : MonoBehaviour
{
    public static MainGameUIOverlayManager Instance;

    [SerializeField] private TMP_Text coinAmountText;

    void Start()
    {
        if(Instance != null)
        {
            throw new Exception();
        }
        Instance = this;
    }
    
    public void SetCoinAmount(int newCoinAmount){
        coinAmountText.text = newCoinAmount.ToString();
    }
}
