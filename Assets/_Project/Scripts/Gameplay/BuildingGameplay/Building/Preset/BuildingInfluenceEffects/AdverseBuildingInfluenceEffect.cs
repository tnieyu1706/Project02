using System;
using UnityEngine;

namespace Game.StrategyBuilding
{
    [Serializable]
    public class AdverseBuildingInfluenceEffect : IBuildingInfluenceEffect
    {
        public float influenceRatio;
        
        // THÊM: Cài đặt hiển thị cho hiệu ứng Xấu
        public string EffectName => "Giảm hiệu suất";
        public Color EffectColor => Color.red;
        public string GetEffectValue() => $"-{Mathf.RoundToInt(influenceRatio * 100)}%";
        
        public void ApplyEffect(IBuildingImpacted impacted)
        {
            impacted.InfluenceRatio.Value -= influenceRatio;
        }

        public void RemoveEffect(IBuildingImpacted impacted)
        {
            impacted.InfluenceRatio.Value += influenceRatio;
        }
    }
}