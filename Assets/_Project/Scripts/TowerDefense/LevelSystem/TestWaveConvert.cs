using TnieYuPackage.CustomAttributes;
using UnityEngine;
using Button = EditorAttributes.ButtonAttribute;

namespace Game.Td
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