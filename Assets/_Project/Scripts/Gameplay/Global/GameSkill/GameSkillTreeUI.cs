using System;
using System.Collections.Generic;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Global
{
    public class GameSkillTreeUI : VisualElement
    {
        public readonly VisualElement Viewport;
        private readonly VisualElement content;
        private readonly VisualElement nodeLayer;
        private readonly VisualElement edgeLayer;

        private GameSkillTree tree;
        private Dictionary<string, GameSkillTreeRadialLayout.NodeLayoutData> nodeLayouts;

        private Vector3 translation = Vector3.zero;
        private float displayScale = 1.0f;
        private const float MIN_SCALE = 0.4f;
        private const float MAX_SCALE = 2.0f;
        private const float ZOOM_STEP = 0.05f;

        private const float DEFAULT_NODE_SIZE = 60f;

        public event Action<GameSkillTreeNode> OnNodeClicked;

        public GameSkillTreeUI(StyleSheet styleSheet = null)
        {
            if (styleSheet != null)
                styleSheets.Add(styleSheet);

            this.AddClass("skill-tree-root");

            // Viewport acts as a masking frame (overflow: hidden)
            Viewport = this.CreateChild("skill-tree-viewport");

            // Content acts as the infinite canvas for Pan and Zoom
            content = Viewport.CreateChild("skill-tree-container");

            // Layers inside the canvas
            edgeLayer = content.CreateChild("skill-tree-edge-layer");
            nodeLayer = content.CreateChild("skill-tree-content");
            edgeLayer.generateVisualContent += OnGenerateVisualContent;

            nodeLayer.BringToFront();

            RegisterCallback<WheelEvent>(OnScrollWheel);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseDownEvent>(OnMouseDown);

            // Re-center if window/UI size changes
            RegisterCallback<GeometryChangedEvent>(_ => UpdateTransform());
        }

        public void Initialize(GameSkillTree treeSource,
            Dictionary<string, GameSkillTreeRadialLayout.NodeLayoutData> nodeLayoutsSource)
        {
            tree = treeSource;
            nodeLayouts = nodeLayoutsSource;
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            if (tree == null || nodeLayouts == null) return;

            nodeLayer.Clear();

            foreach (var kvp in nodeLayouts)
            {
                var skillId = kvp.Key;
                var layoutData = kvp.Value;
                var node = tree.Refs[skillId];

                VisualElement nodeElement = CreateNodeElement(node, layoutData.Position);
                nodeLayer.Add(nodeElement);
            }

            edgeLayer.MarkDirtyRepaint();
            UpdateTransform();
        }

        private VisualElement CreateNodeElement(GameSkillTreeNode node, Vector2 position)
        {
            var element = new Image
            {
                sprite = node.Data.skillIcon,
                tooltip = $"{node.Data.skillName}\n{node.Data.skillDescription}"
            };
            
            node.NodeState.HandleUI(element);
            
            // handle un-registry: if SkillTreeUI lifecycle dif with SkillTree
            node.NodeStateChanged += state => state.HandleUI(element);

            element.RegisterCallback<ClickEvent>(_ => OnNodeClicked?.Invoke(node));

            float size = DEFAULT_NODE_SIZE;

            // Highlight root node
            if (node.Data.skillId == tree.Root.Data.skillId)
            {
                size *= 1.5f;
                element.AddClass("root-skill-node");
            }

            element.style.width = size;
            element.style.height = size;

            // Offset to center the element at its coordinate
            element.style.left = position.x - (size / 2f);
            element.style.top = position.y - (size / 2f);

            return element;
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (tree == null || nodeLayouts == null) return;

            var painter = mgc.painter2D;
            painter.strokeColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            painter.lineWidth = 3f;

            tree.BrowseBfs(node =>
            {
                if (!nodeLayouts.TryGetValue(node.Data.skillId, out var nodeData)) return;

                foreach (var child in node.Children)
                {
                    if (!nodeLayouts.TryGetValue(child.Data.skillId, out var childData)) continue;

                    painter.BeginPath();
                    painter.MoveTo(nodeData.Position);
                    painter.LineTo(childData.Position);
                    painter.Stroke();
                }
            });
        }

        private void OnScrollWheel(WheelEvent evt)
        {
            float delta = -evt.delta.y;
            float oldScale = displayScale;

            displayScale = Mathf.Clamp(displayScale + delta * ZOOM_STEP, MIN_SCALE, MAX_SCALE);
            float scaleRatio = displayScale / oldScale;

            // Coordinate system alignment
            Vector2 viewportCenter = new Vector2(Viewport.layout.width / 2, Viewport.layout.height / 2);
            Vector2 mousePosInViewport = evt.localMousePosition - Viewport.layout.position;
            Vector2 mousePosRelToCenter = mousePosInViewport - viewportCenter;

            // Zoom to mouse logic: ensure the point under the cursor stays under the cursor
            translation.x = mousePosRelToCenter.x - (mousePosRelToCenter.x - translation.x) * scaleRatio;
            translation.y = mousePosRelToCenter.y - (mousePosRelToCenter.y - translation.y) * scaleRatio;

            UpdateTransform();
            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            // Pan with Alt + Left Mouse Drag
            if (!evt.altKey || (evt.pressedButtons & 1) == 0) return;

            translation += (Vector3)evt.mouseDelta;
            UpdateTransform();
            evt.StopPropagation();
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.altKey) evt.StopPropagation();
        }

        private void UpdateTransform()
        {
            if (Viewport.layout.width <= 0) return;

            // Offset the canvas so (0,0) starts at viewport center
            Vector3 centerOffset = new Vector3(Viewport.layout.width / 2, Viewport.layout.height / 2, 0);

            content.transform.position = translation + centerOffset;
            content.transform.scale = new Vector3(displayScale, displayScale, 1);
        }
    }
}