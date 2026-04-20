using System.Collections.Generic;
using KBCore.Refs;
using TnieYuPackage.DesignPatterns;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Global
{
    [RequireComponent(typeof(UIDocument))]
    public class GameSkillTreeDisplayTest : SingletonBehavior<GameSkillTreeDisplayTest>
    {
        [SerializeField, Self] private UIDocument uiDocument;
        [SerializeField] private StyleSheet skillTreeStyle;

        private GameSkillTreeUI _skillTreeUI;

        private void OnEnable()
        {
            this.ValidateRefs();

            // 1. Setup Test Data
            var allSkills = GetTestSkillData(out var rootSkill);

            // 2. Initialize Logic Tree
            var tree = new GameSkillTree(allSkills, rootSkill);

            // 3. Setup Layout config
            var layout = new GameSkillTreeRadialLayout
            {
                RadiusStep = 180f,
                AngleSpread = 360f,
                Rotation = -90f // Make root point upward
            };

            // 4. Create and Add UI
            _skillTreeUI ??= new GameSkillTreeUI(skillTreeStyle);
            uiDocument.rootVisualElement.Add(_skillTreeUI);

            // 5. Display
            _skillTreeUI.Initialize(tree, layout.CalculateLayout(tree));

            _skillTreeUI.OnNodeClicked += (node) =>
            {
                Debug.Log($"Selected Skill: {node.Data.skillName} ({node.Data.skillId})");
            };
        }

        /// <summary>
        /// Generates a dummy skill tree structure for testing.
        /// </summary>
        private List<GameSkillData> GetTestSkillData(out GameSkillData root)
        {
            var list = new List<GameSkillData>();

            // Helper to create data in memory without needing Asset files
            GameSkillData Create(string id, string name)
            {
                var data = ScriptableObject.CreateInstance<GameSkillData>();
                data.skillId = id;
                data.skillName = name;
                list.Add(data);
                return data;
            }

            // Define Nodes
            root = Create("root", "Origin");
            var s1 = Create("s1", "Warrior Path");
            var s2 = Create("s2", "Mage Path");
            var s3 = Create("s3", "Rogue Path");

            var s1_1 = Create("s1_1", "Strength");
            var s1_2 = Create("s1_2", "Stamina");
            var s1_1_1 = Create("s1_1_1", "Greatsword");

            var s2_1 = Create("s2_1", "Intelligence");
            var s3_1 = Create("s3_1", "Stealth");

            // Build Connections
            root.nextSkills.AddRange(new[] { s1, s2, s3 });
            s1.nextSkills.AddRange(new[] { s1_1, s1_2 });
            s1_1.nextSkills.Add(s1_1_1);
            s2.nextSkills.Add(s2_1);
            s3.nextSkills.Add(s3_1);

            return list;
        }
    }
}