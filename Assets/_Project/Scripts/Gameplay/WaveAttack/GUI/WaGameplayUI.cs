using System;
using System.Collections.Generic;
using Game.Global;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.DictionaryUtilities;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Game.WaveAttack
{
    public class WaGameplayUI : SingletonBehavior<WaGameplayUI>
    {
        [SerializeField] private SerializableDictionary<ArmyType, Text> armyNumberTexts;

        [SerializeField] private Text maxBaseDamageOutputRuleText;
        [SerializeField] private Text maxEntityDeploymentCountRuleText;
        [SerializeField] private Text currentBaseDamageCausingText;
        [SerializeField] private Text currentEntityDeployedText;

        private readonly Dictionary<ArmyType, Action<int>> armyNumberEvents = new();

        private static Dictionary<ArmyType, ObservableValue<int>> GlobalStorageSource =>
            WaGameplayController.Instance.GlobalStorage;

        #region Events

        private void OnEnable()
        {
            RegisterGlobalArmyNumberEvents();

            WaGameplayController.Instance.maxBaseDamageOutput.OnValueChanged += HandleMaxBaseDamageOutputChangedValue;
            WaGameplayController.Instance.maxEntityDeploymentCount.OnValueChanged +=
                HandleMaxEntityDeploymentCountChangedValue;
            WaGameplayController.Instance.currentBaseDamageOutput.OnValueChanged +=
                HandleCurrentBaseDamageCausingChangedValue;
            WaGameplayController.Instance.currentEntityDeploymentCount.OnValueChanged +=
                HandleCurrentEntityDeployedChangedValue;
        }

        private void HandleMaxBaseDamageOutputChangedValue(int changedValue)
        {
            maxBaseDamageOutputRuleText.text = changedValue.ToString();
        }

        private void HandleMaxEntityDeploymentCountChangedValue(int changedValue)
        {
            maxEntityDeploymentCountRuleText.text = changedValue.ToString();
        }

        private void HandleCurrentBaseDamageCausingChangedValue(int changedValue)
        {
            currentBaseDamageCausingText.text = changedValue.ToString();
        }

        private void HandleCurrentEntityDeployedChangedValue(int changedValue)
        {
            currentEntityDeployedText.text = changedValue.ToString();
        }

        private void RegisterGlobalArmyNumberEvents()
        {
            foreach (var armyKvp in armyNumberTexts.Dictionary)
            {
                Action<int> onValueChanged = changedValue => armyKvp.Value.text = changedValue.ToString();
                var armyObserver = GlobalStorageSource[armyKvp.Key];
                armyObserver.OnValueChanged += onValueChanged;

                armyKvp.Value.text = armyObserver.Value.ToString();
                armyNumberEvents[armyKvp.Key] = onValueChanged;
            }
        }

        private void UnRegistryGlobalArmyNumberEvents()
        {
            foreach (var armyEvent in armyNumberEvents)
            {
                GlobalStorageSource[armyEvent.Key].OnValueChanged -= armyEvent.Value;
            }

            armyNumberEvents.Clear();
        }

        private void OnDisable()
        {
            if (WaGameplayController.Instance != null)
            {
                UnRegistryGlobalArmyNumberEvents();

                WaGameplayController.Instance.maxBaseDamageOutput.OnValueChanged -=
                    HandleMaxBaseDamageOutputChangedValue;
                WaGameplayController.Instance.maxEntityDeploymentCount.OnValueChanged -=
                    HandleMaxEntityDeploymentCountChangedValue;
                WaGameplayController.Instance.currentBaseDamageOutput.OnValueChanged -=
                    HandleCurrentBaseDamageCausingChangedValue;
                WaGameplayController.Instance.currentEntityDeploymentCount.OnValueChanged -=
                    HandleCurrentEntityDeployedChangedValue;
            }
        }

        #endregion
    }
}