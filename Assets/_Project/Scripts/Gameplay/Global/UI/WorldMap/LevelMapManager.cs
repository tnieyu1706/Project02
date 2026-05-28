using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Global.PlayerDataSystem;
using Game.BaseGameplay;
using Reflex.Attributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Global.UI.WorldMap
{
    public class LevelMapManager : Singleton<LevelMapManager>
    {
        [SerializeField] private List<LevelMapComponent> levelMapComponents = new();

        [Tooltip("Cây cấu trúc định tuyến Unlock Level. Được Inject qua Reflex hoặc gán trực tiếp.")] [Inject]
        private LevelMapTreeSo
            levelMapTree; // Nếu chưa setup Reflex cho WorldMap, bạn có thể SerializeField và gán tay.

        private Dictionary<Guid, LevelMapComponent> levelMapComponentsByGuidData;

        private void OnEnable()
        {
            if (!PlayerDataManager.HasInstance) return;

            var levelsData = PlayerDataManager.Instance.PlayerData.Levels;
            var dict = levelsData.ToDictionary(data => data.id, data => data);

            // 1. Đồng bộ cơ bản giữa PlayerData và các Component trên Map
            foreach (var levelComponent in levelMapComponents)
            {
                if (dict.TryGetValue(levelComponent.ID, out var levelData))
                {
                    levelComponent.BindData(levelData);
                    continue;
                }

                // Nếu là màn mới được kéo thả vào chưa có trong save, tạo data mới
                levelsData.Add(levelComponent.SaveData());
            }

            // 2. XỬ LÝ ĐỒ THỊ (CÂY UNLOCK)
            ResolveLevelTreeUnlocks(levelsData);

            // 3. Update lại hiển thị (Alpha / Nút bấm) cho các Component sau khi chạy logic Unlock
            foreach (var levelComponent in levelMapComponents)
            {
                if (dict.TryGetValue(levelComponent.ID, out var levelData))
                {
                    levelComponent.BindData(levelData);
                }
            }

            PlayerDataManager.Instance.Save();
        }

        /// <summary>
        /// Duyệt qua cây đồ thị. Nếu một level đã có điểm (hoàn thành), tự động Unlock các con của nó.
        /// </summary>
        private void ResolveLevelTreeUnlocks(List<LevelData> levelsData)
        {
            if (levelMapTree == null)
            {
                Debug.LogWarning("[LevelMapManager] LevelMapTree chưa được cấu hình. Bỏ qua logic Unlock theo đồ thị.");
                return;
            }

            // Tạo Dictionary map giữa BuildingGameplayLevel và LevelData để tra cứu nhanh
            var levelToDataMap = new Dictionary<BuildingGameplayLevel, LevelData>();

            foreach (var component in levelMapComponents)
            {
                // Reflection hoặc public getter để lấy level SO từ component (Yêu cầu Component mở getter cho biến level)
                // Giả định bạn sẽ thêm: public BuildingGameplayLevel GameplayLevel => level; vào LevelMapComponent
                var gameplayLevel = component.level;
                if (gameplayLevel != null)
                {
                    var data = levelsData.FirstOrDefault(d => d.id.Guid == component.ID);
                    if (data != null)
                    {
                        levelToDataMap[gameplayLevel] = data;
                    }
                }
            }

            // Chạy Logic duyệt Graph
            foreach (var node in levelMapTree.nodes)
            {
                if (node.level == null) continue;

                if (levelToDataMap.TryGetValue(node.level, out var parentData))
                {
                    // Nếu màn này đã được chơi (Score > 0)
                    if (parentData.score > 0)
                    {
                        // Mở khoá tất cả các màn tiếp theo
                        foreach (var childNode in node.children)
                        {
                            if (childNode.level == null) continue;

                            if (levelToDataMap.TryGetValue(childNode.level, out var childData))
                            {
                                childData.isUnlocked = true;
                            }
                        }
                    }
                }
            }
        }
    }
}