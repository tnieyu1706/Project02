using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Global.GameController;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Gameplay.Global.Popup
{
    [RequireComponent(typeof(UIDocument))]
    public class GamePopup : SingletonBehavior<GamePopup>
    {
        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private List<StyleSheet> styleSheets;

        private Action onClosed;
        private bool isTimeStop;

        private VisualElement root;
        private Label titleLabel;
        private Label contentLabel;

        async void Start()
        {
            await UniTask.Yield();

            root = uiDocument.rootVisualElement;
            Initialize();
            Close();
        }
        
        public void Display(string title, string content, bool timeStop = false, Action onClosedSource = null)
        {
            onClosed = onClosedSource;
            isTimeStop = timeStop;

            titleLabel.text = title;
            contentLabel.text = content;
            Open();

            if (isTimeStop)
                GameTimeController.SetGameStop();
        }

        private void Initialize()
        {
            root.Clear();
            root.styleSheets.Clear();
            foreach (var style in styleSheets)
            {
                root.styleSheets.Add(style);
            }

            root.AddClass("root");

            var container = root.CreateChild("container");

            var header = container.CreateChild("header");
            titleLabel = header.CreateChild<Label>("title");

            var content = container.CreateChild("content");
            contentLabel = content.CreateChild<Label>("content-text");
            root.RegisterCallback<ClickEvent>(HandleContainerClicked);
        }

        private void HandleContainerClicked(ClickEvent evt)
        {
            if (isTimeStop)
                GameTimeController.SetGameToPreviousState();

            Close();
        }

        private void Open()
        {
            root.style.display = DisplayStyle.Flex;
        }

        private void Close()
        {
            root.style.display = DisplayStyle.None;
            onClosed?.Invoke();
        }
    }
}