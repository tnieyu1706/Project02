using TnieYuPackage.CustomAttributes.Runtime;
using UnityEngine;
using Button = EditorAttributes.ButtonAttribute;

namespace _Project.Scripts.TowerDefense.LevelSystem
{
    public class TestWaveConvert : MonoBehaviour
    {
        [FilePath(".json")] public string filePath;

        public LevelWave levelWave;

        [Button]
        public void TestConvert()
        {
            levelWave = LevelWave.ConvertLevelWaveJson(filePath);
            Debug.Log(levelWave);
        }
    }
}