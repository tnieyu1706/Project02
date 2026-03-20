using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.UI;

namespace Game.StrategyBuilding
{
    public class SbGameplayGUI : SingletonBehavior<SbGameplayGUI>
    {
        [SerializeField] private Text moneyNumberText;
        [SerializeField] private Text peopleNumberText;
        [SerializeField] private Text woodNumberText;
        [SerializeField] private Text stoneNumberText;
        [SerializeField] private Text maxPeopleNumberText;
        [SerializeField] private Text maxWoodNumberText;
        [SerializeField] private Text maxStoneNumberText;

        [SerializeField] private Button buildingListBtn;

        private void OnEnable()
        {
            SbGameplayController.Instance.moneyNumber.OnValueChanged += HandleMoneyNumberChangedGUI;
            SbGameplayController.Instance.peopleNumber.OnValueChanged += HandlePeopleNumberChangedGUI;
            SbGameplayController.Instance.woodNumber.OnValueChanged += HandleWoodNumberChangedGUI;
            SbGameplayController.Instance.stoneNumber.OnValueChanged += HandleStoneNumberChangedGUI;
            SbGameplayController.Instance.maxPeopleNumber.OnValueChanged += HandleMaxPeopleNumberChangedGUI;
            SbGameplayController.Instance.maxWoodNumber.OnValueChanged += HandleMaxWoodNumberChangedGUI;
            SbGameplayController.Instance.maxStoneNumber.OnValueChanged += HandleMaxStoneNumberChangedGUI;
            
            buildingListBtn.onClick.AddListener(HandleBuildingListButtonClicked);
        }

        #region EVENT HANDLERS

        private void HandleBuildingListButtonClicked()
        {
            BuildingListUIToolkit.Instance.Show();
        }

        private void HandleMoneyNumberChangedGUI(int changedMoneyNumber)
        {
            moneyNumberText.text = $"{changedMoneyNumber}";
        }

        private void HandlePeopleNumberChangedGUI(int changedPeopleNumber)
        {
            peopleNumberText.text = $"{changedPeopleNumber}";
        }

        private void HandleWoodNumberChangedGUI(int changedWoodNumber)
        {
            woodNumberText.text = $"{changedWoodNumber}";
        }

        private void HandleStoneNumberChangedGUI(int changedStoneNumber)
        {
            stoneNumberText.text = $"{changedStoneNumber}";
        }

        private void HandleMaxPeopleNumberChangedGUI(int changedMaxPeopleNumber)
        {
            maxPeopleNumberText.text = $"{changedMaxPeopleNumber}";
        }

        private void HandleMaxWoodNumberChangedGUI(int changedMaxWoodNumber)
        {
            maxWoodNumberText.text = $"{changedMaxWoodNumber}";
        }

        private void HandleMaxStoneNumberChangedGUI(int changedMaxStoneNumber)
        {
            maxStoneNumberText.text = $"{changedMaxStoneNumber}";
        }

        #endregion

        private void OnDisable()
        {
            buildingListBtn.onClick.RemoveListener(HandleBuildingListButtonClicked);
            
            if (SbGameplayController.Instance == null) return;
            
            SbGameplayController.Instance.moneyNumber.OnValueChanged -= HandleMoneyNumberChangedGUI;
            SbGameplayController.Instance.peopleNumber.OnValueChanged -= HandlePeopleNumberChangedGUI;
            SbGameplayController.Instance.woodNumber.OnValueChanged -= HandleWoodNumberChangedGUI;
            SbGameplayController.Instance.stoneNumber.OnValueChanged -= HandleStoneNumberChangedGUI;
            SbGameplayController.Instance.maxPeopleNumber.OnValueChanged -= HandleMaxPeopleNumberChangedGUI;
            SbGameplayController.Instance.maxWoodNumber.OnValueChanged -= HandleMaxWoodNumberChangedGUI;
            SbGameplayController.Instance.maxStoneNumber.OnValueChanged -= HandleMaxStoneNumberChangedGUI;
        }
    }
}