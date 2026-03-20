using KBCore.Refs;
using TnieYuPackage.Utils.Structures;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.StrategyBuilding
{
    [RequireComponent(typeof(UIDocument))]
    public class BuildingInfoUIToolkit : SingletonGUIBehaviour<BuildingInfoUIToolkit>
    {
        [SerializeField, Self] private UIDocument uiDocument;

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            
            base.Awake();
        }

        public void Display(BuildingRuntime buildingRuntimeSource)
        {
            Show();
            buildingRuntimeSource.buildingBehaviour.Render(uiDocument.rootVisualElement);
        }

        public void Show()
        {
            BlurBackground.Open();
            Instance.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            Instance.gameObject.SetActive(false);
        }
    }
}