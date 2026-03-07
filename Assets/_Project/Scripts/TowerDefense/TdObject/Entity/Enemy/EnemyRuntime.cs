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
    public class EnemyRuntime : MonoBehaviour, IEnemyProperties
    {
        [SerializeField, Self] private Animator animator;
        [SerializeField, Self] private EnemyMover mover;

        private EnemyPresetSo currentPreset;

        #region ENTITY PROPERTIES

        [SerializeField] private HealthProperty healthProperty;

        public HealthProperty HealthProperty => healthProperty;
        [field: SerializeField] public float Def { get; set; }
        [field: SerializeField] public float MoveSpeed { get; set; }
        [field: SerializeField] public int MapDmg { get; set; }
        [field: SerializeField] public int EarningMoney { get; set; }

        #endregion

        private void Awake()
        {
            animator ??= GetComponent<Animator>();
            mover ??= GetComponent<EnemyMover>();
        }

        void Start()
        {
            HealthProperty.OnDead += OnEnemyDead;
        }

        void OnDestroy()
        {
            HealthProperty.OnDead -= OnEnemyDead;
        }

        private void OnEnemyDead()
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

        private void SetPreset(EnemyPresetSo preset)
        {
            //Pre-Setup
            if (currentPreset is not null)
            {
                currentPreset.configurators.ForEach(conf => conf.UnConfig(gameObject));
                currentPreset.behaviourInstaller?.UnInstall(gameObject);
            }

            //Setup
            HealthProperty.Hp = preset.maxHp;
            HealthProperty.MaxHp = preset.maxHp;
            Def = preset.def;
            MoveSpeed = preset.moveSpeed;
            MapDmg = preset.mapDmg;
            EarningMoney = preset.earningMoney;
            animator.runtimeAnimatorController = preset.animatorController;
            animator.SetBool(TdConstant.TD_ENTITY_MOVE_PARAMETER, true);

            //After-Setup
            currentPreset = preset;
            currentPreset.behaviourInstaller?.Install(gameObject);
            currentPreset.configurators.ForEach(conf => conf.Config(gameObject));
        }
    }
}