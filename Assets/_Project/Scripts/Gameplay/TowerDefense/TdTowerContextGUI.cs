using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using Game.BaseGameplay;
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

        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform guiParentTransform;
        [SerializeField, Required] private GameObject upgradeElementPrefab;
        [SerializeField, Required] private Transform towerRangeDisplayTransform;
        private IObjectPool<TdTowerUpgradeElement> elementPool;
        private readonly List<TdTowerUpgradeElement> activeElements = new();

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();

            elementPool = new ObjectPool<TdTowerUpgradeElement>(
                CreateElement,
                GetElement,
                ReleaseElement,
                DestroyElement,
                true,
                4,
                5
            );
        }

        public void Open()
        {
            canvas.enabled = true;

            BlurBackground.Show();
        }

        public override void Hide()
        {
            canvas.enabled = false;
        }

        #region POOL METHODS

        private TdTowerUpgradeElement CreateElement()
        {
            GameObject elementGo = Instantiate(upgradeElementPrefab, guiParentTransform);
            return new TdTowerUpgradeElement(elementGo);
        }

        private void GetElement(TdTowerUpgradeElement element)
        {
            element.ElementGo.SetActive(true);
        }

        private void ReleaseElement(TdTowerUpgradeElement element)
        {
            element.ElementGo.SetActive(false);
        }

        private void DestroyElement(TdTowerUpgradeElement element)
        {
            if (activeElements.Contains(element))
            {
                activeElements.Remove(element);
            }

            DestroyImmediate(element.ElementGo);
        }

        #endregion

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
            foreach (var active in activeElements)
            {
                elementPool.Release(active);
            }

            activeElements.Clear();

            //setup
            var nextUpgradeTowerPresets = TowerUpgradeTree.Tree[currentTowerPreset.objectId].nextUpgradeTowers;

            //install: upgrade elements
            foreach (var nextPreset in nextUpgradeTowerPresets)
            {
                var element = elementPool.Get();
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

                activeElements.Add(element);
            }

            //install: event elements
            foreach (var uiEvent in currentTowerPreset.towerEvents)
            {
                var element = elementPool.Get();

                element.SetElement(
                    uiEvent.eventIcon,
                    0,
                    () =>
                    {
                        uiEvent.OnCall(CurrentContext);

                        Hide();
                        BlurBackground.CloseManual();
                    });

                activeElements.Add(element);
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