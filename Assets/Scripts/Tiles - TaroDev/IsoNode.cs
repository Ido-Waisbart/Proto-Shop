using System.Collections.Generic;
using System.Linq;
using _Scripts.Tiles;
using Tarodev_Pathfinding._Scripts.Grid;
using UnityEngine;

namespace Tarodev_Pathfinding._Scripts.Tiles {
    public class IsoNode : NodeBase {
        private static readonly List<Vector2> Dirs = new List<Vector2>() {
            new Vector2(1, 0.5f), new Vector2(-1, 0.5f), new Vector2(1, -0.5f), new Vector2(-1, -0.5f),
            new Vector2(0, 1), new Vector2(0, -1), new Vector2(2, 0), new Vector2(-2, 0),
        };

        public override void CacheNeighbors() {
            Neighbors = new List<NodeBase>();

            foreach (var tile in Dirs.Select(dir => ShopManager.Instance.GetTileAtPosition(Coords.Pos + dir)).Where(tile => tile != null)) {
                Neighbors.Add(tile);
            }
        }
    }
}