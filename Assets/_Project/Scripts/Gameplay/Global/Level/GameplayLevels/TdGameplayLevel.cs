using System.Collections.Generic;
using BackboneLogger;
using Game.TowerDefense;
using UnityEngine;

namespace Game.BaseGameplay
{
    [CreateAssetMenu(fileName = "TdGameplayLevel", menuName = "Game/Level/TdGameplayLevel")]
    public class TdGameplayLevel : BaseGameplayLevel
    {
        public List<WaveDataConfigDto> waveConfigs;

        public override string GetDisplayedText()
        {
            return $"{base.GetDisplayedText()}\n" +
                   $"Waves: {waveConfigs.Count}";
        }

        public override void SetupGameplay()
        {
            if (TdGameplayController.Instance == null)
            {
                BLogger.Log($"[TdGameplayLevel] TdGameplayController is not found after loading scene",
                    LogLevel.Error, category: "Loading");
                return;
            }

            TdGameplayController.Instance.Setup(this);

            if (BaseGameplayController.Instance == null)
            {
                BLogger.Log($"[TdGameplayLevel] BaseGameplayController is not found after loading scene",
                    LogLevel.Error, category: "Loading");
                return;
            }

            BaseGameplayController.Instance.Setup(this, waveConfigs.Count);
        }
    }
}