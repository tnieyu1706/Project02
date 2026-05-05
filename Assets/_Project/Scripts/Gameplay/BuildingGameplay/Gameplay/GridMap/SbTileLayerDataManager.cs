using System;
using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.StrategyBuilding
{
    [Serializable]
    public class SbTileLayerData
    {
        public TileBase tile;
    }

    [CreateAssetMenu(fileName = "SbTileLayerDataManager",
        menuName = "Game/StrategyBuilding/GridMap/SbTileLayerDataManager")]
    public class SbTileLayerDataManager : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<SbTileLayer, SbTileLayerData> tileLayerDataMap;

        public Dictionary<SbTileLayer, SbTileLayerData> Refs => tileLayerDataMap.Dictionary;
    }
}