using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Game.Td
{
    [Serializable]
    public class WaveSpawnCommand
    {
        [JsonProperty] public string entityId;
        [JsonProperty] public int quantity;
        [JsonProperty] public float spawnInterval;
    }

    [Serializable]
    public class WavePath
    {
        [JsonProperty] public string pathId;
        [JsonProperty] public List<WaveSpawnCommand> spawns;
    }

    [Serializable]
    public class WaveKeyFrame
    {
        [JsonProperty] public string id;
        [JsonProperty] public float time;
        [JsonProperty] public List<WavePath> paths;
    }

    [Serializable]
    public class Wave
    {
        [JsonProperty] public string id;
        [JsonProperty] public int endTime;
        [JsonProperty] public List<WaveKeyFrame> keyframes = new();
    }

    [Serializable]
    public class LevelWave
    {
        [JsonProperty] public List<Wave> waves = new();

        public static LevelWave ConvertLevelWaveJson(string filePath)
        {
            string json = File.ReadAllText(filePath);

            JObject jsonObject = JObject.Parse(json);
            Debug.Log(jsonObject);

            return jsonObject.ToObject<LevelWave>();
        }
    }
}