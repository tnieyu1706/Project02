using System;
using EditorAttributes;
using Game.BaseGameplay;
using KBCore.Refs;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;

namespace _Project.Scripts.Gameplay.Global.UI.WorldMap
{
    [Serializable]
    public class LevelData
    {
        [ReadOnly] public SerializableGuid id = Guid.NewGuid();
        public bool isUnlocked;

        /// <summary>
        /// Score == 0 means the level is not completed,
        /// Score > 0 means the level is completed,
        /// and the value represents the score achieved in that level.
        /// </summary>
        public int score;
    }

    public class LevelMapComponent : MonoBehaviour, ISaveLoadData<LevelData>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private LevelData levelData;
        [SerializeField, Required] private BuildingGameplayLevel level;
        [SerializeField, Self] private CanvasGroup canvasGroup;
        [SerializeField] private Button button;

        public Guid ID => levelData.id;

        void Start()
        {
            InitLevelData();
        }

        private void InitLevelData()
        {
            canvasGroup.alpha = levelData.isUnlocked ? 1 : 0f;
        }

        private void OnEnable()
        {
            // Thay vì LoadLevel trực tiếp, chúng ta sẽ mở Popup Info
            button.onClick.AddListener(ShowLevelInfo);
        }

        private void ShowLevelInfo()
        {
            // Gọi Singleton của UI Toolkit để hiển thị thông tin
            if (LevelMapInfoUIManager.Instance != null)
            {
                LevelMapInfoUIManager.Instance.ShowInfo(levelData, level);
            }
            else
            {
                Debug.LogError("LevelMapInfoUIManager Instance is missing in the scene!");
            }
        }

        private void OnDisable()
        {
            button.onClick.RemoveAllListeners();
        }

        public void BindData(LevelData data)
        {
            levelData = data;
            canvasGroup.alpha = levelData.isUnlocked ? 1 : 0f;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            LevelMapSelectionHighLight.Instance?.Display(transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            LevelMapSelectionHighLight.Instance?.Hide();
        }

        public LevelData SaveData()
        {
            return levelData;
        }
    }
}