using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using Game.BaseGameplay;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.TowerDefense
{
    public class TdTowerContextGUI : SingletonDisplayUI<TdTowerContextGUI>
    {
        public static TowerRuntime CurrentContext;

        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform guiParentTransform;
        [SerializeField, Required] private GameObject upgradeElementPrefab;
        [SerializeField, Required] private Transform towerRangeDisplayTransform;
        [SerializeField] private int maxElements = 8;
        private List<TdTowerUpgradeElement> elements;

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();

            Initialize();
        }

        public void Open()
        {
            canvas.enabled = true;
            TdInteractSystem.Instance.enabled = false;

            BlurBackground.Show();
        }

        public override void Hide()
        {
            canvas.enabled = false;
            TdInteractSystem.Instance.enabled = true;

            ReleaseAllElements();
        }

        private void Initialize()
        {
            elements = new List<TdTowerUpgradeElement>(8);
            for (int i = 0; i < maxElements; i++)
            {
                var e = CreateElement();
                e.ElementGo.SetActive(false);
                elements.Add(e);
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
                if (!element.ElementGo.activeSelf)
                {
                    element.ElementGo.SetActive(true);
                    return element;
                }
            }

            return null;
        }

        private void ReleaseAllElements()
        {
            foreach (var element in elements)
            {
                if (element.ElementGo.activeSelf)
                {
                    element.ElementGo.SetActive(false);
                }
            }
        }

        public void Display(TowerRuntime towerRuntime)
        {
            Open();
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
            ReleaseAllElements();

            //setup
            var nextUpgradeTowerPresets = TowerUpgradeTree.Tree[currentTowerPreset.objectId].nextUpgradeTowers;

            //install: upgrade elements
            foreach (var nextPreset in nextUpgradeTowerPresets)
            {
                var element = GetElement();
                if (element == null) return;

                int cost = TowerPresetSo.CalculateCost(currentTowerPreset, nextPreset);

                element.SetElement(
                    nextPreset.towerIcon,
                    cost,
                    () =>
                    {
                        CurrentContext.Setup(nextPreset);

                        Hide();
                        BlurBackground.CloseManual();
                    }
                );
            }

            //install: event elements
            foreach (var uiEvent in currentTowerPreset.towerEvents)
            {
                var element = GetElement();
                if (element == null) return;

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