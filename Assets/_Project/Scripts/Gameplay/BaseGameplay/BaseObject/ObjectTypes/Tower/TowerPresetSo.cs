using System.Collections.Generic;
using UnityEngine;

namespace Game.BaseGameplay
{
    public enum TowerLevel
    {
        Level0,
        Level1,
        Level2,
        Level3
    }

    public enum TowerType
    {
        Archer,
        Barrack,
        Magic,
        Cannon
    }

    public abstract class TowerEvent : ScriptableObject
    {
        public Sprite eventIcon;
        public abstract void OnCall(TowerRuntime towerRuntime);
    }

    [CreateAssetMenu(fileName = "Tower", menuName = "Game/TD/Tower")]
    public class TowerPresetSo : BaseObjectPresetSo
    {
        public Sprite towerIcon;
        public int towerPriceValue;

        public TowerType towerType;
        public TowerLevel towerLevel;

        public List<TowerEvent> towerEvents = new();

        //Support Funcs
        public static int CalculateCost(TowerPresetSo currentPreset, TowerPresetSo nextPreset)
        {
            return nextPreset.towerPriceValue - currentPreset.towerPriceValue;
        }
    }
}