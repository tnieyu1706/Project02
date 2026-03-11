using KBCore.Refs;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Td
{
    public interface IEnemyProperties : IHealthProperty
    {
        public float Def { get; }
        public float MoveSpeed { get; }
        public int MapDmg { get; }
        public int EarningMoney { get; }
    }

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(EnemyMover))]
    public class EnemyRuntime : EntityRuntime<EnemyPresetSo>, IEnemyProperties
    {
        [SerializeField, Self] private EnemyMover mover;
        
        #region ENTITY PROPERTIES
        
        [field: SerializeField] public float MoveSpeed { get; set; }
        [field: SerializeField] public int MapDmg { get; set; }
        [field: SerializeField] public int EarningMoney { get; set; }

        #endregion

        protected override void OnEntityDead()
        {
            //Entity Death Handler
            animator.SetTrigger(TdConstant.TD_ENTITY_DEAD_PARAMETER);

            HandleEnemyDead();
        }

        private void HandleEnemyDead()
        {
            TdGameplayController.Instance.Money += EarningMoney;

            TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Enemy].Release(gameObject);
        }

        public void Setup(EnemyPresetSo presetSo, SplineContainer pathMove, Vector2 offset)
        {
            SetPreset(presetSo);
            SetEnemyMoverComponent(pathMove, offset);
        }

        private void SetEnemyMoverComponent(SplineContainer pathMove, Vector2 offset)
        {
            mover.Setup(pathMove, MoveSpeed);
            mover.offset = offset;
        }

        protected override void SetPresetProperties(EnemyPresetSo preset)
        {
            base.SetPresetProperties(preset);
            MoveSpeed = preset.moveSpeed;
            MapDmg = preset.mapDmg;
            EarningMoney = preset.earningMoney;
        }
    }
}