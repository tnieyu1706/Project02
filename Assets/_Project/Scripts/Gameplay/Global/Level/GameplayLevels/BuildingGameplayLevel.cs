using System.Collections.Generic;
using System.IO;
using Eflatun.SceneReference;
using UnityEngine;

namespace Game.BaseGameplay
{
    [CreateAssetMenu(fileName = "BuildingGameplayLevel", menuName = "Game/Level/BuildingGameplayLevel")]
    public class BuildingGameplayLevel : ScriptableObject
    {
        private const string TEMP_FILE_NAME = "BuildingGameplayDataTemporate";
        public List<SerializableEventData> events = new();
        public SceneReference sceneReference;

        public string TempFilePath => $"{Application.persistentDataPath}/{TEMP_FILE_NAME}.json";

        public void Reset()
        {
            File.WriteAllText(TempFilePath, string.Empty);
            foreach (var baseConfig in events)
            {
                baseConfig.data.isCompleted = false;
            }
        }
    }
}