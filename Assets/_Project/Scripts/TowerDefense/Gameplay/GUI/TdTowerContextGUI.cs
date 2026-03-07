using System.Collections.Generic;
using System.Linq;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.Td
{
    public class TdTowerContextGUI : SingletonBehavior<TdTowerContextGUI>
    {
        public static TowerRuntime CurrentTowerContextRuntime;

        [SerializeField] private GameObject elementPrefab;

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
            GameObject elementGo = Instantiate(elementPrefab, transform);
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

            CurrentTowerContextRuntime = towerRuntime;
            var currentTowerPreset = towerRuntime.currentPreset;

            //pre-install
            foreach (var active in activeElements)
            {
                elementPool.Release(active);
            }

            activeElements.Clear();

            //setup
            var nextUpgradeIds = TowerUpgradeTree.Tree[currentTowerPreset.towerId].NextUpgradeTowerIds;

            Dictionary<string, TowerPresetSo> nextUpgradePresets = nextUpgradeIds
                .ToDictionary(id => id, id => TowerUpgradeTree.Tree[id].towerPreset);

            //install
            foreach (var nextUpgradeKvp in nextUpgradePresets)
            {
                var element = elementPool.Get();

                element.SetElement(
                    nextUpgradeKvp.Value.towerIcon,
                    () =>
                    {
                        CurrentTowerContextRuntime.UpgradeContext(nextUpgradeKvp.Key);
                        TurnOff();
                    }
                );

                activeElements.Add(element);
            }

            foreach (var uiEvent in currentTowerPreset.uiEvents)
            {
                var element = elementPool.Get();

                element.SetElement(uiEvent.Icon, () =>
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

        public TdTowerUpgradeElement(GameObject elementGo)
        {
            ElementGo = elementGo;

            elementGo.TryGetComponent(out elementImage);
            elementGo.TryGetComponent(out elementButton);
        }

        public void SetElement(Sprite icon, UnityAction onClick)
        {
            elementImage.sprite = icon;
            elementButton.onClick.RemoveAllListeners();
            elementButton.onClick.AddListener(onClick);
        }
    }
}