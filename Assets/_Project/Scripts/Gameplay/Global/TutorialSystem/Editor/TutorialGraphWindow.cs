#if UNITY_EDITOR
using System;
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
                bool nodesRemoved = false;
                foreach (var elem in change.elementsToRemove)
                {
                    if (elem is TutorialStepNode node && node.StepData != null)
                    {
                        currentData.Steps.Remove(node.StepData);
                        AssetDatabase.RemoveObjectFromAsset(node.StepData);
                        DestroyImmediate(node.StepData, true);
                        nodesRemoved = true;
                    }
                }

                if (nodesRemoved)
                {
                    UpdateNodeVisuals();
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
                EditorUtility.DisplayDialog("Error", "Please select TutorialData before creating a node!", "OK");
                return null;
            }

            // VIBRA NOTE: Use a persistent random ID for the asset name to prevent reference loss on re-ordering.
            string randomId = Guid.NewGuid().ToString().Substring(0, 4);
            TutorialStepData newStep = CreateInstance<TutorialStepData>();
            newStep.name = $"Step_{randomId}";

            AssetDatabase.AddObjectToAsset(newStep, currentData);
            currentData.Steps.Add(newStep);
            AssetDatabase.SaveAssets();

            var node = graphView.CreateStepNode(newStep, position);
            UpdateNodeVisuals();
            return node;
        }

        /// <summary>
        /// Updates node titles to show their current sequence index without renaming the underlying assets.
        /// </summary>
        private void UpdateNodeVisuals()
        {
            if (currentData == null) return;

            var allNodes = graphView.nodes.ToList().Cast<TutorialStepNode>().ToList();
            if (allNodes.Count == 0) return;

            // Traversal to find order
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

            // Update titles visually: [#Index] AssetName
            for (int i = 0; i < orderedNodes.Count; i++)
            {
                orderedNodes[i].title = $"[{i}] {orderedNodes[i].StepData.name}";
            }
        }

        private void SaveData()
        {
            if (currentData == null) return;

            UpdateNodeVisuals();

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
            Debug.Log(
                $"[TutorialGraph] Saved {orderedSteps.Count} steps in linear sequence. Asset IDs remained persistent.");
        }

        private void LoadData()
        {
            if (currentData == null) return;

            graphView.DeleteElements(graphView.nodes.ToList());
            graphView.DeleteElements(graphView.edges.ToList());

            Dictionary<TutorialStepData, TutorialStepNode> stepToNode =
                new Dictionary<TutorialStepData, TutorialStepNode>();

            // 1. Create all nodes
            foreach (var step in currentData.Steps)
            {
                if (step == null) continue;
                var node = graphView.CreateStepNode(step, step.NodePosition);
                stepToNode[step] = node;
            }

            // 2. Re-connect edges based on the saved sequence list
            for (int i = 0; i < currentData.Steps.Count - 1; i++)
            {
                var currentStep = currentData.Steps[i];
                var nextStep = currentData.Steps[i + 1];

                if (stepToNode.ContainsKey(currentStep) && stepToNode.ContainsKey(nextStep))
                {
                    var edge = stepToNode[currentStep].OutputPort.ConnectTo(stepToNode[nextStep].InputPort);
                    graphView.AddElement(edge);
                }
            }

            UpdateNodeVisuals();
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

            // VIBRA NOTE: Capacity.Single to ensure linear flow
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            InputPort.portName = "Input";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            OutputPort.portName = "Next Step";
            outputContainer.Add(OutputPort);

            // Node Data UI
            var idField = new TextField("Asset ID") { value = StepData.name };
            idField.RegisterValueChangedCallback(evt =>
            {
                // VIBRA NOTE: Manual rename of the asset ID still allowed, but cautioned. 
                // The visual sequence prefix will be restored by the next UpdateNodeVisuals call.
                StepData.name = evt.newValue;
                title = evt.newValue;
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