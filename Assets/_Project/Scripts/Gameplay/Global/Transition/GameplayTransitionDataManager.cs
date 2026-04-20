using System;
using System.Collections.Generic;
using Game.BaseGameplay;
using Game.Global;
using SceneManagement;
using UnityEngine;

namespace Gameplay.Global
{
    public class GameplayTransitionDataManager : MonoBehaviour
    {
        [SerializeField] private SceneGroup mainMenuSceneGroup;

        public Dictionary<ArmyType, int> MilitaryTemp { get; set; }

        public EventData ActiveEvent { get; set; }
        public BuildingGameplayLevel CurrentBuildingLevel { get; set; }

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
            SceneLoader.Instance.Load(mainMenuSceneGroup);
        }

        private void OnDestroy()
        {
            GameplayTransition.DataManager = null;
        }
    }
}