using System;
using System.Collections.Generic;
using _Scripts.Tiles;
using Tarodev_Pathfinding._Scripts;
using Tarodev_Pathfinding._Scripts.Grid;
using Tarodev_Pathfinding._Scripts.Tiles;
using TMPro;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private IsoNode startIsoNode;
    private IsoNode currStepIsoNode;
    private int currStepIsoNodeIndex;
    private IsoNode nextStepIsoNode;
    private IsoNode targetIsoNode;
    private Vector2 startIsoNode_position;  // Customer might go through a path from next to a selling stand, to elsewhere.
    private Vector2 targetIsoNode_position;  // Customer might go through a path to a selling stand, and stand next to it.
    [SerializeField] private float tileToTileDeltaTime = 0.75f;
    private float lastWalkTime = float.PositiveInfinity;
    [SerializeField] private float waitNextToSellingStandDeltaTime = 2;
    [SerializeField] private float waitInLineDeltaTime = 3;
    private float startWaitingTime = float.PositiveInfinity;  // Waiting in line, waiting next to selling stand...

    private List<NodeBase> calculatedPath;

    private enum CustomerState{
        Walk,  // Needs to target a specific item stand. (Relevant: state_walkSubstate, targetIsoNode/targetIsoNode_position)
        Ponder,  // Waits in front of a specific item stand. Happens after walking, unless buying. TODO OPTIONAL: Ponder at other selling stands.
        WaitInLine,  // Happens after walking. Includes walking ahead.
        PurchaseCelebration,  // TODO OPTIONAL: In Shop Heroes, the character does a little celebration before walking away. Not important for this project.
    }
    private CustomerState state = CustomerState.Walk;

    private enum CustomerState_WalkSubstate{
        TargetRandomSellingStand,
        TargetWaitingLine,
        TargetExit,
    }
    private CustomerState_WalkSubstate state_walkSubstate;

    [NonSerialized] public SellingStand targetSellingStand;  // Set by ShopManager, when it sets a new path towards a random selling stand.
    // [NonSerialized] public ItemData itemToPurchase;  // Set by ShopManager, when it sets a new path towards a random selling stand.
    private ItemData heldItem;

    [SerializeField, NotNull] private SpriteRenderer heldItemSpriteRenderer;


    /*public void SetPath_StartIsoNodePosition(Vector2 newStartIsoNode_position){
        startIsoNode_position = newStartIsoNode_position;
    }

    public void SetPath_TargetIsoNodePosition(Vector2 newTargetIsoNode_position){
        targetIsoNode_position = newTargetIsoNode_position;
    }*/

    public void SetNewTargetPath(IsoNode newStartIsoNode, IsoNode newTargetIsoNode)
    {
        startIsoNode = newStartIsoNode;
        targetIsoNode = newTargetIsoNode;
        calculatedPath = Pathfinding.FindPath(startIsoNode, targetIsoNode);
        if (calculatedPath == null)
        {
            throw new NullReferenceException($"Couldn't find a path from {newStartIsoNode} to {newTargetIsoNode}.");
        }
        currStepIsoNode = startIsoNode;
        nextStepIsoNode = calculatedPath != null ? (IsoNode)calculatedPath[calculatedPath.Count - 1] : null;
        currStepIsoNodeIndex = calculatedPath.Count - 1;
        lastWalkTime = Time.time;  // Start walking!
    }

    public void SetSellingStandToApproach(SellingStand newSellingStandToApproach){
        targetSellingStand = newSellingStandToApproach;
    }

    /*public void SetItemToPurchase(ItemData newItemToPurchase){
        itemToPurchase = newItemToPurchase;
    }*/

    void Start()
    {
        if(targetIsoNode == null){
            throw new NullReferenceException();
        }
    }

    void Update()
    {
        switch(state){
            case CustomerState.Walk:
                if(nextStepIsoNode == null)
                {
                    // print("Target = Null");
                    return;
                }

                // If reached target, change state. Else, walk.
                float tNormalized = (Time.time - lastWalkTime) / tileToTileDeltaTime;
                if(tNormalized >= 1){
                    // print("REACHED NEXT STEP TILE");
                    // Choose next target.
                    bool needToRefreshPath = false;  // TODO OPTIONAL: Only refresh the path if there's a change in the layout.
                    if(needToRefreshPath){
                        calculatedPath = Pathfinding.FindPath(nextStepIsoNode, targetIsoNode);
                    }
                    
                    if(nextStepIsoNode == calculatedPath[0]){
                        print("REACHED TARGET TILE");
                        switch(state_walkSubstate){
                            case CustomerState_WalkSubstate.TargetWaitingLine:
                                // Reached waiting line. Start waiting.
                                state = CustomerState.WaitInLine;
                                startWaitingTime = Time.time;
                                print("Waiting in line...");
                                break;
                                
                            case CustomerState_WalkSubstate.TargetRandomSellingStand:
                                // Reached a selling stand. Pick the item (show it visually?) and walk to the waiting line.
                                state = CustomerState.Ponder;
                                startWaitingTime = Time.time;
                                print("Pondering...");
                                break;
                                
                            case CustomerState_WalkSubstate.TargetExit:
                                // Reached the exit. Disappear.
                                ShopManager.Instance.OnCustomerExit(this);  // May cause a new customer to appear, if [INSERT LOGIC]/there's any items for purchase.
                                print("Goodbye!");
                                Destroy(gameObject);
                                break;

                            default:
                                throw new NotImplementedException("Unexpected state_walkSubstate: " + state_walkSubstate.ToString());
                        }
                        return;
                    }
                    currStepIsoNode = nextStepIsoNode;
                    nextStepIsoNode = (IsoNode) calculatedPath[currStepIsoNodeIndex - 1];
                    currStepIsoNodeIndex--;
                    lastWalkTime = Time.time;  // Start walking!
                    return;
                }
                
                transform.position = new Vector3(
                    Mathf.Lerp(currStepIsoNode.transform.position.x, nextStepIsoNode.transform.position.x, tNormalized),
                    Mathf.Lerp(currStepIsoNode.transform.position.y, nextStepIsoNode.transform.position.y, tNormalized),
                    0);
                break;

            case CustomerState.Ponder:
                // Wait for 2 seconds... and pick up!
                float tNormalized_nextToSellingStand = (Time.time - startWaitingTime) / waitNextToSellingStandDeltaTime;
                if(tNormalized_nextToSellingStand >= 1){
                    heldItem = targetSellingStand.GetItemData();
                    if (heldItem == null)
                    {
                        // TODO OPTIONAL: Either go to another table, or go home, depending on whether there's any more tables with an item.
                        //  At the moment, there's no other tables, so the character will simply go home.
                        state = CustomerState.Walk;
                        state_walkSubstate = CustomerState_WalkSubstate.TargetExit;
                        SetNewTargetPath(targetIsoNode, ShopManager.Instance.ExitNode);
                        return;
                    }
                    targetSellingStand.OnCustomerTakeItem();
                    heldItemSpriteRenderer.sprite = heldItem.Sprite;
                    heldItemSpriteRenderer.gameObject.SetActive(true);

                    print($"Picked up {heldItem.Name}!");

                    state = CustomerState.Walk;
                    state_walkSubstate = CustomerState_WalkSubstate.TargetWaitingLine;
                    SetNewTargetPath(targetIsoNode, ShopManager.Instance.WaitingLineNode);
                }
                break;

            case CustomerState.WaitInLine:
                // Wait for 3 seconds... and purchase!
                float tNormalized_waitInLine = (Time.time - startWaitingTime) / waitInLineDeltaTime;
                if(tNormalized_waitInLine >= 1){
                    // ASSUMPTION: The customer is indeed holding an item.
                    InventoryManager.Instance.AddCoins(heldItem.Price);
                    ShopManager.Instance.SpawnMoneyAddedPopup(heldItem.Price, heldItemSpriteRenderer.transform.position);
                    print($"Purchased {heldItem.Name}!");

                    state = CustomerState.Walk;
                    state_walkSubstate = CustomerState_WalkSubstate.TargetExit;
                    SetNewTargetPath(targetIsoNode, ShopManager.Instance.ExitNode);
                }
                break;

            default:
                throw new NotImplementedException("Unimplemented state: " + state.ToString());
        }
    }
}
