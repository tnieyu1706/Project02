using System.Collections.Generic;
using _Project.Scripts.Gameplay.Global.Tooltip;
using Cysharp.Threading.Tasks;
using Game.BaseGameplay;
using KBCore.Refs;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Game.BuildingGameplay
{
    [RequireComponent(typeof(UIDocument))]
    public class EnemyBaseEventDisplayUIToolkit : BehaviourDisplayUI
    {
        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private List<StyleSheet> styleSheets;

        private VisualElement root;
        private VisualElement contentContainer;
        private readonly List<EnemyBaseElement> elements = new();

        protected override void Awake()
        {
            base.Awake();
            SbGameplayGUI.Instance.enemyBaseEventDisplay = this;
        }

        async void Start()
        {
            await UniTask.Yield();

            root = uiDocument.rootVisualElement;
            Initialize();
            Hide();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (SbGameplayGUI.Instance != null)
            {
                SbGameplayGUI.Instance.enemyBaseEventDisplay = null;
            }
        }

        private void Initialize()
        {
            root.styleSheets.Clear();
            foreach (var sheet in styleSheets)
            {
                root.styleSheets.Add(sheet);
            }

            var container = root.CreateChild("container");

            var header = container.CreateChild("header");
            header.CreateChild<Label>("header__title")
                .text = "[Enemy Base]";

            contentContainer = container.CreateChild("content");
            // Đảm bảo content chiếm hết không gian còn lại để có vùng random rộng
            contentContainer.style.flexGrow = 1;

            elements.Clear();
            foreach (var baseConfig in SbGameplayController.Instance.currentLevel.events)
            {
                var el = contentContainer.CreateChild<EnemyBaseElement>();
                
                el.eventImg.sprite = baseConfig.icon;
                el.SetData(baseConfig.data);
                elements.Add(el);
            }

            // Đợi UI layout xong để lấy width/height thực tế rồi mới tính toán vị trí
            contentContainer.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);
        }

        private void OnContentGeometryChanged(GeometryChangedEvent evt)
        {
            // Tránh tính toán lại nếu kích thước không đổi đáng kể
            if (evt.newRect.width < 1 || evt.newRect.height < 1) return;

            float containerWidth = evt.newRect.width;
            float containerHeight = evt.newRect.height;

            // Giả định size icon là 80px (khớp với USS)
            float iconSize = 80f;
            float padding = 10f;

            foreach (var el in elements)
            {
                // Thuật toán random vị trí đảm bảo nằm trong boundary
                float randomX = Random.Range(padding, containerWidth - iconSize - padding);
                float randomY = Random.Range(padding, containerHeight - iconSize - padding);

                el.style.left = randomX;
                el.style.top = randomY;
            }

            // Chỉ chạy một lần khi mở hoặc khi layout thay đổi lớn
            contentContainer.UnregisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);
        }

        public void Open()
        {
            root.style.display = DisplayStyle.Flex;
            // Khi mở lại, có thể cần tính toán lại vị trí nếu muốn "fresh" mỗi lần
            // contentContainer.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);

            BlurBackground.Show();
        }

        public override void Hide()
        {
            root.style.display = DisplayStyle.None;
            TextTooltipController.Instance.Hide();
        }
    }

    public class EnemyBaseElement : VisualElement
    {
        private EventData baseConfig;
        public Image eventImg;

        public EnemyBaseElement()
        {
            this.AddToClassList("enemy-base-element");
            // Set absolute ngay trong code hoặc qua USS
            this.style.position = Position.Absolute;
            eventImg = this.CreateChild<Image>("enemy-base-icon");
        }

        public void SetData(EventData baseConfigSource)
        {
            baseConfig = baseConfigSource;
            this.RegisterCallback<PointerEnterEvent>(HandlePointerEnter);
            this.RegisterCallback<PointerLeaveEvent>(HandlePointerLeave);
            this.RegisterCallback<ClickEvent>(HandleEnemyBaseClicked);

            this.SetEnabled(!baseConfigSource.isCompleted);
        }

        private void HandlePointerEnter(PointerEnterEvent evt)
        {
            string status = baseConfig.isCompleted
                ? "<color=#00FF00>COMPLETED</color>"
                : "<color=#FF5555>ACTIVE</color>";
            string contentText = $"Level: {baseConfig.levelType}\nStatus: {status}";
            TextTooltipController.Instance.Display(contentText, evt.position);
        }

        private void HandlePointerLeave(PointerLeaveEvent evt)
        {
            TextTooltipController.Instance.Hide();
        }

        private void HandleEnemyBaseClicked(ClickEvent _)
        {
            EventInfoUIToolkit.Instance.Display(baseConfig);
        }
    }
}