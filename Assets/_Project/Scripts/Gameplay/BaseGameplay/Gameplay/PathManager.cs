using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.BaseGameplay
{
    public class PathManager : SingletonBehavior<PathManager>
    {
        [SerializeField] private SerializableDictionary<string, SplineContainer> paths;

        public Dictionary<string, SplineContainer> Paths => paths.Dictionary;

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
        }
    }
}