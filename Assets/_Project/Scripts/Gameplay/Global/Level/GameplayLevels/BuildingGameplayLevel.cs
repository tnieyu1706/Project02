using System.Collections.Generic;
using System.IO;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace Game.BaseGameplay
{
    [CreateAssetMenu(fileName = "BuildingGameplayLevel", menuName = "Game/Level/BuildingGameplayLevel")]
    public class BuildingGameplayLevel : ScriptableObject
    {
        public List<EventData> events = new();

        [FilePath(".json")] public string saveFilePath;

        public bool isCompleted;

        public void Reset()
        {
            isCompleted = false;
            File.WriteAllText(saveFilePath, string.Empty);
            foreach (var baseConfig in events)
            {
                baseConfig.isCompleted = false;
            }
        }
    }
}