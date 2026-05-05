using System;
using System.Collections.Generic;
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
            if (!PathManager.HasInstance)
            {
                Debug.LogError($"[WaGameplayLevel] PathManager is not found after loading scene");
                return;
            }

            var pathCount = PathManager.Instance.Paths.Count;

            if (!TowerRuntimeManager.HasInstance)
            {
                Debug.LogError($"[WaGameplayLevel] TowerRuntimeManager is not found after loading scene");
                return;
            }

            var towerCount = TowerRuntimeManager.Instance.towerRuntimeList.Count;

            if (GameplayTransition.DataManager == null)
            {
                Debug.LogError($"[WaGameplayLevel] GameplayTransition.DataManager is not found after loading scene");
                return;
            }

            var military = GameplayTransition.DataManager.MilitaryTemp;

            if (!WaGameplayController.HasInstance)
            {
                Debug.LogError($"[WaGameplayLevel] WaGameplayController is not found after loading scene");
                return;
            }

            // setup: Wave Attack
            WaGameplayController.Setup(this, military, pathCount, towerCount);

            if (!BaseGameplayController.HasInstance)
            {
                Debug.LogError($"[WaGameplayLevel] BaseGameplayController is not found after loading scene");
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