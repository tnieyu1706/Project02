#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Game.BaseGameplay;
using _Project.Scripts.Gameplay.Global.UI.WorldMap;

namespace Game.Editor.WorldMap
{
    public class LevelMapGraphWindow : EditorWindow
    {
        private LevelMapGraphView _graphView;
        private LevelMapTreeSo _currentTree;

        [MenuItem("Window/Game/Level Map Graph")]
        public static void OpenGraphWindow()
        {
            var window = GetWindow<LevelMapGraphWindow>();
            window.titleContent = new GUIContent("Level Map Graph");
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }

        private void ConstructGraphView()
        {
            _graphView = new LevelMapGraphView(this)
            {
                name = "Level Map Graph"
            };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var treeField = new ObjectField("Level Tree SO")
            {
                objectType = typeof(LevelMapTreeSo),
                allowSceneObjects = false
            };
            treeField.RegisterValueChangedCallback(evt =>
            {
                _currentTree = evt.newValue as LevelMapTreeSo;
                _graphView.PopulateView(_currentTree);
            });

            var saveBtn = new Button(() => SaveGraph()) { text = "Save Graph" };

            toolbar.Add(treeField);
            toolbar.Add(saveBtn);
            rootVisualElement.Add(toolbar);
        }

        private void SaveGraph()
        {
            if (_currentTree == null) return;
            EditorUtility.SetDirty(_currentTree);
            AssetDatabase.SaveAssets();
            Debug.Log("Level Map Tree Saved!");
        }
    }

    public class LevelMapGraphView : GraphView
    {
        private LevelMapTreeSo _tree;
        private LevelMapGraphWindow _window;

        public LevelMapGraphView(LevelMapGraphWindow window)
        {
            _window = window;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        public void PopulateView(LevelMapTreeSo tree)
        {
            _tree = tree;
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            if (_tree == null) return;

            // Create Nodes
            foreach (var node in _tree.nodes)
            {
                CreateNodeView(node);
            }

            // Create Edges
            foreach (var node in _tree.nodes)
            {
                var parentView = GetNodeByGuid(node.guid) as LevelNodeView;
                foreach (var child in node.children)
                {
                    var childView = GetNodeByGuid(child.guid) as LevelNodeView;
                    if (parentView != null && childView != null)
                    {
                        var edge = parentView.outputPort.ConnectTo(childView.inputPort);
                        AddElement(edge);
                    }
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var elem in graphViewChange.elementsToRemove)
                {
                    if (elem is LevelNodeView nodeView)
                    {
                        _tree.nodes.Remove(nodeView.nodeData);
                        AssetDatabase.RemoveObjectFromAsset(nodeView.nodeData);
                    }

                    if (elem is Edge edge)
                    {
                        var parentView = edge.output.node as LevelNodeView;
                        var childView = edge.input.node as LevelNodeView;
                        parentView.nodeData.children.Remove(childView.nodeData);
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    var parentView = edge.output.node as LevelNodeView;
                    var childView = edge.input.node as LevelNodeView;
                    parentView.nodeData.children.Add(childView.nodeData);
                }
            }

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_tree == null) return;
            var screenMousePos = evt.localMousePosition;
            var worldMousePos = contentViewContainer.WorldToLocal(screenMousePos);

            evt.menu.AppendAction("Create Level Node", action => CreateNode(worldMousePos));
        }

        private void CreateNode(Vector2 position)
        {
            var nodeData = ScriptableObject.CreateInstance<LevelMapNode>();

            // XỬ LÝ TRÁNH TRÙNG LẶP SUB-ASSET NAME: 
            // Cấp phát một tên mặc định nhưng có số thứ tự để tránh trùng hoàn toàn
            string initialName = "New Node " + (_tree.nodes.Count + 1);

            nodeData.name = initialName; // name là tên File của Sub-asset ẩn bên trong Unity
            nodeData.nodeName = initialName; // nodeName là tên hiển thị trên UI của Editor
            nodeData.guid = Guid.NewGuid().ToString();
            nodeData.position = position;

            _tree.nodes.Add(nodeData);
            AssetDatabase.AddObjectToAsset(nodeData, _tree);

            CreateNodeView(nodeData);
        }

        private void CreateNodeView(LevelMapNode nodeData)
        {
            var nodeView = new LevelNodeView(nodeData);
            nodeView.SetPosition(new Rect(nodeData.position, new Vector2(200, 150)));
            AddElement(nodeView);
        }
    }

    public class LevelNodeView : Node
    {
        public LevelMapNode nodeData;
        public Port inputPort;
        public Port outputPort;

        public LevelNodeView(LevelMapNode node)
        {
            nodeData = node;
            viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPort();
            CreateOutputPort();
            CreateFields();
            UpdateTitle();
        }

        private void CreateInputPort()
        {
            inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "Input";
            inputContainer.Add(inputPort);
        }

        private void CreateOutputPort()
        {
            outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Next Levels";
            outputContainer.Add(outputPort);
        }

        private void CreateFields()
        {
            // Trường đổi tên Custom
            var nameField = new TextField("Node Name")
            {
                value = nodeData.nodeName
            };
            nameField.RegisterValueChangedCallback(evt =>
            {
                nodeData.nodeName = evt.newValue;

                // QUAN TRỌNG: Đổi luôn tên Sub-asset (thuộc tính name của SO) để Unity cập nhật tên file nội bộ
                nodeData.name = evt.newValue;

                UpdateTitle();
                EditorUtility.SetDirty(nodeData); // Báo cho Unity biết Asset đã bị chỉnh sửa
            });
            mainContainer.Add(nameField);

            // Trường chọn SO Level
            var levelField = new ObjectField("Target Level")
            {
                objectType = typeof(BuildingGameplayLevel),
                value = nodeData.level,
                allowSceneObjects = false
            };
            levelField.RegisterValueChangedCallback(evt =>
            {
                nodeData.level = evt.newValue as BuildingGameplayLevel;
                UpdateTitle();
                EditorUtility.SetDirty(nodeData);
            });
            mainContainer.Add(levelField);
        }

        private void UpdateTitle()
        {
            string levelName = nodeData.level != null ? nodeData.level.name : "None";
            title = $"{nodeData.nodeName} ({levelName})";
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            nodeData.position = new Vector2(newPos.xMin, newPos.yMin);
        }
    }
}
#endif