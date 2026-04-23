using System;
using Cysharp.Threading.Tasks;
using Game.BaseGameplay;
using Game.BuildingGameplay;
using SceneManagement;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using EventType = Game.BaseGameplay.EventType;

namespace Gameplay.Global
{
    [CreateAssetMenu(fileName = "GameplayTransition", menuName = "Game/Transition/GameplayTransition")]
    public class GameplayTransition : SingletonScriptable<GameplayTransition>
    {
        public static GameplayTransitionDataManager DataManager;

        [Header("Building Gameplay")] [SerializeField]
        private SceneGroup buildingGameplaySceneGroup;

        [Header("Base Gameplay")] [SerializeField]
        private SceneGroup baseGameplaySceneGroup;

        [Header("TowerDefense Gameplay")] [SerializeField]
        private SceneData towerDefenseSceneData;

        [Header("WaveAttack Gameplay")] [SerializeField]
        private SceneData waveAttackSceneData;

        #region Building Gameplay Transition

        public static async UniTask CreateBuildingGameplay(BuildingGameplayLevel buildingLevelSource)
        {
            // attach buildingGameplayLevel for CreateBuildingGameplay
            await SceneLoader.Instance.Load(Instance.buildingGameplaySceneGroup);

            if (SbGameplayController.Instance != null)
            {
                Debug.Log("Creating building gameplay");
                buildingLevelSource.Reset();
                SbGameplayController.Instance.CreateGameplay(buildingLevelSource);
                DataManager.CurrentBuildingLevel = buildingLevelSource;

                //setup once-time for events in building level.
                foreach (var eventData in DataManager.CurrentBuildingLevel.events)
                {
                    EventData.SetupAwardsRandomized(eventData);
                }
            }
        }

        public static async UniTask LoadBuildingGameplay()
        {
            await SceneLoader.Instance.Load(Instance.buildingGameplaySceneGroup);
            if (SbGameplayController.Instance != null)
            {
                SbGameplayController.Instance.SetupGameplay(DataManager.CurrentBuildingLevel);
                if (DataManager.ActiveEvent is { isCompleted: true })
                {
                    DataManager.ActiveEvent.ExecuteAsWin();
                }

                SbGameplayController.Instance.LoadAll();
            }
        }

        #endregion

        #region Base Gameplay Transition

        public static async UniTask LoadBaseGameplayWithEvent(EventData eventData)
        {
            var gameLevel = eventData.GetGameplayLevel();
            var loadingSceneGroup = GetBaseGameplayAndLevelSceneGroup(gameLevel);

            switch (eventData.eventType)
            {
                case EventType.Defense:
                    loadingSceneGroup.scenes.Add(Instance.towerDefenseSceneData);
                    break;
                case EventType.Attack:
                    //only get ArmyStorageUsing when Attack event 
                    DataManager.MilitaryTemp = SbGameplayController.Instance.GetArmyStorageAsUsing();
                    loadingSceneGroup.scenes.Add(Instance.waveAttackSceneData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventData.eventType));
            }

            PreLoadBaseGameplay(eventData);
            await SceneLoader.Instance.Load(loadingSceneGroup);

            gameLevel.SetupGameplay();
        }

        private static void PreLoadBaseGameplay(EventData eventData)
        {
            if (SbGameplayController.Instance != null)
            {
                SbGameplayController.Instance.SaveAll();
            }

            DataManager.ActiveEvent = eventData;
        }

        private static SceneGroup GetBaseGameplayAndLevelSceneGroup(BaseGameplayLevel gameLevel)
        {
            var levelSceneData = LevelSceneData(gameLevel);

            SceneGroup globalSceneGroupClone = Instance.baseGameplaySceneGroup.Clone() as SceneGroup;
            if (globalSceneGroupClone == null) return new SceneGroup();
            globalSceneGroupClone.groupName = $"{gameLevel.name}";
            globalSceneGroupClone.scenes.Add(levelSceneData);

            return globalSceneGroupClone;
        }

        private static SceneData LevelSceneData(BaseGameplayLevel gameplayLevel)
        {
            return new SceneData()
            {
                reference = gameplayLevel.levelScene,
                alwaysReload = true,
                sceneType = SceneType.Level
            };
        }

        #endregion
    }
}