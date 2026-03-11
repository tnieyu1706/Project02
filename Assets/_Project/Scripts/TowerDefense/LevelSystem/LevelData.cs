using Eflatun.SceneReference;
using TnieYuPackage.CustomAttributes;
using UnityEngine;
using ReadOnly = EditorAttributes.ReadOnlyAttribute;
using Button = EditorAttributes.ButtonAttribute;

namespace Game.Td
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Game/TD/Level")]
    public class LevelData : ScriptableObject
    {
        public int maxHealth;
        public int startingMoney;
        public SceneReference levelScene;
        [SerializeField, FilePath(".json")] private string levelWavePath;

        [SerializeField, ReadOnly] private LevelWave levelWave;

        public LevelWave LevelWave
        {
            get
            {
                if (levelWavePath == string.Empty) return null;

                levelWave ??= LevelWave.ConvertLevelWaveJson(levelWavePath);

                return levelWave;
            }
        }

        [Button]
        private void ConvertLevelWave()
        {
            levelWave = LevelWave.ConvertLevelWaveJson(levelWavePath);
        }
    }
}