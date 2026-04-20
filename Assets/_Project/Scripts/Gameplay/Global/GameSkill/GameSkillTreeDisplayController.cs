using System.Linq;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using SerializeButton = EditorAttributes.ButtonAttribute;

namespace Game.Global
{
    [RequireComponent(typeof(UIDocument))]
    public class GameSkillTreeDisplayController : SingletonBehavior<GameSkillTreeDisplayController>, IDisplayGUI
    {
        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private StyleSheet skillTreeStyle;
        
        private VisualElement root;
        private GameSkillTreeUI skillTreeUI;

        public GameSkillTree SkillTree;
        public GameSkillTreeRadialLayout SkillTreeLayout;

        [Header("Layout Properties")] [SerializeField]
        private float radiusStep = 180f;

        [SerializeField] private float angleSpread = 360f;
        [SerializeField] private float layoutRotation = -90f;

        private async void Start()
        {
            await UniTask.Yield();

            SetupSkillTreeData();

            root = uiDocument.rootVisualElement;
            Initialize();
            Hide();
        }

        protected void OnDestroy()
        {
            if (skillTreeUI != null)
            {
                skillTreeUI.OnNodeClicked -= HandleSkillNodeClicked;
            }
        }

        private void SetupSkillTreeData()
        {
            var rootSkill = GameSkillDataManager.Instance.Refs[GameSkillData.ROOT_SKILL_NAME];
            var gameSkills = GameSkillDataManager.Instance.Refs.Values.ToList();

            SkillTree = new GameSkillTree(gameSkills, rootSkill);
            SkillTreeLayout = new GameSkillTreeRadialLayout(radiusStep, angleSpread, layoutRotation);
        }

        private void HandleScreenClicked(ClickEvent evt)
        {
            Hide();
            evt.StopPropagation();
        }

        private void HandleIntervalClicked(ClickEvent evt)
        {
            evt.StopPropagation();
        }

        private void Initialize()
        {
            root.Clear();

            root.RegisterCallback<ClickEvent>(HandleScreenClicked);

            skillTreeUI = new GameSkillTreeUI(skillTreeStyle);
            skillTreeUI.Initialize(SkillTree, SkillTreeLayout.CalculateLayout(SkillTree));
            skillTreeUI.AddTo(root);

            skillTreeUI.Viewport.RegisterCallback<ClickEvent>(HandleIntervalClicked);

            skillTreeUI.OnNodeClicked += HandleSkillNodeClicked;
        }

        private void HandleSkillNodeClicked(GameSkillTreeNode skillNode)
        {
            var skillData = skillNode.Data;
            Debug.Log($"Clicked Skill: {skillData.skillName} ({skillData.skillId})");

            if (!skillNode.NodeState.CanInteract || !CheckSkillPointValid(skillData)) return;

            skillNode.ChangeState<UnlockState>();
            GamePropertiesRuntime.Instance.SkillPoints.Value -= skillData.requiredSkillPoint;
        }

        private static bool CheckSkillPointValid(GameSkillData skillData)
        {
            return GamePropertiesRuntime.Instance.SkillPoints.Value >= skillData.requiredSkillPoint;
        }

        [SerializeButton]
        public void Open()
        {
            root.style.display = DisplayStyle.Flex;
        }

        [SerializeButton]
        public void Hide()
        {
            root.style.display = DisplayStyle.None;
        }
    }
}