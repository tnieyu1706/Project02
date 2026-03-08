using EditorAttributes;
using UnityEngine;

namespace Game.Td
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Game/TD/Entity/Enemy/Preset")]
    public class EnemyPresetSo : EntityPresetSo
    {
        [PropertyOrder(-1)]
        public string enemyId;
        public float moveSpeed;
        public int mapDmg;
        public int earningMoney;
    }
}