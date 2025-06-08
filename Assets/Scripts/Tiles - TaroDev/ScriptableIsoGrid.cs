using System.Collections.Generic;
using System.Linq;
using _Scripts.Tiles;
using UnityEngine;

namespace Tarodev_Pathfinding._Scripts.Grid.Scriptables {
    [CreateAssetMenu(fileName = "New Scriptable Iso Grid")]
    public class ScriptableIsoGrid : ScriptableGrid
    {
        [SerializeField,Range(3,50)] private int _gridWidth = 16;
        [SerializeField,Range(3,50)] private int _gridHeight = 9;
        
        public override Dictionary<Vector2, NodeBase> GenerateGridWithObstacles() =>
            GenerateGridWithObstacles(new List<TileData>());
        public override Dictionary<Vector2, NodeBase> GenerateGridWithObstacles(List<TileData> manualTiles) {
            var tiles = new Dictionary<Vector2, NodeBase>();
            var grid = new GameObject {
                name = "Tile Grid"
            };
            
            for (var x = 0; x < _gridWidth; x++) {
                for (var y = 0; y < _gridHeight; y++) {
                    // if(unwalkableTiles.Contains(new Vector2(x, y)))
                        // continue;  // Skip.
                    var pos = new Vector2((x - y) * 0.5f, (x + y) * 0.25f) * 2;
                    if(manualTiles.Exists((tile) => tile.TilePosition == pos))
                        continue;  // Skip.
                    var tile = Instantiate(nodeBasePrefab,grid.transform);
                    tile.Init(DecideIfObstacle(), new SquareCoords(){Pos = pos });
                    tiles.Add(pos,tile);
                }
            }

            return tiles;
        }

        public override Dictionary<Vector2, NodeBase> GenerateGrid() =>
            GenerateGrid(new List<TileData>());
        public override Dictionary<Vector2, NodeBase> GenerateGrid(List<TileData> manualTiles) {
            var tiles = new Dictionary<Vector2, NodeBase>();
            var grid = new GameObject {
                name = "Tile Grid"
            };
            
            for (var x = 0; x < _gridWidth; x++) {
                for (var y = 0; y < _gridHeight; y++) {
                    var pos = new Vector2((x - y) * 0.5f, (x + y) * 0.25f) * 2;
                    var isManual = manualTiles.Exists((tile) => tile.TilePosition == pos);
                    TileData manualTileData = default;
                    if(isManual){
                        manualTileData = manualTiles.Where((tile) => tile.TilePosition == pos).First();
                    }
                    
                    NodeBase tile;
                    bool isWalkable = isManual ? manualTileData.IsWalkable : true;
                    if(isManual){
                        // No need to instantiate a new tile. Do nothing, except for initializing existing tile.
                        tile = manualTileData.Tile;
                    }
                    else{
                        tile = Instantiate(nodeBasePrefab,grid.transform);
                    }
                    tile.Init(walkable: isWalkable, new SquareCoords() { Pos = pos });
                    tiles.Add(pos,tile);
                }
            }

            return tiles;
        }
    }
}
