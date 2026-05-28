using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using Game.BaseGameplay;
using Game.Global;
using Reflex.Attributes;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.TowerDefense
{
    public class TdTowerContextGUI : SingletonDisplayUI<TdTowerContextGUI>
    {
        public static TowerRuntime CurrentContext;

        [Inject] TowerUpgradeTree towerUpgradeTree;

        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform guiParentTransform;
        [SerializeField, Required] private GameObject upgradeElementPrefab;
        [SerializeField, Required] private Transform towerRangeDisplayTransform;
        [SerializeField] private int maxElements = 8;
        private List<TdTowerUpgradeElement> elements;

        private List<TowerType> filteredTowerTypes = new();
        private List<TowerLevel> filteredTowerLevels = new();

        protected override void Awake()
        {
            base.Awake();

            Initialize();
        }

        private void Initialize()
        {
            elements = new(8);
            for (int i = 0; i < maxElements; i++)
            {
                var element = CreateElement();
                element.ElementGo.SetActive(false);
                elements.Add(element);
            }
        }

        public void Open()
        {
            canvas.enabled = true;

            BlurBackground.Show();
            TdInteractSystem.Instance.enabled = false;
        }

        public override void Hide()
        {
            canvas.enabled = false;
            TdInteractSystem.Instance.enabled = true;

            // disable all elements
            var actives = elements.Where(e => e.ElementGo.activeSelf);

            foreach (var active in actives)
            {
                active.ElementGo.SetActive(false);
            }
        }

        private TdTowerUpgradeElement CreateElement()
        {
            GameObject elementGo = Instantiate(upgradeElementPrefab, guiParentTransform);
            return new TdTowerUpgradeElement(elementGo);
        }

        private TdTowerUpgradeElement GetElement()
        {
            foreach (var element in elements)
            {
                if (!element.ElementGo.activeSelf) return element;
            }

            return null;
        }

        public void Display(TowerRuntime towerRuntime)
        {
            Open();

            filteredTowerTypes.Clear();
            filteredTowerTypes = GamePropertiesRuntime.Instance
                .UnlockTowerTypeDict
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            filteredTowerLevels.Clear();
            filteredTowerLevels = GamePropertiesRuntime.Instance
                .UnlockTowerLevelDict
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            // current: only display first interact range.
            var firstInteract = towerRuntime.InteractStrategyList.FirstOrDefault();
            if (firstInteract != null)
            {
                towerRangeDisplayTransform.localScale = firstInteract.Installer.interactRange * Vector3.one;
            }

            DisplayTowerUpgradeElements(towerRuntime);
        }

        private void DisplayTowerUpgradeElements(TowerRuntime towerRuntime)
        {
            transform.position = (Vector2)towerRuntime.transform.position;

            CurrentContext = towerRuntime;
            var currentTowerPreset = towerRuntime.currentPreset;

            //pre-install

            //setup
            var nextUpgradeTowerPresets = towerUpgradeTree.Tree[currentTowerPreset.objectId].nextUpgradeTowers;

            var validateNextTowerPresets = nextUpgradeTowerPresets
                .Where(p => filteredTowerTypes.Contains(p.towerType) && filteredTowerLevels.Contains(p.towerLevel))
                .ToList();

            //install: upgrade elements
            foreach (var nextPreset in validateNextTowerPresets)
            {
                var element = GetElement();
                if (element == null) return;
                element.ElementGo.SetActive(true);

                int cost = TowerPresetSo.CalculateCost(currentTowerPreset, nextPreset);

                element.SetElement(
                    nextPreset.towerIcon,
                    cost,
                    () =>
                    {
                        CurrentContext.Setup(nextPreset);

                        Hide();
                        BlurBackground.CloseAll();
                    }
                );
            }

            //install: event elements
            foreach (var uiEvent in currentTowerPreset.towerEvents)
            {
                var element = GetElement();
                if (element == null) return;
                element.ElementGo.SetActive(true);

                element.SetElement(
                    uiEvent.eventIcon,
                    0,
                    () =>
                    {
                        uiEvent.OnCall(CurrentContext);

                        Hide();
                        BlurBackground.CloseManual();
                    });
            }
        }
    }

    public class TdTowerUpgradeElement
    {
        public GameObject ElementGo { get; }
        private readonly Image elementImage;
        private readonly Button elementButton;
        private readonly Text elementText;

        public TdTowerUpgradeElement(GameObject elementGo)
        {
            ElementGo = elementGo;

            elementGo.TryGetComponent(out elementImage);
            elementGo.TryGetComponent(out elementButton);
            elementText = elementGo.GetComponentInChildren<Text>();
        }

        private void SetElement(Sprite icon, UnityAction onClick)
        {
            elementImage.sprite = icon;
            elementButton.onClick.RemoveAllListeners();
            elementButton.onClick.AddListener(onClick);
        }

        public void SetElement(Sprite icon, int cost, UnityAction onClick)
        {
            if (elementText != null)
                elementText.text = cost <= 0 ? "" : cost.ToString();
            SetElement(icon, onClick);

            elementButton.interactable = BaseGameplayController.Instance.money.Value >= cost;
        }
    }
}