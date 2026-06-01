using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Global.GameController;
using _Project.Scripts.Gameplay.Global.UI.WorldMap;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Game.BaseGameplay;
using Game.BuildingGameplay;
using Game.Global;
using SceneManagement;
using UnityEngine;
using EventType = Game.BaseGameplay.EventType;

namespace Gameplay.Global
{
    [CreateAssetMenu(fileName = "GameplayTransition", menuName = "Game/Transition/GameplayTransition")]
    public class GameplayTransition : ScriptableObject
    {
        public static GameplayTransitionDataManager DataManager;

        [SerializeField] private SceneGroup mainMenuSceneGroup;
        [SerializeField] private SceneGroup worldMapSceneGroup;

        [Header("Story Gameplay")] [SerializeField]
        private SceneGroup storySceneGroup;

        [Header("Building Gameplay")] [SerializeField]
        private SceneGroup buildingGameplaySceneGroup;

        [Header("Base Gameplay")] [SerializeField]
        private SceneGroup baseGameplaySceneGroup;

        [Header("TowerDefense Gameplay")] [SerializeField]
        private SceneData towerDefenseSceneData;

        [Header("WaveAttack Gameplay")] [SerializeField]
        private SceneData waveAttackSceneData;

        public async UniTask LoadStoryGame(bool applyDelay = true)
        {
            await SceneLoader.Instance.Load(storySceneGroup, applyDelay);
        }

        #region Helper: Cleanup Current Gameplay

        private void CleanUpCurrentGameplay(bool forceSave = false)
        {
            if (SbGameplayController.HasInstance)
            {
                if (forceSave)
                {
                    SbGameplayController.Instance.SaveAll();
                }

                SbGameplayController.Instance.CleanUpGameplay();
            }
        }

        #endregion

        #region Building Gameplay Transition

        public async UniTask LoadMainMenuGame(bool applyDelay = true)
        {
            CleanUpCurrentGameplay(forceSave: true);
            await SceneLoader.Instance.Load(mainMenuSceneGroup, applyDelay);
        }

        public async UniTask LoadWorldMapGame()
        {
            CleanUpCurrentGameplay(forceSave: true);
            await SceneLoader.Instance.Load(worldMapSceneGroup);
            GameTimeController.SetTimeScaleToDefault();
        }

        public async UniTask CreateBuildingGameplay(BuildingGameplayLevel buildingLevelSource, LevelData levelData)
        {
            DataManager.CurrentBuildingLevel = buildingLevelSource;
            DataManager.CurrentLevel = levelData;
            buildingLevelSource.Reset();

            var buildingGameplaySg = GetBuildingGameplaySgWithLevel(buildingLevelSource);
            await SceneLoader.Instance.Load(buildingGameplaySg);

            if (SbGameplayController.HasInstance)
            {
                SbGameplayController.Instance.CreateGameplay(buildingLevelSource);
            }
        }

        private SceneGroup GetBuildingGameplaySgWithLevel(BuildingGameplayLevel buildingLevelSource)
        {
            var buildingLevelSceneData = LevelSceneData(buildingLevelSource.sceneReference);
            var buildingGameplaySg = (SceneGroup)buildingGameplaySceneGroup.Clone();
            buildingGameplaySg.scenes.Add(buildingLevelSceneData);
            return buildingGameplaySg;
        }

        public async UniTask LoadBuildingGameplay()
        {
            var buildingGameplaySg = GetBuildingGameplaySgWithLevel(DataManager.CurrentBuildingLevel);
            await SceneLoader.Instance.Load(buildingGameplaySg);

            if (SbGameplayController.HasInstance)
            {
                SbGameplayController.Instance.SetLevel(DataManager.CurrentBuildingLevel);
                await SbGameplayController.Instance.LoadAll();

                DataManager.ActiveEvent?.ApplyEventResult();
            }
        }

        #endregion

        #region Base Gameplay Transition

        // THÊM: Truyền Dictionary<ArmyType, int> để nhận số lượng quân lính được custom
        public async UniTask LoadBaseGameplayWithEvent(EventData eventData, Dictionary<ArmyType, int> selectedArmy = null)
        {
            var gameLevel = LevelTypeManager.Instance.GetGameplayLevelBy(eventData);
            var loadingSceneGroup = GetBaseGameplayAndLevelSceneGroup(gameLevel);

            switch (eventData.eventType)
            {
                case EventType.Defense:
                    loadingSceneGroup.scenes.Add(towerDefenseSceneData);
                    eventData.shouldChange = true;
                    break;
                case EventType.Attack:
                    // THAY ĐỔI: Nếu selectedArmy được truyền vào (từ UI Chọn Quân), ta dùng nó. 
                    // Nếu không (hoặc null), dùng Fallback lấy tất cả từ Storage (cho an toàn)
                    DataManager.MilitaryTemp = selectedArmy ?? SbGameplayController.Instance.GetArmyStorageAsUsing();
                    loadingSceneGroup.scenes.Add(waveAttackSceneData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventData.eventType));
            }

            PreLoadBaseGameplay(eventData);
            await SceneLoader.Instance.Load(loadingSceneGroup);

            gameLevel.SetupGameplay();
        }

        private void PreLoadBaseGameplay(EventData eventData)
        {
            CleanUpCurrentGameplay(forceSave: true);
            DataManager.ActiveEvent = eventData;
        }

        private SceneGroup GetBaseGameplayAndLevelSceneGroup(BaseGameplayLevel gameLevel)
        {
            var levelSceneData = LevelSceneData(gameLevel.levelScene);

            SceneGroup globalSceneGroupClone = baseGameplaySceneGroup.Clone() as SceneGroup;
            if (globalSceneGroupClone == null) return new SceneGroup();
            globalSceneGroupClone.groupName = $"{gameLevel.name}";
            globalSceneGroupClone.scenes.Add(levelSceneData);

            return globalSceneGroupClone;
        }

        private static SceneData LevelSceneData(SceneReference scene)
        {
            return new SceneData()
            {
                reference = scene,
                alwaysReload = true,
                sceneType = SceneType.Level
            };
        }

        #endregion
    }
}