using System.Collections.Generic;
using TnieYuPackage.Utils;
using UnityEngine;

namespace Game.StrategyBuilding
{
    public interface ITilePos
    {
        /// <summary>
        /// Position of tile in grid map. Unit = tile.
        /// </summary>
        Vector2Int TilePosition { get; }
    }

    /// <summary>
    /// Get influenced by tiles. Apply for Building Object.
    /// </summary>
    public interface IBuildingImpacted : ITilePos
    {
        ObservableValue<float> InfluenceRatio { get; set; }
        /// <summary>
        /// Key = Vector2Int: Direction of Influencer by self direction.
        /// </summary>
        Dictionary<Vector2Int, ITileInfluencer> TileInfluencers { get; }
    }

    /// <summary>
    /// Cause influence for Buildings. Apply for All Tile.
    /// </summary>
    public interface ITileInfluencer : ITilePos
    {
        SbTileLayer TileLayer { get; }
        /// <summary>
        /// Key = Vector2Int: Direction of Impacted by self direction.
        /// </summary>
        Dictionary<Vector2Int, IBuildingImpacted> ImpactedBuildings { get; }
        
        void OnDestroyed();
    }
}