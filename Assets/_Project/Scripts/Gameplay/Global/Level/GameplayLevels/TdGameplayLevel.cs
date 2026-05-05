using System.Collections.Generic;
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
            if (!TdGameplayController.HasInstance)
            {
                Debug.LogError($"[TdGameplayLevel] TdGameplayController is not found after loading scene");
                return;
            }

            TdGameplayController.Instance.Setup(this);

            if (!BaseGameplayController.HasInstance)
            {
                Debug.LogError($"[TdGameplayLevel] BaseGameplayController is not found after loading scene");
                return;
            }

            BaseGameplayController.Instance.Setup(this, waveConfigs.Count);
        }
    }
}