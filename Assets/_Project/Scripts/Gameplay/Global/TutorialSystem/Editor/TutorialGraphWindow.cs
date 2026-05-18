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
            graphView = new TutorialGraphView { name = "Tutorial Graph" };
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            // Bắt sự kiện xóa Node để dọn dẹp Sub-asset
            graphView.graphViewChanged = OnGraphViewChanged;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null && currentData != null)
            {
                foreach (var elem in change.elementsToRemove)
                {
                    if (elem is TutorialStepNode node && node.StepData != null)
                    {
                        currentData.Steps.Remove(node.StepData);
                        AssetDatabase.RemoveObjectFromAsset(node.StepData);
                        DestroyImmediate(node.StepData, true);
                    }
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
            var addNodeBtn = new Button(CreateNewNode) { text = "Add Node" };

            toolbar.Add(saveBtn);
            toolbar.Add(new ToolbarSpacer());
            toolbar.Add(addNodeBtn);

            rootVisualElement.Add(toolbar);
        }

        private void CreateNewNode()
        {
            if (currentData == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng chọn một TutorialData trước khi tạo Node!", "OK");
                return;
            }

            // Tự động tạo Sub-asset nhúng vào TutorialData
            TutorialStepData newStep = ScriptableObject.CreateInstance<TutorialStepData>();
            newStep.name = "Step_" + System.Guid.NewGuid().ToString().Substring(0, 5);

            AssetDatabase.AddObjectToAsset(newStep, currentData);
            currentData.Steps.Add(newStep);
            AssetDatabase.SaveAssets();

            graphView.CreateStepNode(newStep, Vector2.zero);
        }

        private void SaveData()
        {
            if (currentData == null) return;

            var allNodes = graphView.nodes.ToList().Cast<TutorialStepNode>().ToList();
            currentData.Steps.Clear();

            var startNode = allNodes.FirstOrDefault(n => !n.InputPort.connections.Any());
            currentData.StartStep =
                startNode != null ? startNode.StepData : (allNodes.Count > 0 ? allNodes[0].StepData : null);

            foreach (var node in allNodes)
            {
                node.StepData.NodePosition = node.GetPosition().position;
                node.StepData.NextStep = null;

                if (node.OutputPort.connections.Any())
                {
                    var targetNode = node.OutputPort.connections.First().input.node as TutorialStepNode;
                    if (targetNode != null)
                    {
                        node.StepData.NextStep = targetNode.StepData;
                    }
                }

                // Cập nhật tên của file SO theo title của node
                node.StepData.name = node.title;
                EditorUtility.SetDirty(node.StepData);
                currentData.Steps.Add(node.StepData);
            }

            EditorUtility.SetDirty(currentData);
            AssetDatabase.SaveAssets();
            Debug.Log("[TutorialGraph] Đã lưu liên kết Sub-asset thành công!");
        }

        private void LoadData()
        {
            if (currentData == null) return;

            graphView.DeleteElements(graphView.nodes.ToList());
            graphView.DeleteElements(graphView.edges.ToList());

            Dictionary<TutorialStepData, TutorialStepNode> nodeDict =
                new Dictionary<TutorialStepData, TutorialStepNode>();

            foreach (var step in currentData.Steps)
            {
                if (step != null)
                {
                    var node = graphView.CreateStepNode(step, step.NodePosition);
                    nodeDict[step] = node;
                }
            }

            foreach (var step in currentData.Steps)
            {
                if (step != null && step.NextStep != null && nodeDict.ContainsKey(step) &&
                    nodeDict.ContainsKey(step.NextStep))
                {
                    var sourceNode = nodeDict[step];
                    var targetNode = nodeDict[step.NextStep];
                    var edge = sourceNode.OutputPort.ConnectTo(targetNode.InputPort);
                    graphView.AddElement(edge);
                }
            }
        }
    }

    public class TutorialGraphView : GraphView
    {
        public TutorialGraphView()
        {
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
            AddElement(node);
            return node;
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

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
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