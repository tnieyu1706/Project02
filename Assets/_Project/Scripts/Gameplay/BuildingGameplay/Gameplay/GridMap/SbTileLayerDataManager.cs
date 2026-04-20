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
    
    [CreateAssetMenu(fileName = "SbTileLayerDataManager", menuName = "Game/StrategyBuilding/GridMap/SbTileLayerDataManager")]
    public class SbTileLayerDataManager : SingletonScriptable<SbTileLayerDataManager>
    {
        [SerializeField] private SerializableDictionary<SbTileLayer, SbTileLayerData> tileLayerDataMap;

        public static Dictionary<SbTileLayer, SbTileLayerData> Refs => Instance.tileLayerDataMap.Dictionary;
    }
}