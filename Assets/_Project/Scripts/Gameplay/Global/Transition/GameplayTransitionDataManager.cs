using System.Collections.Generic;
using _Project.Scripts.Gameplay.Global.PlayerDataSystem;
using _Project.Scripts.Gameplay.Global.UI.WorldMap;
using Cysharp.Threading.Tasks;
using Game.BaseGameplay;
using Game.Global;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Global
{
    public class GameplayTransitionDataManager : MonoBehaviour
    {
        [Inject] GameplayTransition gameplayTransition;

        public Dictionary<ArmyType, int> MilitaryTemp { get; set; }

        public EventData ActiveEvent { get; set; }
        public BuildingGameplayLevel CurrentBuildingLevel { get; set; }
        public LevelData CurrentLevel { get; set; }

        void Awake()
        {
            GameplayTransition.DataManager = this;
        }

        private void Start()
        {
            LoadMainMenu();
        }

        public void LoadMainMenu()
        {
            gameplayTransition.LoadMainMenuGame().Forget();
        }
        
        /// <summary>
        /// Nơi duy nhất xử lý nghiệp vụ Data (Cập nhật điểm, ra lệnh lưu).
        /// Gameplay module (như SbGameplayController) chỉ việc gọi hàm này để báo cáo kết quả.
        /// </summary>
        public void SubmitLevelResult(int finalScore)
        {
            if (CurrentLevel == null) return;

            // Xử lý luôn logic: Chỉ cập nhật nếu đạt điểm cao hơn
            if (finalScore > CurrentLevel.score)
            {
                CurrentLevel.score = finalScore;
                
                Debug.Log($"[TransitionData] Kỷ lục mới: {finalScore} điểm. Tiến hành lưu PlayerData.");
                
                // Lưu thẳng xuống file để đảm bảo không mất dữ liệu
                if (PlayerDataManager.HasInstance)
                {
                    PlayerDataManager.Instance.Save();
                }
            }

            // GHI CHÚ: Logic Unlock (Node -> Con) đã được xử lý bằng thuật toán Flood 
            // bên trong LevelMapManager.OnEnable() (như file tôi đã gửi ở tin nhắn trước).
            // Do đó, khi bấm nút chuyển Scene về Map, Scene Map bật lên sẽ TỰ ĐỘNG nhận
            // diện điểm số mới này và Unlock các node con tương ứng! Hoàn toàn Decoupled!
        }

        private void OnDestroy()
        {
            GameplayTransition.DataManager = null;
        }
    }
}