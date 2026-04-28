using System.Collections.Generic;
using _Project.Scripts.Gameplay.Global.GameController;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Game.BaseGameplay;
using Gameplay.Global;
using KBCore.Refs;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.BuildingGameplay
{
    [RequireComponent(typeof(UIDocument))]
    public class EventInfoUIToolkit : SingletonDisplayUI<EventInfoUIToolkit>
    {
        [SerializeField, Self] private UIDocument uiDocument;
        public List<StyleSheet> styleSheets;

        private static EventData currentConfig;

        private VisualElement root;
        private Label titleLabel;
        private Label contentLabel;
        private VisualElement awardsContainer;
        private Button attackButton;

        private bool isStopTimer = false;
        private int preFrameRate;
        private float preTimeScale;

        private async void Start()
        {
            await UniTask.Yield();
            root = uiDocument.rootVisualElement;
            Initialize();
            Hide();
        }

        public void Display(EventData eventData, bool stopTimer = false)
        {
            currentConfig = eventData;
            Open(stopTimer);

            titleLabel.text = eventData.eventName;
            var gameLevel = eventData.GetGameplayLevel();
            contentLabel.text = gameLevel.GetDisplayedText();
            attackButton.text = eventData.GetEventHandlerName();
            
            awardsContainer.Clear();
            foreach (var award in eventData.Awards)
            {
                awardsContainer.CreateChild<Label>("award-container__award", "content__text")
                        .text = $"[{award.Key}]: {award.Value:F1}";
            }
            
            if (eventData.isCompleted)
            {
                attackButton.SetEnabled(false);
                attackButton.RemoveClass("button-active");
                attackButton.AddClass("button-disable");
            }
            else
            {
                attackButton.SetEnabled(true);
                attackButton.RemoveClass("button-disable");
                attackButton.AddClass("button-active");
            }
        }

        private void Initialize()
        {
            root.Clear();
            root.styleSheets.Clear();
            foreach (var styleSheet in styleSheets)
            {
                root.styleSheets.Add(styleSheet);
            }

            root.AddClass("root");
            root.pickingMode = PickingMode.Ignore;

            var container = root.CreateChild("container");

            var header = container.CreateChild("header");
            titleLabel = header.CreateChild<Label>("header__title");

            var content = container.CreateChild("content");

            var contentLeft = content.CreateChild("content-left");

            contentLeft.CreateChild<Label>("content__title")
                .text = "INFOS";

            contentLabel = contentLeft.CreateChild<Label>("content-left__level", "content__text");

            var contentRight = content.CreateChild("content-right");

            contentRight.CreateChild<Label>("content__title")
                .text = "AWARDS";
            awardsContainer = contentRight.CreateChild("award-container");

            var footer = container.CreateChild("footer");
            attackButton = footer.CreateChild<Button>("footer__attack-button");
            attackButton.clicked += HandleAttackButtonClicked;
        }

        private static void HandleAttackButtonClicked()
        {
            Instance.Hide();
            GameplayTransition.LoadBaseGameplayWithEvent(currentConfig).Forget();
        }

        [Button]
        private void Open(bool stopTimer)
        {
            root.style.display = DisplayStyle.Flex;
            BlurBackground.Show();
            isStopTimer = stopTimer;
            if (!isStopTimer) return;

            preFrameRate = GameTimeController.TargetFrameRate;
            preTimeScale = GameTimeController.TimeScale;
            GameTimeController.SetGameStop();
        }

        [Button]
        public override void Hide()
        {
            root.style.display = DisplayStyle.None;

            if (!isStopTimer) return;
            GameTimeController.SetFrameRate(preFrameRate);
            GameTimeController.SetTimeScale(preTimeScale);
            isStopTimer = false;
        }
    }
}