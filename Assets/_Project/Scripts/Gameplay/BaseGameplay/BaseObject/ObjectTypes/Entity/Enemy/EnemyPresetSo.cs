using UnityEngine;

namespace Game.BaseGameplay
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Game/TD/Entity/Enemy/Preset")]
    public class EnemyPresetSo : EntityPresetSo
    {
        public float moveSpeed;
        public int baseCausingDmg;
        public int dropMoney;
    }
}