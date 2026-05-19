#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Global.TutorialSystem.Editor
{
    public class TutorialGraphWindow : EditorWindow
    {
        private TutorialGraphView graphView;
        private ObjectField dataSelectorField;
        private TutorialData currentData;

        [MenuItem("Window/Tutorial System/Tutorial Node Graph")]
        public static void OpenGraphWindow()
        {
            var window = GetWindow<TutorialGraphWindow>();
            window.titleContent = new GUIContent("Tutorial Graph");
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }

        private void ConstructGraphView()
        {
            graphView = new TutorialGraphView(this) { name = "Tutorial Graph" };
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            // Bắt sự kiện xóa Node để dọn dẹp Sub-asset
            graphView.graphViewChanged = OnGraphViewChanged;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null && currentData != null)
            {
                bool renamed = false;
                foreach (var elem in change.elementsToRemove)
                {
                    if (elem is TutorialStepNode node && node.StepData != null)
                    {
                        currentData.Steps.Remove(node.StepData);
                        AssetDatabase.RemoveObjectFromAsset(node.StepData);
                        DestroyImmediate(node.StepData, true);
                        renamed = true;
                    }
                }

                if (renamed)
                {
                    UpdateNodeNames();
                }
                
                AssetDatabase.SaveAssets();
            }

            return change;
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            dataSelectorField = new ObjectField("Target Data:")
            {
                objectType = typeof(TutorialData),
                allowSceneObjects = false
            };
            dataSelectorField.RegisterValueChangedCallback(evt =>
            {
                currentData = evt.newValue as TutorialData;
                LoadData();
            });
            toolbar.Add(dataSelectorField);

            var saveBtn = new Button(SaveData) { text = "Save Data" };
            var addNodeBtn = new Button(() => CreateNewNode(Vector2.zero)) { text = "Add Node" };

            toolbar.Add(saveBtn);
            toolbar.Add(new ToolbarSpacer());
            toolbar.Add(addNodeBtn);

            rootVisualElement.Add(toolbar);
        }

        public TutorialStepNode CreateNewNode(Vector2 position)
        {
            if (currentData == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng chọn một TutorialData trước khi tạo Node!", "OK");
                return null;
            }

            // Tự động tạo Sub-asset nhúng vào TutorialData
            TutorialStepData newStep = ScriptableObject.CreateInstance<TutorialStepData>();
            newStep.name = "New Step";

            AssetDatabase.AddObjectToAsset(newStep, currentData);
            currentData.Steps.Add(newStep);
            AssetDatabase.SaveAssets();

            var node = graphView.CreateStepNode(newStep, position);
            UpdateNodeNames();
            return node;
        }

        private void UpdateNodeNames()
        {
            if (currentData == null) return;

            var allNodes = graphView.nodes.ToList().Cast<TutorialStepNode>().ToList();
            if (allNodes.Count == 0) return;

            // VIBRA NOTE: Reuse traversal logic to determine sequential order for naming
            var startNode = allNodes.FirstOrDefault(n => !n.InputPort.connections.Any());
            if (startNode == null) startNode = allNodes[0];

            List<TutorialStepNode> orderedNodes = new List<TutorialStepNode>();
            HashSet<TutorialStepNode> visited = new HashSet<TutorialStepNode>();
            TutorialStepNode currentNode = startNode;

            while (currentNode != null && !visited.Contains(currentNode))
            {
                visited.Add(currentNode);
                orderedNodes.Add(currentNode);
                if (currentNode.OutputPort.connections.Any())
                {
                    currentNode = currentNode.OutputPort.connections.First().input.node as TutorialStepNode;
                }
                else currentNode = null;
            }

            // Add remaining nodes (orphans)
            foreach (var node in allNodes)
            {
                if (!visited.Contains(node)) orderedNodes.Add(node);
            }

            // Apply names: <DataName>_<Index>
            for (int i = 0; i < orderedNodes.Count; i++)
            {
                string newName = $"{currentData.name}_{i}";
                orderedNodes[i].title = newName;
                orderedNodes[i].StepData.name = newName;
                
                // Update the text field in the node UI if it exists
                var textField = orderedNodes[i].mainContainer.Q<TextField>();
                if (textField != null) textField.SetValueWithoutNotify(newName);
                
                EditorUtility.SetDirty(orderedNodes[i].StepData);
            }
        }

        private void SaveData()
        {
            if (currentData == null) return;

            UpdateNodeNames(); // Ensure names are correct before saving

            var allNodes = graphView.nodes.ToList().Cast<TutorialStepNode>().ToList();
            var startNode = allNodes.FirstOrDefault(n => !n.InputPort.connections.Any());
            if (startNode == null && allNodes.Count > 0) startNode = allNodes[0];

            List<TutorialStepData> orderedSteps = new List<TutorialStepData>();
            HashSet<TutorialStepNode> visited = new HashSet<TutorialStepNode>();
            TutorialStepNode currentNode = startNode;

            while (currentNode != null && !visited.Contains(currentNode))
            {
                visited.Add(currentNode);
                currentNode.StepData.NodePosition = currentNode.GetPosition().position;
                orderedSteps.Add(currentNode.StepData);
                EditorUtility.SetDirty(currentNode.StepData);

                if (currentNode.OutputPort.connections.Any())
                {
                    currentNode = currentNode.OutputPort.connections.First().input.node as TutorialStepNode;
                }
                else currentNode = null;
            }

            foreach (var node in allNodes)
            {
                if (!visited.Contains(node))
                {
                    node.StepData.NodePosition = node.GetPosition().position;
                    orderedSteps.Add(node.StepData);
                    EditorUtility.SetDirty(node.StepData);
                }
            }

            currentData.Steps = orderedSteps;
            EditorUtility.SetDirty(currentData);
            AssetDatabase.SaveAssets();
            Debug.Log($"[TutorialGraph] Đã lưu {orderedSteps.Count} bước theo thứ tự tuyến tính.");
        }

        private void LoadData()
        {
            if (currentData == null) return;

            graphView.DeleteElements(graphView.nodes.ToList());
            graphView.DeleteElements(graphView.edges.ToList());

            TutorialStepNode lastNode = null;

            foreach (var step in currentData.Steps)
            {
                if (step == null) continue;

                var currentNode = graphView.CreateStepNode(step, step.NodePosition);
                if (lastNode != null)
                {
                    var edge = lastNode.OutputPort.ConnectTo(currentNode.InputPort);
                    graphView.AddElement(edge);
                }
                lastNode = currentNode;
            }
            
            UpdateNodeNames();
        }
    }

    public class TutorialGraphView : GraphView
    {
        private TutorialGraphWindow _window;

        public TutorialGraphView(TutorialGraphWindow window)
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
            ports.ForEach(funcCall =>
            {
                if (startPort != funcCall && startPort.node != funcCall.node &&
                    startPort.direction != funcCall.direction)
                    compatiblePorts.Add(funcCall);
            });
            return compatiblePorts;
        }

        public TutorialStepNode CreateStepNode(TutorialStepData stepData, Vector2 position)
        {
            var node = new TutorialStepNode(stepData);
            node.SetPosition(new Rect(position, new Vector2(250, 200)));
            
            // Setup custom edge listener for drag-to-create
            var connectorListener = new TutorialEdgeConnectorListener(this, _window);
            node.OutputPort.AddManipulator(new EdgeConnector<Edge>(connectorListener));
            
            AddElement(node);
            return node;
        }
    }

    public class TutorialEdgeConnectorListener : IEdgeConnectorListener
    {
        private GraphView _graphView;
        private TutorialGraphWindow _window;

        public TutorialEdgeConnectorListener(GraphView gv, TutorialGraphWindow window)
        {
            _graphView = gv;
            _window = window;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            // VIBRA NOTE: This is triggered when an edge is dragged from a port and dropped in empty space.
            var outputNode = edge.output.node as TutorialStepNode;
            if (outputNode == null) return;

            // Calculate local position in GraphView
            Vector2 graphPosition = _graphView.contentViewContainer.WorldToLocal(position);
            
            // 1. Create new node
            var newNode = _window.CreateNewNode(graphPosition);
            if (newNode == null) return;

            // 2. Automatically connect the ports
            var newEdge = edge.output.ConnectTo(newNode.InputPort);
            _graphView.AddElement(newEdge);
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            // Standard drop behavior (already handled by GraphView natively)
        }
    }

    public class TutorialStepNode : Node
    {
        public TutorialStepData StepData;
        public Port InputPort;
        public Port OutputPort;

        public TutorialStepNode(TutorialStepData data)
        {
            StepData = data;
            title = data.name;

            // VIBRA NOTE: Capacity.Single để đảm bảo luồng tutorial là tuyến tính (Sequential)
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            InputPort.portName = "Input";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            OutputPort.portName = "Next Step";
            outputContainer.Add(OutputPort);

            // Giao diện dữ liệu của Node
            var idField = new TextField("Step Name") { value = StepData.name };
            idField.RegisterValueChangedCallback(evt =>
            {
                title = evt.newValue;
                StepData.name = evt.newValue;
                EditorUtility.SetDirty(StepData);
            });
            mainContainer.Add(idField);

            var msgField = new TextField("Message") { value = StepData.Message, multiline = true };
            msgField.style.minHeight = 40;
            msgField.RegisterValueChangedCallback(evt =>
            {
                StepData.Message = evt.newValue;
                EditorUtility.SetDirty(StepData);
            });
            mainContainer.Add(msgField);

            var displayField = new EnumField("Display Type", StepData.DisplayType);
            displayField.RegisterValueChangedCallback(evt =>
            {
                StepData.DisplayType = (TutorialDisplayType)evt.newValue;
                EditorUtility.SetDirty(StepData);
            });
            mainContainer.Add(displayField);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
#endif