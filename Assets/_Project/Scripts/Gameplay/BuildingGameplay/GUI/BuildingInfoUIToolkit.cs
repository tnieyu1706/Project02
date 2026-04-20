using Cysharp.Threading.Tasks;
using EditorAttributes;
using Game.StrategyBuilding;
using KBCore.Refs;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.BuildingGameplay
{
    [RequireComponent(typeof(UIDocument))]
    public class BuildingInfoUIToolkit : SingletonDisplayUI<BuildingInfoUIToolkit>
    {
        [SerializeField, Self] private UIDocument uiDocument;

        private VisualElement root;

        private IBuildingUI buildingUITemp;

        private async void Start()
        {
            await UniTask.Yield();

            root = uiDocument.rootVisualElement;
            Initialize();
            Hide();
        }

        private void Initialize()
        {
            root.Clear();
        }

        public void Display(IBuildingUI buildingUI)
        {
            if (buildingUITemp != null)
            {
                buildingUITemp.DetachUIFromPanel(root);
                buildingUITemp = null;
            }

            buildingUITemp = buildingUI;
            Show();
            buildingUITemp.AttachUIToPanel(root);
        }

        [Button]
        private void Show()
        {
            root.style.display = DisplayStyle.Flex;
            BlurBackground.Show();
        }

        [Button]
        public override void Hide()
        {
            if (buildingUITemp != null)
            {
                buildingUITemp.DetachUIFromPanel(root);
                buildingUITemp = null;
            }

            root.style.display = DisplayStyle.None;
        }
    }
}