using System;
using System.Collections.Generic;
using _Project.Scripts.TowerDefense.LevelSystem;
using EditorAttributes;
using TnieYuPackage.DesignPatterns.Patterns.Singleton;
using UnityEngine;
using UnityEngine.Splines;

namespace _Project.Scripts.TowerDefense.Gameplay
{
    [DefaultExecutionOrder(-49)]
    public class TdGameplayController : SingletonBehavior<TdGameplayController>
    {
        public WaveController WaveController;

        private Dictionary<string, SplineContainer> paths = new();
        public Dictionary<string, SplineContainer> Paths => paths ??= new();

        #region GAMEPLAY PROPERTIES

        [SerializeField] private int health;
        [SerializeField] private int money;

        public Action<int> OnHealthChange;
        public Action<int> OnMoneyChange;

        public int Health
        {
            get => health;
            set
            {
                OnHealthChange?.Invoke(value);
                health = value;
            }
        }

        public int Money
        {
            get => money;
            set
            {
                OnMoneyChange?.Invoke(value);
                money = value;
            }
        }

        [SerializeField, ReadOnly] private LevelWave currentLevelWave;

        #endregion

        protected override void Awake()
        {
            base.Awake();

            WaveController = new WaveController();
        }

        public void LoadLevel(LevelData level)
        {
            currentLevelWave = level.LevelWave;
            Health = level.maxHealth;
            Money = level.startingMoney;

            //LevelScene load

            //wave setup
            WaveController.SetupLevelWave(level.LevelWave);
        }

        //Test
        public LevelData setLevelData;

        public TdGameplayController(int health)
        {
            Health = health;
        }

        [Button]
        private void LoadLevelButton()
        {
            LoadLevel(setLevelData);
        }

        [Button]
        public void PlayWave()
        {
            WaveController.PlayWave();
        }
    }
}