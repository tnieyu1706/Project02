using System;
using _Project.Scripts.Gameplay.Global.UI.WorldMap;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
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

        [SerializeField] private SceneGroup mainMenuSceneGroup;
        [SerializeField] private SceneGroup worldMapSceneGroup;

        [Header("Building Gameplay")] [SerializeField]
        private SceneGroup buildingGameplaySceneGroup;

        [Header("Base Gameplay")] [SerializeField]
        private SceneGroup baseGameplaySceneGroup;

        [Header("TowerDefense Gameplay")] [SerializeField]
        private SceneData towerDefenseSceneData;

        [Header("WaveAttack Gameplay")] [SerializeField]
        private SceneData waveAttackSceneData;

        #region Building Gameplay Transition

        public static async UniTask LoadMainMenuGame()
        {
            await SceneLoader.Instance.Load(Instance.mainMenuSceneGroup);
        }

        public static async UniTask LoadWorldMapGame()
        {
            await SceneLoader.Instance.Load(Instance.worldMapSceneGroup);
        }

        public static async UniTask CreateBuildingGameplay(BuildingGameplayLevel buildingLevelSource,
            LevelData levelData)
        {
            var buildingGameplaySg = GetBuildingGameplaySgWithLevel(buildingLevelSource);

            await SceneLoader.Instance.Load(buildingGameplaySg);

            if (SbGameplayController.Instance != null)
            {
                Debug.Log("Creating building gameplay");
                buildingLevelSource.Reset();
                SbGameplayController.Instance.CreateGameplay(buildingLevelSource);
                DataManager.CurrentBuildingLevel = buildingLevelSource;
                DataManager.CurrentLevel = levelData;
            }
        }

        private static SceneGroup GetBuildingGameplaySgWithLevel(BuildingGameplayLevel buildingLevelSource)
        {
            var buildingLevelSceneData = LevelSceneData(buildingLevelSource.sceneReference);
            var buildingGameplaySg = (SceneGroup)Instance.buildingGameplaySceneGroup.Clone();
            buildingGameplaySg.scenes.Add(buildingLevelSceneData);
            return buildingGameplaySg;
        }

        public static async UniTask LoadBuildingGameplay()
        {
            var buildingGameplaySg = GetBuildingGameplaySgWithLevel(DataManager.CurrentBuildingLevel);
            await SceneLoader.Instance.Load(buildingGameplaySg);
            if (SbGameplayController.Instance != null)
            {
                SbGameplayController.Instance.SetupGameplay(DataManager.CurrentBuildingLevel);
                await SbGameplayController.Instance.LoadAll();

                DataManager.ActiveEvent?.ApplyEventResult();
            }
        }

        #endregion

        #region Base Gameplay Transition

        public static async UniTask LoadBaseGameplayWithEvent(EventData eventData)
        {
            //setup
            var gameLevel = eventData.GetGameplayLevel();
            var loadingSceneGroup = GetBaseGameplayAndLevelSceneGroup(gameLevel);

            switch (eventData.eventType)
            {
                case EventType.Defense:
                    loadingSceneGroup.scenes.Add(Instance.towerDefenseSceneData);
                    eventData.shouldChange = true;
                    break;
                case EventType.Attack:
                    //only get ArmyStorageUsing when Attack event 
                    DataManager.MilitaryTemp = SbGameplayController.Instance.GetArmyStorageAsUsing();
                    loadingSceneGroup.scenes.Add(Instance.waveAttackSceneData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventData.eventType));
            }

            //handle loading...
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
            var levelSceneData = LevelSceneData(gameLevel.levelScene);

            SceneGroup globalSceneGroupClone = Instance.baseGameplaySceneGroup.Clone() as SceneGroup;
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