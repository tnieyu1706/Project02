using System;
using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using Game.Td;
using PrimeTween;
using TnieYuPackage.Core;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Td
{
    [Serializable]
    public class TowerControlBehaviourInstaller : ITowerBehaviourInstaller
    {
        public float commandRange = 1;
        public float spawnDelay = 1;
        public int maxCapacity = 1;
        public SoldierPresetSo soldierPreset;

        public void Install(GameObject towerObject)
        {
            var behaviour = towerObject.AddComponent<TowerControlBehaviour>();
            behaviour.CommandRange = commandRange;
            behaviour.SpawnDelay = spawnDelay;
            behaviour.MaxCapacity = maxCapacity;
            behaviour.SoldierPreset = soldierPreset;
        }

        public void UnInstall(GameObject towerObject)
        {
            if (!towerObject.TryGetComponent(out TowerControlBehaviour behaviour)) return;

            behaviour.KillAllSoldiers();
            Object.DestroyImmediate(behaviour);
        }
    }

    public class TowerControlBehaviour : MonoBehaviour, ITowerBehaviour
    {
        private const float MOVE_SPEED = 1f;
        private static readonly Vector2 StandFixOffset = new Vector2(0.3f, 0.3f);

        internal readonly List<GameObject> Soldiers = new();
        private WaitForSeconds spawnWait;
        [SerializeField, ReadOnly] private Vector2 currentRallyPos = Vector2.zero;

        #region PROPERTIES

        public float CommandRange { get; set; }
        public float SpawnDelay { get; set; }
        public int MaxCapacity { get; set; }
        public SoldierPresetSo SoldierPreset { get; set; }

        private ObjectPool<GameObject> SoldierPool => TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Soldier];

        #endregion


        private void Start()
        {
            currentRallyPos = ValidatePositionRally(currentRallyPos);

            spawnWait = new WaitForSeconds(SpawnDelay);
            StartCoroutine(ChannelingControl());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private IEnumerator ChannelingControl()
        {
            while (true)
            {
                if (Soldiers.Count < MaxCapacity)
                {
                    var soldier = CreateSoldier();
                    soldier.transform.position = transform.position;

                    SetSoldierMoveToRallyPos(soldier);

                    Soldiers.Add(soldier);
                }

                yield return spawnWait;
            }
        }

        private GameObject CreateSoldier()
        {
            GameObject soldier = SoldierPool.Get();

            if (soldier.TryGetComponent(out SoldierRuntime entityRuntime))
            {
                entityRuntime.Setup(SoldierPreset, this);
            }

            return soldier;
        }

        private Vector2 ValidatePositionRally(Vector2 position)
        {
            if (Vector2.Distance(position, transform.position) <= CommandRange)
                return position;

            return (Vector2)transform.position +
                   (position - (Vector2)transform.position).normalized * CommandRange;
        }

        [Button]
        public void RallyAt(Vector2 position)
        {
            currentRallyPos = ValidatePositionRally(position);

            Soldiers.ForEach(SetSoldierMoveToRallyPos);
        }

        private void SetSoldierMoveToRallyPos(GameObject soldier)
        {
            if (soldier.TryGetComponent(out Animator soldierAnimator))
            {
                soldierAnimator.SetBool(TdConstant.TD_ENTITY_MOVE_PARAMETER, true);
            }
            Tween.LocalPositionAtSpeed(
                soldier.transform,
                soldier.transform.position,
                GetFixOffsetAtRallyPos(),
                MOVE_SPEED
            );
            
            EventManager.Instance.RegistryDelay(
                () => soldierAnimator.SetBool(TdConstant.TD_ENTITY_MOVE_PARAMETER, false),
                MOVE_SPEED
            );
        }

        public void KillAllSoldiers()
        {
            for (int i = Soldiers.Count - 1; i >= 0; i--)
            {
                SoldierPool.Release(Soldiers[i]);
                Soldiers.RemoveAt(i);
            }
        }

        //temp using Random =====> permanent
        private Vector2 GetFixOffsetAtRallyPos()
        {
            return
                new Vector2(
                    Random.Range(-StandFixOffset.x, StandFixOffset.x),
                    Random.Range(-StandFixOffset.y, StandFixOffset.y)
                ).normalized * TdConstant.MAP_UNIT + currentRallyPos;
        }
    }
}