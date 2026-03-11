using System.Collections.Generic;
using EditorAttributes;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.Td
{
    public class TdTowerContextGUI : SingletonBehavior<TdTowerContextGUI>
    {
        public static TowerRuntime CurrentContext;

        [SerializeField, Required] private GameObject upgradeElementPrefab;
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

        #region POOL METHODS

        private TdTowerUpgradeElement CreateElement()
        {
            GameObject elementGo = Instantiate(upgradeElementPrefab, transform);
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
            GameObject elementGo = element.ElementGo;

            if (activeElements.Contains(element))
            {
                activeElements.Remove(element);
            }

            DestroyImmediate(element.ElementGo);
        }

        #endregion

        public void DisplayTowerUpgradeElements(TowerRuntime towerRuntime)
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
            var nextUpgradeTowerPresets = TowerUpgradeTree.Tree[currentTowerPreset.towerId].nextUpgradeTowers;

            //install: upgrade elements
            foreach (var nextPreset in nextUpgradeTowerPresets)
            {
                var element = elementPool.Get();
                int cost = nextPreset.towerPriceValue - currentTowerPreset.towerPriceValue;

                element.SetElement(
                    nextPreset.towerIcon,
                    cost,
                    () =>
                    {
                        CurrentContext.Setup(nextPreset);
                        TdGameplayController.Instance.Money -= cost;
                        TurnOff();
                    }
                );

                activeElements.Add(element);
            }

            //install: event elements
            foreach (var uiEvent in currentTowerPreset.uiEvents)
            {
                var element = elementPool.Get();

                element.SetElement(
                    uiEvent.Icon,
                    0,
                    () =>
                    {
                        uiEvent.Perform();
                        TurnOff();
                    });

                activeElements.Add(element);
            }
        }

        public static void TurnOn()
        {
            Instance.gameObject.SetActive(true);
            PhysicsInteractController.Instance.TurnOff();
            TdGameplayGUI.Instance.screenImage.enabled = true;
        }

        public static void TurnOff()
        {
            Instance.gameObject.SetActive(false);
            PhysicsInteractController.Instance.TurnOn();
            TdGameplayGUI.Instance.screenImage.enabled = false;
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

            if (TdGameplayController.Instance.Money < cost)
            {
                elementButton.interactable = false;
            }
            else
            {
                elementButton.interactable = true;
            }
        }
    }
}