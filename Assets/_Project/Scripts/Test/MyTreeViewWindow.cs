using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MyTreeViewWindow : EditorWindow
{
    private TreeView treeView;

    [MenuItem("Tools/My TreeView")]
    public static void Open()
    {
        var window = GetWindow<MyTreeViewWindow>();
        window.titleContent = new GUIContent("My TreeView");
    }

    public void CreateGUI()
    {
        // Root
        var root = rootVisualElement;

        // Tạo TreeView
        treeView = new TreeView
        {
            fixedItemHeight = 22,
            selectionType = SelectionType.Single
        };

        treeView.makeItem = MakeItem;
        treeView.bindItem = BindItem;

        // Set dữ liệu
        treeView.SetRootItems(CreateTreeData());
        treeView.Rebuild();

        // Callback khi chọn item
        treeView.selectionChanged += OnSelectionChanged;

        root.Add(treeView);
    }

    // ----------------------------
    // Tạo dữ liệu dạng cây
    // ----------------------------
    private List<TreeViewItemData<string>> CreateTreeData()
    {
        return new List<TreeViewItemData<string>>
        {
            new TreeViewItemData<string>(1, "Root",
                new List<TreeViewItemData<string>>
                {
                    new TreeViewItemData<string>(2, "Child A"),
                    new TreeViewItemData<string>(3, "Child B",
                        new List<TreeViewItemData<string>>
                        {
                            new TreeViewItemData<string>(4, "Sub Child B1"),
                            new TreeViewItemData<string>(5, "Sub Child B2")
                        })
                })
        };
    }

    // ----------------------------
    // Tạo UI cho mỗi item
    // ----------------------------
    private VisualElement MakeItem()
    {
        return new Label();
    }

    // ----------------------------
    // Bind dữ liệu vào item
    // ----------------------------
    private void BindItem(VisualElement element, int index)
    {
        var label = element as Label;

        var itemData = treeView.GetItemDataForIndex<string>(index);
        label.text = itemData;
    }

    // ----------------------------
    // Khi chọn item
    // ----------------------------
    private void OnSelectionChanged(IEnumerable<object> selectedItems)
    {
        foreach (var item in selectedItems)
        {
            Debug.Log("Selected: " + item);
        }
    }
}