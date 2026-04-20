using System;
using System.Collections.Generic;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Global
{
    #region State

    public abstract class BaseState
    {
        protected GameSkillTreeNode SkillNode { get; }
        public bool CanInteract { get; } = false;

        protected BaseState(GameSkillTreeNode skillNode, bool canInteract)
        {
            SkillNode = skillNode;
            CanInteract = canInteract;
        }

        public abstract void Entry();
        public abstract void Exit();
        public abstract void HandleUI(Image uiImage);
    }

    public class LockState : BaseState
    {
        public LockState(GameSkillTreeNode skillNode) : base(skillNode, false)
        {
        }

        public override void Entry()
        {
        }

        public override void Exit()
        {
        }

        public override void HandleUI(Image uiImage)
        {
            uiImage.ClearClassList();
            uiImage.AddClass(GameSkillTreeNode.DEFAULT_CLASS_NAME);
            uiImage.AddClass("skill-lock-state");
        }
    }

    public class AllowState : BaseState
    {
        public AllowState(GameSkillTreeNode skillNode) : base(skillNode, true)
        {
        }

        public override void Entry()
        {
        }

        public override void Exit()
        {
        }

        public override void HandleUI(Image uiImage)
        {
            uiImage.ClearClassList();
            uiImage.AddClass(GameSkillTreeNode.DEFAULT_CLASS_NAME);
            uiImage.AddClass("skill-allow-state");
        }
    }

    public class UnlockState : BaseState
    {
        public UnlockState(GameSkillTreeNode skillNode) : base(skillNode, false)
        {
        }

        public override void Entry()
        {
            SkillNode.RefreshChildren();
            SkillNode.Data.skillInfluencer?.ApplyAffect();
        }

        public override void Exit()
        {
        }

        public override void HandleUI(Image uiImage)
        {
            uiImage.ClearClassList();
            uiImage.AddClass(GameSkillTreeNode.DEFAULT_CLASS_NAME);
            uiImage.AddClass("skill-unlock-state");
        }
    }

    #endregion

    public class GameSkillTreeNode
    {
        public const string DEFAULT_CLASS_NAME = "skill-node";
        
        public readonly GameSkillData Data;
        public BaseState NodeState { get; private set; }
        public readonly List<GameSkillTreeNode> Children;

        public event Action<BaseState> NodeStateChanged;

        public GameSkillTreeNode(GameSkillData data, List<GameSkillTreeNode> children)
        {
            this.Data = data;
            this.Children = children;
            NodeState = new LockState(this);
        }

        public void ChangeState<T>() where T : BaseState
        {
            NodeState.Exit();
            NodeState = (BaseState)Activator.CreateInstance(typeof(T), this);
            NodeState.Entry();
            NodeStateChanged?.Invoke(NodeState);
        }

        public void RefreshChildren()
        {
            if (!NodeState.GetType().IsAssignableFrom(typeof(UnlockState))) return;

            foreach (var child in Children)
            {
                if (child.NodeState.GetType().IsAssignableFrom(typeof(LockState)))
                {
                    child.ChangeState<AllowState>();
                }
            }
        }
    }

    public class GameSkillTree
    {
        public static GameSkillTree Instance { get; private set; }

        public Dictionary<string, GameSkillTreeNode> Refs;
        public GameSkillTreeNode Root;

        public GameSkillTree(List<GameSkillData> nodes, GameSkillData rootSkill)
        {
            Refs = new Dictionary<string, GameSkillTreeNode>(nodes.Count);
            Initialize(nodes, rootSkill);

            Instance = this;
        }

        private void Initialize(List<GameSkillData> nodes, GameSkillData rootSkill)
        {
            foreach (var node in nodes)
            {
                var treeNode = new GameSkillTreeNode(node, new List<GameSkillTreeNode>());
                Refs[node.skillId] = treeNode;
            }

            foreach (var node in nodes)
            {
                var treeNode = Refs[node.skillId];
                foreach (var nextSkill in node.nextSkills)
                {
                    if (!Refs.TryGetValue(nextSkill.skillId, out var nextTreeNode))
                    {
                        Debug.LogError($"Next skill with id {nextSkill.skillId} not found in refs.");
                        continue;
                    }

                    treeNode.Children.Add(nextTreeNode);
                }
            }

            // Assuming the first node in the list is the root. This can be changed based on your requirements.
            if (nodes.Count > 0)
            {
                Root = Refs[rootSkill.skillId];
                Root.ChangeState<UnlockState>();
            }
        }

        public void BrowseBfs(Action<GameSkillTreeNode> nodeHandler)
        {
            Queue<GameSkillTreeNode> queue = new();

            queue.Enqueue(Root);
            while (queue.TryDequeue(out GameSkillTreeNode node))
            {
                nodeHandler?.Invoke(node);
                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        public void BrowseDfs(Action<GameSkillTreeNode> nodeHandler)
        {
            Stack<GameSkillTreeNode> stack = new();
            stack.Push(Root);

            while (stack.TryPop(out GameSkillTreeNode node))
            {
                nodeHandler?.Invoke(node);
                foreach (var child in node.Children)
                {
                    stack.Push(child);
                }
            }
        }
    }
}