using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Global.UI.WorldMap;
using Cysharp.Threading.Tasks;
using Game.BaseGameplay;
using Game.Global;
using SceneManagement;
using UnityEngine;

namespace Gameplay.Global
{
    public class GameplayTransitionDataManager : MonoBehaviour
    {
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
            GameplayTransition.LoadMainMenuGame().Forget();
        }

        private void OnDestroy()
        {
            GameplayTransition.DataManager = null;
        }
    }
}