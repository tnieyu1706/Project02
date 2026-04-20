using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace Game.StrategyBuilding
{
    [CreateAssetMenu(fileName = "GridMapDataController",
        menuName = "Game/StrategyBuilding/GridMap/GridMapDataController")]
    public class SbGridMapDataController : SingletonScriptable<SbGridMapDataController>
    {
        public Sprite impactedContextSprite;
        public Sprite influenceContextSprite;
    }
}