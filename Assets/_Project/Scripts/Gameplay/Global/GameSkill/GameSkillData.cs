using System;
using System.Collections.Generic;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace Game.Global
{
    [CreateAssetMenu(fileName = "GameSkill", menuName = "Game/Global/GameSkill")]
    public class GameSkillData : ScriptableObject
    {
        public const string ROOT_SKILL_NAME = "RootSkill";

        public string skillId;
        public string skillName;
        public int requiredSkillPoint;

        public Sprite skillIcon;
        [TextArea(3, 10)] public string skillDescription;

        [SerializeReference]
        [AbstractSupport(
            classAssembly: typeof(ISkillInfluencer),
            abstractTypes: typeof(ISkillInfluencer)
        )]
        public ISkillInfluencer skillInfluencer;

        public List<GameSkillData> nextSkills = new();
    }

    public interface ISkillInfluencer
    {
        void ApplyAffect();
    }

    [Serializable]
    public class NoneSkillInfluencer : ISkillInfluencer
    {
        public void ApplyAffect()
        {
            // No Effect
        }
    }
}