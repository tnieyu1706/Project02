using EditorAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Td
{
    public class TdGameplayGUI : MonoBehaviour
    {
        [SerializeField, Required] private Text textMapHealth;
        [SerializeField, Required] private Text textWaveInfo;
        [SerializeField, Required] private Text textMoneyInfo;

        public void OnEnable()
        {
            TdGameplayController.Instance.OnHealthChange += HandleMapHealthChangedGUI;
            TdGameplayController.Instance.OnMoneyChange += HandleMoneyInfoChangedGUI;
            TdGameplayController.Instance.WaveController.OnCurrentWaveIndexChanged += HandleWaveInfoChangedGUI;
        }

        public void OnDisable()
        {
            if (TdGameplayController.Instance == null) return;

            TdGameplayController.Instance.OnHealthChange -= HandleMapHealthChangedGUI;
            TdGameplayController.Instance.OnMoneyChange -= HandleMoneyInfoChangedGUI;
            TdGameplayController.Instance.WaveController.OnCurrentWaveIndexChanged -= HandleWaveInfoChangedGUI;
        }

        private void HandleMapHealthChangedGUI(int changedHealth)
        {
            textMapHealth.text = $"{changedHealth}";
        }

        private void HandleWaveInfoChangedGUI(int changedWave, int maxWave)
        {
            textWaveInfo.text = $"{changedWave}/{maxWave}";
        }

        private void HandleMoneyInfoChangedGUI(int changedMoney)
        {
            textMoneyInfo.text = $"{changedMoney}";
        }
    }
}