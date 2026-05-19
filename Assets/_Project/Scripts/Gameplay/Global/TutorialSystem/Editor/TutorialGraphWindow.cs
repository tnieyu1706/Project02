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
            
            // 1. Tìm node bắt đầu (không có kết nối input)
            var startNode = allNodes.FirstOrDefault(n => !n.InputPort.connections.Any());
            
            // Nếu không tìm thấy node không có input nhưng có node trong graph, lấy node đầu tiên được tạo
            if (startNode == null && allNodes.Count > 0) startNode = allNodes[0];

            List<TutorialStepData> orderedSteps = new List<TutorialStepData>();
            HashSet<TutorialStepNode> visited = new HashSet<TutorialStepNode>();
            
            TutorialStepNode currentNode = startNode;

            // 2. Duyệt đồ thị theo dạng chuỗi tuyến tính
            while (currentNode != null && !visited.Contains(currentNode))
            {
                visited.Add(currentNode);
                currentNode.StepData.NodePosition = currentNode.GetPosition().position;
                currentNode.StepData.name = currentNode.title;
                
                orderedSteps.Add(currentNode.StepData);
                EditorUtility.SetDirty(currentNode.StepData);

                // Lấy node tiếp theo qua kết nối Output
                if (currentNode.OutputPort.connections.Any())
                {
                    currentNode = currentNode.OutputPort.connections.First().input.node as TutorialStepNode;
                }
                else
                {
                    currentNode = null;
                }
            }

            // 3. Xử lý các node mồ côi (không nằm trong chuỗi chính)
            foreach (var node in allNodes)
            {
                if (!visited.Contains(node))
                {
                    node.StepData.NodePosition = node.GetPosition().position;
                    node.StepData.name = node.title;
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
                
                // Tự động kết nối nếu có bước trước đó (tái lập chuỗi tuyến tính)
                if (lastNode != null)
                {
                    var edge = lastNode.OutputPort.ConnectTo(currentNode.InputPort);
                    graphView.AddElement(edge);
                }

                lastNode = currentNode;
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