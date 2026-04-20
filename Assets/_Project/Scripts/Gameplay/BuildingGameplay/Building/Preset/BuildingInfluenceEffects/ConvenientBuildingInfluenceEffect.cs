using System;
using UnityEngine;

namespace Game.StrategyBuilding
{
    [Serializable]
    public class ConvenientBuildingInfluenceEffect : IBuildingInfluenceEffect
    {
        public float influenceRatio;

        // THÊM: Cài đặt hiển thị cho hiệu ứng Tốt
        public string EffectName => "Tăng hiệu suất";
        public Color EffectColor => Color.green;
        public string GetEffectValue() => $"+{Mathf.RoundToInt(influenceRatio * 100)}%";

        public void ApplyEffect(IBuildingImpacted impacted)
        {
            impacted.InfluenceRatio.Value += influenceRatio;
        }

        public void RemoveEffect(IBuildingImpacted impacted)
        {
            impacted.InfluenceRatio.Value -= influenceRatio;
        }
    }
}