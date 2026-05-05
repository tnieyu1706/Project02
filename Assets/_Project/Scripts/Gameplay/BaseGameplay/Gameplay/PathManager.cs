using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.BaseGameplay
{
    public class PathManager : Singleton<PathManager>
    {
        [SerializeField] private SerializableDictionary<string, SplineContainer> paths;

        public Dictionary<string, SplineContainer> Paths => paths.Dictionary;
    }
}