using UnityEngine;
using UnityEngine.Serialization;

namespace Game.BaseGameplay.Strategies
{
    // =====================================================================
    // CONCRETE CLASS: STATIC DEFENSE (Cung thủ xếp vòng tròn quanh tháp)
    // =====================================================================

    [CreateAssetMenu(fileName = "StaticSpawn_Strategy", menuName = "Gameplay/Interact Strategies/Spawn/Static Defense")]
    public class StaticTowerSpawnInteractStrategyInstaller : TowerSpawnInteractStrategyInstaller
    {
        [Header("Static Defense Settings")] [Tooltip("Độ lệch tâm (Offset) so với gốc của tháp")]
        public Vector2 centerOffset = Vector2.zero;

        [Tooltip("Bán kính vòng tròn để lính xếp vị trí trên tháp")]
        public float spawnRadiusOffset = 1.0f;

        public override IBaseObjectInteractStrategy CreateInteractStrategy() =>
            new StaticTowerSpawnInteractStrategy(this);
    }

    public class
        StaticTowerSpawnInteractStrategy : TowerSpawnInteractStrategy<StaticTowerSpawnInteractStrategyInstaller>
    {
        public StaticTowerSpawnInteractStrategy(StaticTowerSpawnInteractStrategyInstaller installer) : base(installer)
        {
        }

        protected override void DispatchSoldier(SoldierRuntime soldier, int soldierIndex)
        {
            // Xác định tâm thực tế trên tháp (Center + Offset)
            Vector3 formationCenter = OwnerRuntime.CurrentPosition + (Vector3)ActualInstaller.centerOffset;

            // Xếp đội hình vòng tròn
            float angleStep = 360f / ActualInstaller.maxSpawnQuantity;
            float currentAngle = angleStep * soldierIndex;

            Vector3 circleOffset = new Vector3(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad),
                0f
            ) * ActualInstaller.spawnRadiusOffset;

            soldier.transform.position = formationCenter + circleOffset;

            // TODO: Thiết lập trạng thái đứng yên cho lính tại vị trí này
        }
    }
}