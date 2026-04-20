using Game.Global;
using TnieYuPackage.GlobalExtensions;
using UnityEngine.UIElements;

namespace Game.WaveAttack
{
    public class WaveArmyStorageElement : VisualElement
    {
        private readonly ArmyType armyType;

        public readonly Label TitleLabel;
        public readonly Label ValueLabel;

        public WaveArmyStorageElement(ArmyType armyType)
        {
            this.armyType = armyType;
            this.AddClass("army-storage");

            var textArea = this.CreateChild("text-area");
            TitleLabel = textArea.CreateChild<Label>("army-storage__label");
            TitleLabel.text = $"{armyType.ToString()}:";

            ValueLabel = textArea.CreateChild<Label>("army-storage__value");

            var buttonArea = this.CreateChild("button-area");
            var minusButton = buttonArea.CreateChild<Button>("army-storage__minus-btn");
            minusButton.text = "-";
            minusButton.clicked += HandleMinusButtonClicked;
            var addButton = buttonArea.CreateChild<Button>("army-storage__add-btn");
            addButton.text = "+";
            addButton.clicked += HandleAddButtonClicked;
        }

        public void OnValueChanged(int changedValue)
        {
            ValueLabel.text = changedValue.ToString();
        }

        private void HandleAddButtonClicked()
        {
            WaGameplayController.Instance.AddArmyForWave(armyType, 1);
        }

        private void HandleMinusButtonClicked()
        {
            WaGameplayController.Instance.AddArmyForWave(armyType, -1);
        }
    }
}