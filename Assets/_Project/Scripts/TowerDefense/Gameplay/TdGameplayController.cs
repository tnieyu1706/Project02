using System;
using System.Collections.Generic;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Td
{
    [DefaultExecutionOrder(-49)]
    public class TdGameplayController : SingletonBehavior<TdGameplayController>
    {
        [SerializeField] private LevelData levelData;
        
        public TdWaveController tdWaveController;
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
            dontDestroyOnLoad = false;
            
            base.Awake();
            tdWaveController ??= new TdWaveController();
        }

        private void Start()
        {
            LoadLevel(levelData);
        }

        private void LoadLevel(LevelData level)
        {
            currentLevelWave = level.LevelWave;
            Health = level.maxHealth;
            Money = level.startingMoney;

            //LevelScene load

            //wave setup
            tdWaveController.SetupLevelWave(level.LevelWave);
        }

        public void PlayWave()
        {
            tdWaveController.PlayWave();
        }
    }
}