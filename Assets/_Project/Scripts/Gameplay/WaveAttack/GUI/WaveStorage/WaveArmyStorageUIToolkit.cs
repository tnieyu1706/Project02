using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Global;
using KBCore.Refs;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.WaveAttack
{
    [RequireComponent(typeof(UIDocument))]
    public class ArmyStorageUIToolkit : MonoBehaviour
    {
        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private List<StyleSheet> styleSheets = new();
        [SerializeField] private ArmyStorageSoapDataSo armyStorageSoap;
        [SerializeField] private string storageTitle;

        // Tối ưu hóa: Dùng Dictionary thay vì List<Action> để tránh tạo closure không cần thiết
        private readonly Dictionary<ArmyType, Action<int>> armyPropertiesEvents = new();

        public virtual Dictionary<ArmyType, ObservableValue<int>> ArmyStorage =>
            armyStorageSoap.data.Value.Dictionary;

        // Tối ưu hóa: Chờ 1 frame bằng UniTask để đảm bảo UI Tree đã sẵn sàng 100%
        private async void Start()
        {
            await UniTask.Yield();
            Initialize(uiDocument.rootVisualElement);
        }

        private void Initialize(VisualElement root)
        {
            root.styleSheets.Clear();
            foreach (var styleSheet in styleSheets)
            {
                root.styleSheets.Add(styleSheet);
            }

            var mainContainer = new FoldoutElement();
            mainContainer.AddClass("main-foldout");
            root.Add(mainContainer);

            mainContainer.Header.CreateChild<Label>("header__title")
                .text = storageTitle;

            // 4. Setup Data
            foreach (var armyType in Enum.GetValues(typeof(ArmyType)).OfType<ArmyType>())
            {
                // Tối ưu hóa: Khởi tạo trực tiếp không cần hàm ảo (virtual) cho ngắn gọn
                var armyStorageElement = new WaveArmyStorageElement(armyType);
                armyStorageElement.AddTo(mainContainer.Content);

                var armyStorageData = ArmyStorage[armyType];
                armyStorageElement.ValueLabel.text = armyStorageData.Value.ToString();
                armyStorageData.OnValueChanged += armyStorageElement.OnValueChanged;

                armyPropertiesEvents[armyType] = armyStorageElement.OnValueChanged;
            }
        }

        // Dọn dẹp event khi object bị hủy thông qua Dictionary
        private void OnDestroy()
        {
            foreach (var cleanupAction in armyPropertiesEvents)
            {
                ArmyStorage[cleanupAction.Key].OnValueChanged -= cleanupAction.Value;
            }

            armyPropertiesEvents.Clear();
        }
    }
}