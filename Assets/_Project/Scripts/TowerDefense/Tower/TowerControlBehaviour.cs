using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.TowerDefense.Entity;
using _Project.Scripts.TowerDefense.Gameplay;
using EditorAttributes;
using PrimeTween;
using TnieYuPackage.CustomAttributes.Runtime;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Project.Scripts.TowerDefense.Tower
{
    [Serializable]
    public class TowerControlBehaviourInstaller : ITowerBehaviourInstaller
    {
        public float commandRange = 1;
        public float spawnDelay = 1;
        public int maxCapacity = 1;
        public EntityPresetSo soldierPreset;

        public void Install(GameObject towerObject)
        {
            var behaviour = towerObject.AddComponent<TowerControlBehaviour>();

            SetupBehaviour(behaviour);
        }

        private void SetupBehaviour(TowerControlBehaviour behaviour)
        {
            behaviour.CommandRange = commandRange;
            behaviour.SpawnDelay = spawnDelay;
            behaviour.MaxCapacity = maxCapacity;
            behaviour.SoldierPreset = soldierPreset;
        }

        public void UnInstall(GameObject towerObject)
        {
            if (towerObject.TryGetComponent(out TowerControlBehaviour behaviour))
            {
                behaviour.KillAllSoldiers();

                Object.DestroyImmediate(behaviour);
            }
        }
    }

    public class TowerControlBehaviour : MonoBehaviour, ITowerBehaviour
    {
        private const float MOVE_SPEED = 1f;
        private static readonly Vector2 StandFixOffset = new Vector2(0.3f, 0.3f);

        private readonly List<GameObject> soldiers = new();
        private WaitForSeconds spawnWait;
        [SerializeField, ReadOnly] private Vector2 currentRallyPos = Vector2.zero;

        public float CommandRange { get; set; }
        public float SpawnDelay { get; set; }
        public int MaxCapacity { get; set; }
        public EntityPresetSo SoldierPreset { get; set; }

        private ObjectPool<GameObject> SoldierPool => TdSpawnManager.Instance.RuntimePools[TdSpawnKey.Soldier];

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
                if (soldiers.Count < MaxCapacity)
                {
                    var soldier = CreateSoldier();
                    soldier.transform.position = transform.position;

                    MoveSoldier(soldier);

                    soldiers.Add(soldier);
                }

                yield return spawnWait;
            }
        }

        private GameObject CreateSoldier()
        {
            GameObject soldier = SoldierPool.Get();
            
            if (soldier.TryGetComponent(out EntityRuntime entityRuntime))
            {
                entityRuntime.Setup(SoldierPreset);
                entityRuntime.OnDead = () => KillSoldier(soldier);
            }

            return soldier;
        }

        [Button]
        public void RallyAt(Vector2 position)
        {
            currentRallyPos = ValidatePositionRally(position);

            Rally();
        }

        private Vector2 ValidatePositionRally(Vector2 position)
        {
            if (Vector2.Distance(position, transform.position) <= CommandRange)
                return position;

            return (Vector2)transform.position +
                   (position - (Vector2)transform.position).normalized * CommandRange;
        }

        public void KillAllSoldiers()
        {
            for (int i = soldiers.Count - 1; i >= 0; i--)
            {
                KillSoldier(i);
            }
        }

        private void KillSoldier(GameObject soldier)
        {
            SoldierPool.Release(soldier);
            soldiers.Remove(soldier);
        }
        private void KillSoldier(int i)
        {
            SoldierPool.Release(soldiers[i]);
            soldiers.RemoveAt(i);
        }

        //Test
        [Button]
        public void RemoveSoldierRandomize()
        {
            int randomIndex = Random.Range(0, soldiers.Count);

            KillSoldier(randomIndex);
        }

        private Vector2 GetFixOffsetAtRallyPos()
        {
            return
                new Vector2(
                    Random.Range(-StandFixOffset.x, StandFixOffset.x),
                    Random.Range(-StandFixOffset.y, StandFixOffset.y)
                ).normalized * TdConstant.MAP_UNIT + currentRallyPos;
        }

        private void Rally()
        {
            soldiers.ForEach(MoveSoldier);
        }

        private void MoveSoldier(GameObject soldier)
        {
            Tween.LocalPositionAtSpeed(soldier.transform, soldier.transform.position, GetFixOffsetAtRallyPos(),
                MOVE_SPEED);
        }
    }
}