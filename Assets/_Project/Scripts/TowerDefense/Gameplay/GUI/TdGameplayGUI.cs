using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Td
{
    public class TdGameplayGUI : SingletonBehavior<TdGameplayGUI>
    {
        [SerializeField, Required] private Text textMapHealth;
        [SerializeField, Required] private Text textWaveInfo;
        [SerializeField, Required] private Text textMoneyInfo;
        [Required] public Image screenImage;

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
        }

        public void OnEnable()
        {
            TdGameplayController.Instance.OnHealthChange += HandleMapHealthChangedGUI;
            TdGameplayController.Instance.OnMoneyChange += HandleMoneyInfoChangedGUI;
            TdGameplayController.Instance.tdWaveController.OnCurrentWaveIndexChanged += HandleWaveInfoChangedGUI;
        }

        public void OnDisable()
        {
            if (TdGameplayController.Instance == null) return;

            TdGameplayController.Instance.OnHealthChange -= HandleMapHealthChangedGUI;
            TdGameplayController.Instance.OnMoneyChange -= HandleMoneyInfoChangedGUI;
            TdGameplayController.Instance.tdWaveController.OnCurrentWaveIndexChanged -= HandleWaveInfoChangedGUI;
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

        public void SceneClick()
        {
            TdTowerContextGUI.TurnOff();
        }
    }
}