using System;
using System.Collections;
using System.Collections.Generic;
using TnieYuPackage.DesignPatterns;
using UnityEngine;

namespace Game.StrategyBuilding
{
    public class SbTimeController : SingletonBehavior<SbTimeController>
    {
        private readonly HashSet<Action> events = new();
        private WaitForSeconds routineWait;
        
        #region PROPERTIES

        public float timeInterval = 5f;

        #endregion

        protected override void Awake()
        {
            routineWait = new WaitForSeconds(timeInterval);
        }

        private void Start()
        {
            StartCoroutine(RealtimeRoutine());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        IEnumerator RealtimeRoutine()
        {
            while (true)
            {
                foreach (var action in events)
                {
                    action?.Invoke();
                }

                yield return routineWait;
            }   
        }

        public void Registry(Action action)
        {
            events.Add(action);
        }

        public void UnRegistry(Action action)
        {
            events.Remove(action);
        }
    }
}