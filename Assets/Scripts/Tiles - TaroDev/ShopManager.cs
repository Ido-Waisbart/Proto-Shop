// TODO OPTIONAL: Move elsewhere, like simply /Assets/Scripts/?
// I likely don't need this namespace in that case, too.

using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Tiles;
using Tarodev_Pathfinding._Scripts.Grid.Scriptables;
using Tarodev_Pathfinding._Scripts.Tiles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO OPTIONAL: Does this need a rename?
[Serializable] public struct TileData{
    public Vector2 TilePosition;
    public bool IsWalkable;
    public NodeBase Tile;
}

namespace Tarodev_Pathfinding._Scripts.Grid {
    public class ShopManager : MonoBehaviour {
        public static ShopManager Instance;

        [SerializeField, PrefabObjectOnly] private Customer _customerPrefab;
        private List<Customer> _customers;
        [SerializeField] private IsoNode _purchasingPositionNode;
        public IsoNode WaitingLineNode => _purchasingPositionNode;
        [SerializeField] private IsoNode _exitNode;
        public IsoNode ExitNode => _exitNode;
        [SerializeField] private List<SellingStand> _sellingStands;
        private int _activeSellingStands = 0;
        [SerializeField] private IsoNode _customerSpawnPoint;
        [SerializeField] private ScriptableGrid _scriptableGrid;

        [SerializeField, ReorderableList] private List<TileData> _manualUnwalkableTiles;  // TODO OPTIONAL: Can probably be merged with manual nodes.
        [SerializeField] private List<IsoNode> manualNodes;
        [SerializeField] private bool _drawConnections;
        
        private SerializedDictionary<Vector2, NodeBase> Tiles { get => _tiles; set{_tiles = value;} }
        private SerializedDictionary<Vector2, NodeBase> _tiles;
    
        [SerializeField, PrefabObjectOnly, NotNull] private CanvasGroup purchaseTextPopupPrefab;  // I could make a script instead of simply using a CanvasGroup.
        [SerializeField, NotNull] private Canvas ingameUICanvas;  // I could make a script instead of simply using a Canvas.

        void Awake() => Instance = this;

        private void Start()
        {
            // _manualUnwalkableTiles likely needs tile coords, and not world position.
            Tiles = _scriptableGrid.GenerateGrid(_manualUnwalkableTiles);

            foreach (var tile in manualNodes)
            {
                // tilePos = 0,0, 0,1, 0,2... NOT actual world position (which has floats, too).
                // worldPos(tilePos.x,y) = (x - y, (x + y) * 0.5f)
                // position = (a, 0.5a) -> x-y = 0.5a, (x+y)*0.5 = 1.5a*0.5 = 0.75a
                // -> tilePos = 0.5a,0.75a
                /*var tilePosX = tile.transform.position.x * 0.5f;
                var tilePosY = tile.transform.position.y * 0.75f;
                var tilePos = new Vector2((tilePosX - tilePosY) * 0.5f,
                    (tilePosX + tilePosY) * 0.25f) * 2;*/
                
                Tiles.Add(tile.transform.position, tile);
                tile.Init(true, new SquareCoords() { Pos = tile.transform.position });
            }
            /*var startTilePosX = _customerSpawnPoint.transform.position.x;
            var startTilePosY = _customerSpawnPoint.transform.position.y;
            var startTilePos = new Vector2((startTilePosX - startTilePosY) * 0.5f,
                (startTilePosX + startTilePosY) * 0.25f) * 2;
            var purchasingLineTilePosX = _purchasingPositionNode.transform.position.x;
            var purchasingLineTilePosY = _purchasingPositionNode.transform.position.y;
            var purchasingLineTilePos = new Vector2((purchasingLineTilePosX - purchasingLineTilePosY) * 0.5f,
                (purchasingLineTilePosX + purchasingLineTilePosY) * 0.25f) * 2;
            
            Tiles.Add(startTilePos, _customerSpawnPoint);
            _customerSpawnPoint.Init(true, new SquareCoords(){Pos = startTilePos });
            _purchasingPositionNode.Init(true, new SquareCoords(){Pos = purchasingLineTilePos });*/

            foreach (var tile in Tiles.Values) tile.CacheNeighbors();

            _customers = new List<Customer>();
        }

        // Might be removed in the final product. A weird method.
        public bool IsThereAnySellingStandWithGoods(){
            var sellingStandsWithGoods = _sellingStands.Where(sellingStand => sellingStand.GetItemData() != null);
            return sellingStandsWithGoods.Count() > 0;
        }

        private void SpawnCustomer(){
            var customerGO = Instantiate(_customerPrefab, _customerSpawnPoint.transform.position, Quaternion.identity, null);
            var customer = customerGO.GetComponent<Customer>();
            var sellingStandsWithGoods = _sellingStands.Where(sellingStand => sellingStand.GetItemData() != null /*&& sellingStand.amount != 0*/);
            if(sellingStandsWithGoods.Count() == 0)
            {
                throw new Exception("A customer shouldn't spawn when there's no selling stands they can walk to!");
            }
            // https://stackoverflow.com/a/18598042
            var randomSellingStandWithGoods = sellingStandsWithGoods.ElementAt(
                UnityEngine.Random.Range(0, sellingStandsWithGoods.Count())
                );
            // The commentated code below is there to allow movement outside of the isometric grid - The idea was discarded.
            // var sellingStandIsoNode = randomSellingStandWithGoods.GetComponent<IsoNode>();  // ASSUMPTION: All SellingStand's also have IsoNode. Easy to ensure, not sure if I want to??
            // customer.SetPath(_customerSpawnPoint, _purchasingPositionNode);
            // customer.SetPath_TargetIsoNodePosition(randomSellingStandWithGoods.GetCustomerStandingPoint());
            var sellingStandStandingNode = randomSellingStandWithGoods.GetCustomerStandingPointNode();
            customer.SetNewTargetPath(_customerSpawnPoint, sellingStandStandingNode);
            customer.SetSellingStandToApproach(randomSellingStandWithGoods);
            _customers.Add(customer);
        }

        public void OnCustomerExit(Customer customer){
            _customers.Remove(customer);
            if(IsThereAnySellingStandWithGoods()) SpawnCustomer();  // TODO OPTIONAL: Different logic! When does the next customer spawn? Should he?
        }

        public void OnSellingStandWasFedItemPile(){
            _activeSellingStands++;
            if(_activeSellingStands == 1 && _customers.Count == 0){
                // Spawning a customer only when there's no customers is not perfect logic, but it's fine.
                SpawnCustomer();
            }
        }

        public void OnSellingStandWasEmptied(ItemData itemDataToReturnToInventory, int itemQuantityToReturnToInventory){
            if(itemDataToReturnToInventory != null && itemQuantityToReturnToInventory > 0)
                InventoryManager.Instance.AddItemPile(new ItemPile() { itemData = itemDataToReturnToInventory, quantity = itemQuantityToReturnToInventory });
            _activeSellingStands--;
            /*if(_activeSellingStands == 0){
                for(int i = 0; i < _customers.Count; i++)
                {
                    Destroy(_customers[i].gameObject);
                }
            }
            _customers.Clear();*/
        }

        /*private void OnTileHover(NodeBase nodeBase) {
            _goalNodeBase = nodeBase;
            _spawnedGoal.transform.position = _goalNodeBase.Coords.Pos;

            foreach (var t in Tiles.Values) t.RevertTile();

            var path = Pathfinding.FindPath(_playerNodeBase, _goalNodeBase);
        }*/

        /*void SpawnUnits() {
            _playerNodeBase = Tiles.Where(t => t.Value.Walkable).OrderBy(t => Random.value).First().Value;
            _spawnedPlayer = Instantiate(_unitPrefab, _playerNodeBase.Coords.Pos, Quaternion.identity);
            _spawnedPlayer.Init(_playerSprite);

            _spawnedGoal = Instantiate(_unitPrefab, new Vector3(50, 50, 50), Quaternion.identity);
            _spawnedGoal.Init(_goalSprite);
        }*/

        public NodeBase GetTileAtPosition(Vector2 pos) => Tiles.TryGetValue(pos, out var tile) ? tile : null;

        // Would have made this static, if it weren't for the desire to place this object in a canvas.
        // With a dedicated script, with a static singleton, this could be possible.
        public void SpawnMoneyAddedPopup(int moneyReceived, Vector3 position)
        {
            var purchaseTextPopupInstance = Instantiate(purchaseTextPopupPrefab, position, Quaternion.identity, ingameUICanvas.transform);
            purchaseTextPopupInstance.GetComponentInChildren<TMP_Text>().text = "+" + moneyReceived.ToString();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !_drawConnections) return;
            Gizmos.color = Color.red;
            foreach (var tile in Tiles)
            {
                if (tile.Value.Connection == null) continue;
                Gizmos.DrawLine((Vector3)tile.Key + new Vector3(0, 0, -1), (Vector3)tile.Value.Connection.Coords.Pos + new Vector3(0, 0, -1));
            }
        }
    }
}