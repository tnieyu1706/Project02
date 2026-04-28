using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Global.PlayerDataSystem;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.UI.WorldMap
{
    public class LevelMapManager : SingletonBehavior<LevelMapManager>
    {
        [SerializeField] private List<LevelMapComponent> levelMapComponents = new();    

        private Dictionary<Guid, LevelMapComponent> levelMapComponentsByGuidData;

        private void OnEnable()
        {
            if (PlayerDataManager.Instance != null)
            {
                var levelsData = PlayerDataManager.Instance.PlayerData.Levels;
                var dict = levelsData.ToDictionary(data => data.id, data => data);

                // synchronize level map components with player data
                foreach (var levelComponent in levelMapComponents)
                {
                    if (dict.TryGetValue(levelComponent.ID, out var levelData))
                    {
                        levelComponent.BindData(levelData);
                        continue;
                    }

                    levelsData.Add(levelComponent.SaveData());
                }
            }
        }
    }
}