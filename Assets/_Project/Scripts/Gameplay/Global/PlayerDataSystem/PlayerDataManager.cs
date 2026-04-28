using System;
using System.Collections.Generic;
using System.IO;
using _Project.Scripts.Gameplay.Global.UI.WorldMap;
using Newtonsoft.Json;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.PlayerDataSystem
{
    public class PlayerData
    {
        public readonly List<LevelData> Levels = new();
    }

    public class PlayerDataManager : SingletonBehavior<PlayerDataManager>
    {
        private const string FILE_NAME = "PlayerData";
        public static string FilePath => $"{Application.persistentDataPath}/{FILE_NAME}.json";

        public PlayerData PlayerData { get; private set; }

        public void Save()
        {
            Debug.Log($"[{nameof(PlayerDataManager)}] Saving to {FilePath}");
            File.WriteAllText(FilePath,
                JsonConvert.SerializeObject(PlayerData, Formatting.Indented));
        }

        public void Load()
        {
            if (!File.Exists(FilePath))
            {
                InitData();
                return;
            }

            var json = File.ReadAllText(FilePath);
            PlayerData = JsonConvert.DeserializeObject<PlayerData>(json);
        }

        private void InitData()
        {
            PlayerData = new PlayerData();
        }

        public static bool IsDataFileExist()
        {
            return File.Exists(FilePath);
        }
    }
}