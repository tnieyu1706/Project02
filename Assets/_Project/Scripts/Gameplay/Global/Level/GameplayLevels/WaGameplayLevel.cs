using System;
using System.Collections.Generic;
using BackboneLogger;
using Game.WaveAttack;
using Gameplay.Global;
using UnityEngine;

namespace Game.BaseGameplay
{
    [CreateAssetMenu(fileName = "WaGameplayLevel", menuName = "Game/Level/WaGameplayLevel")]
    public class WaGameplayLevel : BaseGameplayLevel
    {
        public List<WaveRuleData> waveRules = new();

        public override string GetDisplayedText()
        {
            return $"{base.GetDisplayedText()}\n" +
                   $"Waves: {waveRules.Count}";
        }

        public override void SetupGameplay()
        {
            if (PathManager.Instance == null)
            {
                BLogger.Log($"[WaGameplayLevel] PathManager is not found after loading scene", LogLevel.Error,
                    category: "Loading");
                return;
            }

            var pathCount = PathManager.Instance.Paths.Count;

            if (TowerRuntimeManager.Instance == null)
            {
                BLogger.Log($"[WaGameplayLevel] TowerRuntimeManager is not found after loading scene",
                    LogLevel.Error, category: "Loading");
                return;
            }

            var towerCount = TowerRuntimeManager.Instance.towerRuntimeList.Count;

            if (GameplayTransition.DataManager == null)
            {
                BLogger.Log($"[WaGameplayLevel] GameplayTransitionDataManager is not found after loading scene",
                    LogLevel.Error, category: "Loading");
                return;
            }

            var military = GameplayTransition.DataManager.MilitaryTemp;

            if (WaGameplayController.Instance == null)
            {
                BLogger.Log($"[WaGameplayLevel] WaGameplayController is not found after loading scene",
                    LogLevel.Error, category: "Loading");
                return;
            }

            // setup: Wave Attack
            WaGameplayController.Setup(this, military, pathCount, towerCount);

            if (BaseGameplayController.Instance == null)
            {
                BLogger.Log($"[WaGameplayLevel] BaseGameplayController is not found after loading scene",
                    LogLevel.Error, category: "Loading");
                return;
            }

            BaseGameplayController.Instance.Setup(this, waveRules.Count);
        }
    }


    [Serializable]
    public class WaveRuleData
    {
        public int maxBaseDamageOutput;
        public int maxEntityDeploymentCount;
    }
}