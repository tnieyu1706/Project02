#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Game.BaseGameplay;

namespace Game.Editor
{
    public class TowerUpgradeTreeGraphWindow : EditorWindow
    {
        private TowerUpgradeTree currentTree;
        private TowerUpgradeGraphView graphView;

        [MenuItem("Window/Game/Tower Upgrade Graph (UI Toolkit)")]
        public static void ShowWindow()
        {
            var window = GetWindow<TowerUpgradeTreeGraphWindow>("Tower Upgrade Tree");
            window.minSize = new Vector2(800, 500);
            window.Show();
        }

        private void CreateGUI()
        {
            // Thiết lập GraphView
            graphView = new TowerUpgradeGraphView(this)
            {
                name = "Tower Graph"
            };
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            // Thiết lập Toolbar
            GenerateToolbar();
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var treeField = new ObjectField("Upgrade Tree:")
            {
                objectType = typeof(TowerUpgradeTree),
                allowSceneObjects = false,
                value = currentTree,
                style = { minWidth = 250, paddingLeft = 5, paddingTop = 2 }
            };

            treeField.RegisterValueChangedCallback(evt =>
            {
                currentTree = evt.newValue as TowerUpgradeTree;
                graphView.PopulateView(currentTree);
            });
            toolbar.Add(treeField);

            toolbar.Add(new ToolbarButton(() => graphView.PopulateView(currentTree)) { text = "Reload" });
            toolbar.Add(new ToolbarButton(() => graphView.AutoLayout()) { text = "Auto Layout" });
            toolbar.Add(new ToolbarButton(() => SyncAllPresets()) { text = "Sync All Presets (Auto)" });

            rootVisualElement.Add(toolbar);
        }

        private void SyncAllPresets()
        {
            if (currentTree == null) return;

            var method = typeof(TowerUpgradeTree).GetMethod("LoadData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(currentTree, null);
                EditorUtility.SetDirty(currentTree);
                AssetDatabase.SaveAssets();
                graphView.PopulateView(currentTree);
            }
        }
    }

    public class TowerUpgradeGraphView : GraphView
    {
        private TowerUpgradeTree currentTree;
        private TowerUpgradeTreeGraphWindow window;
        private Dictionary<TowerPresetSo, TowerNode> nodeDictionary;

        public TowerUpgradeGraphView(TowerUpgradeTreeGraphWindow window)
        {
            this.window = window;
            nodeDictionary = new Dictionary<TowerPresetSo, TowerNode>();

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Thêm các thao tác di chuyển mặc định
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Thêm Grid Background
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            // Đăng ký sự kiện Drag & Drop ScriptableObject vào Graph
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);

            // Lắng nghe sự kiện xoá node / thêm/xoá dây nối (Edge)
            graphViewChanged = OnGraphViewChanged;
        }

        private List<TowerUpgradeTreeNode> GetTreeNodes()
        {
            if (currentTree == null) return null;
            var field = typeof(TowerUpgradeTree).GetField("nodes",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(currentTree) as List<TowerUpgradeTreeNode>;
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (currentTree == null) return;

            var objects = DragAndDrop.objectReferences;
            if (objects.Any(obj => obj is TowerPresetSo))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                evt.StopPropagation();
            }
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            if (currentTree == null) return;

            var objects = DragAndDrop.objectReferences;
            var towerPresets = objects.OfType<TowerPresetSo>().ToList();

            if (towerPresets.Count > 0)
            {
                DragAndDrop.AcceptDrag();

                bool isDirty = false;
                var treeNodes = GetTreeNodes();
                if (treeNodes == null) return;

                foreach (var preset in towerPresets)
                {
                    // Nếu Node đã tồn tại trong cây thì bỏ qua
                    if (treeNodes.Any(n => n.towerPreset == preset))
                        continue;

                    Undo.RecordObject(currentTree, "Add Tower Node via Drag");

                    // 1. Thêm dữ liệu vào cây
                    var newNodeData = new TowerUpgradeTreeNode
                    {
                        towerPreset = preset,
                        nextUpgradeTowers = new List<TowerPresetSo>()
                    };
                    treeNodes.Add(newNodeData);

                    // 2. Tạo Node hiển thị trên Graph tại vị trí con trỏ chuột
                    var towerNode = new TowerNode(preset);

                    // Chuyển đổi toạ độ chuột sang không gian cục bộ của ContentContainer
                    Vector2 localMousePos = contentViewContainer.WorldToLocal(evt.mousePosition);
                    towerNode.SetPosition(new Rect(localMousePos, Vector2.zero));

                    AddElement(towerNode);
                    nodeDictionary[preset] = towerNode;

                    isDirty = true;
                }

                if (isDirty)
                {
                    EditorUtility.SetDirty(currentTree);
                    AssetDatabase.SaveAssets();
                }

                evt.StopPropagation();
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (currentTree == null) return change;

            bool isDirty = false;
            var treeNodes = GetTreeNodes();

            // Khi xoá liên kết hoặc xoá Node
            if (change.elementsToRemove != null && treeNodes != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        var sourceNode = edge.output.node as TowerNode;
                        var targetNode = edge.input.node as TowerNode;

                        if (sourceNode != null && targetNode != null)
                        {
                            var treeNode = treeNodes.FirstOrDefault(n => n.towerPreset == sourceNode.Preset);
                            if (treeNode != null && treeNode.nextUpgradeTowers.Contains(targetNode.Preset))
                            {
                                Undo.RecordObject(currentTree, "Remove Upgrade Link");
                                treeNode.nextUpgradeTowers.Remove(targetNode.Preset);
                                isDirty = true;
                            }
                        }
                    }
                    else if (element is TowerNode towerNode)
                    {
                        var treeNode = treeNodes.FirstOrDefault(n => n.towerPreset == towerNode.Preset);
                        if (treeNode != null)
                        {
                            Undo.RecordObject(currentTree, "Remove Tower Node");

                            // 1. Xoá khỏi danh sách nodes
                            treeNodes.Remove(treeNode);

                            // 2. Dọn dẹp tất cả các reference từ những Node khác trỏ tới Node này
                            foreach (var n in treeNodes)
                            {
                                if (n.nextUpgradeTowers.Contains(towerNode.Preset))
                                {
                                    n.nextUpgradeTowers.Remove(towerNode.Preset);
                                }
                            }

                            nodeDictionary.Remove(towerNode.Preset);
                            isDirty = true;
                        }
                    }
                }
            }

            // Khi tạo liên kết mới
            if (change.edgesToCreate != null && treeNodes != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    var sourceNode = edge.output.node as TowerNode;
                    var targetNode = edge.input.node as TowerNode;

                    if (sourceNode != null && targetNode != null)
                    {
                        var treeNode = treeNodes.FirstOrDefault(n => n.towerPreset == sourceNode.Preset);
                        if (treeNode != null && !treeNode.nextUpgradeTowers.Contains(targetNode.Preset))
                        {
                            Undo.RecordObject(currentTree, "Add Upgrade Link");
                            treeNode.nextUpgradeTowers.Add(targetNode.Preset);
                            isDirty = true;
                        }
                    }
                }
            }

            if (isDirty)
            {
                EditorUtility.SetDirty(currentTree);
            }

            return change;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // Cho phép nối Output với Input, không cho phép tự nối vào chính mình
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        public void PopulateView(TowerUpgradeTree tree)
        {
            currentTree = tree;
            DeleteElements(graphElements);
            nodeDictionary.Clear();

            if (currentTree == null) return;

            var treeNodes = GetTreeNodes();
            if (treeNodes == null) return;

            // 1. Tạo tất cả các Node hiển thị
            foreach (var nodeData in treeNodes)
            {
                if (nodeData.towerPreset == null) continue;

                var towerNode = new TowerNode(nodeData.towerPreset);
                AddElement(towerNode);
                nodeDictionary[nodeData.towerPreset] = towerNode;
            }

            // 2. Tạo các liên kết (Edge)
            foreach (var nodeData in treeNodes)
            {
                if (nodeData.towerPreset == null ||
                    !nodeDictionary.TryGetValue(nodeData.towerPreset, out var sourceNode)) continue;

                foreach (var targetPreset in nodeData.nextUpgradeTowers)
                {
                    if (targetPreset != null && nodeDictionary.TryGetValue(targetPreset, out var targetNode))
                    {
                        var edge = sourceNode.OutputPort.ConnectTo(targetNode.InputPort);
                        AddElement(edge);
                    }
                }
            }

            AutoLayout();
        }

        public void AutoLayout()
        {
            if (nodeDictionary.Count == 0) return;

            float nodeWidth = 200f;
            float nodeHeight = 120f;
            float horizontalSpacing = 250f;
            float verticalSpacing = 160f;

            // Nhóm theo level
            var grouped = nodeDictionary.Values
                .GroupBy(n => n.Preset.towerLevel)
                .OrderBy(g => (int)g.Key)
                .ToList();

            float startX = 50f;

            foreach (var group in grouped)
            {
                var sortedGroup = group.OrderBy(n => n.Preset.towerPriceValue).ToList();

                // Tính toạ độ Y bắt đầu để căn giữa theo chiều dọc
                float startY = -((sortedGroup.Count * verticalSpacing) / 2f) + (verticalSpacing / 2f);

                foreach (var node in sortedGroup)
                {
                    node.SetPosition(new Rect(startX, startY + 200f, nodeWidth, nodeHeight));
                    startY += verticalSpacing;
                }

                startX += horizontalSpacing;
            }
        }
    }

    public class TowerNode : Node
    {
        public TowerPresetSo Preset { get; private set; }
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        public TowerNode(TowerPresetSo preset)
        {
            Preset = preset;
            title = preset.name;

            // Xử lý sự kiện click đúp để Ping asset
            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2)
                {
                    Selection.activeObject = preset;
                    EditorGUIUtility.PingObject(preset);
                }
            });

            CreatePorts();
            DrawCustomInfo();
        }

        private void CreatePorts()
        {
            // Cổng nhận (Input) bên trái
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi,
                typeof(TowerPresetSo));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            // Cổng xuất (Output) bên phải
            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi,
                typeof(TowerPresetSo));
            OutputPort.portName = "Upgrade";
            outputContainer.Add(OutputPort);
        }

        private void DrawCustomInfo()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.paddingTop = 5;
            container.style.paddingBottom = 5;
            container.style.paddingLeft = 5;
            container.style.paddingRight = 5;

            // Icon
            if (Preset.towerIcon != null)
            {
                var iconFrame = new VisualElement();
                iconFrame.style.width = 40;
                iconFrame.style.height = 40;
                iconFrame.style.marginRight = 10;

                // Trích xuất Image từ Sprite Multiple
                var img = new Image();
                img.sprite = Preset.towerIcon;
                img.style.width = 40;
                img.style.height = 40;
                img.scaleMode = ScaleMode.ScaleToFit;

                iconFrame.Add(img);
                container.Add(iconFrame);
            }

            // Text Data
            var infoContainer = new VisualElement();
            infoContainer.style.flexDirection = FlexDirection.Column;
            infoContainer.style.justifyContent = Justify.Center;

            var typeLabel = new Label($"Type: {Preset.towerType}");
            typeLabel.style.fontSize = 11;
            infoContainer.Add(typeLabel);

            var levelLabel = new Label($"Level: {Preset.towerLevel}");
            levelLabel.style.fontSize = 11;
            infoContainer.Add(levelLabel);

            var priceLabel = new Label($"Price: {Preset.towerPriceValue}");
            priceLabel.style.fontSize = 11;
            priceLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            priceLabel.style.color = new StyleColor(new Color(0.9f, 0.7f, 0.2f)); // Màu cam nhạt cho giá
            infoContainer.Add(priceLabel);

            container.Add(infoContainer);

            // Đẩy vào phần mở rộng (phần ruột) của Node
            extensionContainer.Add(container);
            RefreshExpandedState(); // Bắt buộc gọi sau khi thêm item vào extensionContainer
        }
    }
}
#endif