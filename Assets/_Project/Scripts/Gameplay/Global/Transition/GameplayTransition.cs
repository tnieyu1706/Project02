using System;
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

        [Header("Building Gameplay")] [SerializeField]
        private SceneGroup buildingGameplaySceneGroup;

        [Header("Base Gameplay")] [SerializeField]
        private SceneGroup baseGameplaySceneGroup;

        [Header("TowerDefense Gameplay")] [SerializeField]
        private SceneData towerDefenseSceneData;

        [Header("WaveAttack Gameplay")] [SerializeField]
        private SceneData waveAttackSceneData;

        #region Building Gameplay Transition

        public async UniTask LoadMainMenuGame()
        {
            await SceneLoader.Instance.Load(mainMenuSceneGroup);
        }

        public async UniTask LoadWorldMapGame()
        {
            await SceneLoader.Instance.Load(worldMapSceneGroup);
        }

        public async UniTask CreateBuildingGameplay(BuildingGameplayLevel buildingLevelSource,
            LevelData levelData)
        {
            var buildingGameplaySg = GetBuildingGameplaySgWithLevel(buildingLevelSource);

            await SceneLoader.Instance.Load(buildingGameplaySg);

            if (SbGameplayController.HasInstance)
            {
                Debug.Log("Creating building gameplay");
                buildingLevelSource.Reset();
                SbGameplayController.Instance.CreateGameplay(buildingLevelSource);
                DataManager.CurrentBuildingLevel = buildingLevelSource;
                DataManager.CurrentLevel = levelData;
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
                SbGameplayController.Instance.SetupGameplay(DataManager.CurrentBuildingLevel);
                await SbGameplayController.Instance.LoadAll();

                DataManager.ActiveEvent?.ApplyEventResult();
            }
        }

        #endregion

        #region Base Gameplay Transition

        public async UniTask LoadBaseGameplayWithEvent(EventData eventData)
        {
            //setup
            var gameLevel = LevelTypeManager.Instance.GetGameplayLevelBy(eventData);
            var loadingSceneGroup = GetBaseGameplayAndLevelSceneGroup(gameLevel);

            switch (eventData.eventType)
            {
                case EventType.Defense:
                    loadingSceneGroup.scenes.Add(towerDefenseSceneData);
                    eventData.shouldChange = true;
                    break;
                case EventType.Attack:
                    //only get ArmyStorageUsing when Attack event 
                    DataManager.MilitaryTemp = SbGameplayController.Instance.GetArmyStorageAsUsing();
                    loadingSceneGroup.scenes.Add(waveAttackSceneData);
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
            if (SbGameplayController.HasInstance)
            {
                SbGameplayController.Instance.SaveAll();
            }

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