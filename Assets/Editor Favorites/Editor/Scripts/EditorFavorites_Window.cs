#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorFavorites
{
    public sealed class EditorFavorites_Window : EditorWindow
    {
        #region Properties
        [SerializeField] private EditorFavorites_Data data;
        [SerializeField] private Vector2 scroll;
        [SerializeField] private bool renaming;
        [SerializeField] private string renameBuffer;
        [SerializeField] private bool openScenesAdditively;
        [SerializeField] private Vector2 tabsScroll;

        private const string DragSourceKey = "Favorites/DragSource";
        private const float kListEpsilon = 0.0001f;
        private const int kGridMaxTile = 92;
        private const float kThumbToTile = 0.87f;
        private const int kGridMinTile = 32;
        private const int kMinThumbPx = 16;
        private const int kPadAtMin = 4;
        private const int kPadAtMax = 8;
        private const int kListRowH = 22;
        private const int kListIconPx = 16;
        private const float kPad = 4f;
        private const float kHScrollPad = 2f;

        private readonly List<Rect> tabRects = new();
        private int tabDragFrom = -1;
        private int tabInsertBefore = -1;
        private Rect tabsVisibleRect;
        private float tabsTotalHeight;
        private Vector2 tabDragStart;

        private readonly HashSet<int> multiSel = new();
        private int lastSelAnchor = -1;

        private static GUIStyle s_GridTileLabel;
        private static GUIStyle GridTileLabel
        {
            get
            {
                if (s_GridTileLabel == null)
                {
                    s_GridTileLabel = new GUIStyle(EditorStyles.label);
                    s_GridTileLabel.fontSize = 10;
                    s_GridTileLabel.alignment = TextAnchor.LowerCenter;
                    s_GridTileLabel.clipping = TextClipping.Clip;
                    s_GridTileLabel.wordWrap = true;
                    s_GridTileLabel.normal.textColor = Color.white;
                    s_GridTileLabel.hover.textColor = Color.white;
                    s_GridTileLabel.active.textColor = Color.white;
                    s_GridTileLabel.focused.textColor = Color.white;
                }
                return s_GridTileLabel;
            }
        }

        private bool sizeSliderDragging;

        private Color SelectionTint => EditorGUIUtility.isProSkin ? new Color(0.24f, 0.48f, 0.90f, 0.28f) : new Color(0.24f, 0.48f, 0.90f, 0.45f);
        #endregion

        #region Helpers
        private float Scale01
        {
            get
            {
                float v = data != null ? data.ItemScale01 : 1f;
                if (float.IsNaN(v)) v = 1f;
                return Mathf.Clamp01(v);
            }
        }

        private bool IsListMode() => Scale01 <= kListEpsilon;
        
        private float TabsTopHeight()
        {
            if (tabsTotalHeight > 0f) return tabsTotalHeight;
            float h = EditorStyles.toolbar.fixedHeight > 0 ? EditorStyles.toolbar.fixedHeight : 20f;
            return h;
        }

        private int CurrentTilePx()
        {
            float t = Mathf.Lerp(kGridMinTile, kGridMaxTile, Scale01);
            return Mathf.RoundToInt(t);
        }

        private int CurrentPadPx()
        {
            float t = Mathf.Lerp(kPadAtMin, kPadAtMax, Scale01);
            return Mathf.RoundToInt(t);
        }
        #endregion

        #region Menu
        [MenuItem("Tools/Editor Favorites")]
        public static void Open()
        {
            EditorFavorites_Window window = GetWindow<EditorFavorites_Window>("Editor Favorites");
            window.minSize = new Vector2(280f, 180f);
            window.Focus();
        }
        #endregion

        #region Unity Events
        private void OnEnable()
        {
            if (data == null) data = EditorFavorites_Data.LoadOrCreate();
            wantsMouseMove = true;
            LivePreviewPump.EnableOneShot(this);
        }

        private void OnDisable()
        {
            if (data != null) data.Save();
        }

        private void OnFocus()
        {
            if (data != null) data.RefreshSelected();
            IconCache.Clear();
            LivePreviewPump.EnableOneShot(this);
            Repaint();
        }

        private void OnGUI()
        {
            DrawTabs();
            GUILayout.Space(4f);
            if (IsListMode()) DrawList();
            else DrawGrid();
            DrawSizeBar();
            HandleDnD();
            if (Event.current.rawType == EventType.MouseUp && sizeSliderDragging)
            {
                sizeSliderDragging = false;
                if (data != null) data.Save();
            }
            HandleKeys();
            DeselectOnBackground();
        }
        #endregion

        #region Draw: Tabs
        private void DrawTabs()
        {
            float h = EditorStyles.toolbar.fixedHeight > 0 ? EditorStyles.toolbar.fixedHeight : 20f;
            float hbar = GUI.skin.horizontalScrollbar.fixedHeight > 0 ? GUI.skin.horizontalScrollbar.fixedHeight : 15f;

            IReadOnlyList<FavoritesTab> tabs = data.Tabs;
            GUIStyle style = EditorStyles.toolbarButton;

            tabRects.Clear();
            List<GUIContent> contents = new(tabs.Count);
            float totalW = 0f;
            for (int i = 0; i < tabs.Count; i++)
            {
                GUIContent gc = new GUIContent(tabs[i].Name);
                float w = Mathf.Ceil(style.CalcSize(gc).x) + 16f;
                contents.Add(gc);
                tabRects.Add(new Rect(totalW, 0f, w, h - 2f));
                totalW += w;
            }

            float rightW = kPad + 24f;
            if (renaming) rightW += 6f + 180f + 32f + 56f;

            Rect reserve = GUILayoutUtility.GetRect(1, h, GUILayout.ExpandWidth(true));
            float viewW = Mathf.Max(0, reserve.width - rightW);
            bool overflow = totalW > viewW + 0.5f;
            float extraH = overflow ? (hbar + kHScrollPad) : 0f;
            float neededH = h + extraH;

            Rect fullRect = new(reserve.x, reserve.y, reserve.width, neededH);
            tabsTotalHeight = neededH;

            Rect barBg = new(fullRect.x, fullRect.y, fullRect.width, h);
            GUI.Box(barBg, GUIContent.none, EditorStyles.toolbar);

            tabsVisibleRect = new(fullRect.x, fullRect.y, viewW, h);

            float contentW = Mathf.Max(viewW, totalW);
            if (!overflow) tabsScroll.x = 0f;
            else tabsScroll.x = Mathf.Clamp(tabsScroll.x, 0f, Mathf.Max(0f, contentW - viewW));

            Rect contentRect = new(0, 0, contentW, h - 2f);

            tabsScroll = GUI.BeginScrollView(
                tabsVisibleRect,
                tabsScroll,
                contentRect,
                false,
                false,
                GUIStyle.none,
                GUIStyle.none
            );

            Event evt = Event.current;

            for (int i = 0; i < tabs.Count; i++)
            {
                Rect r = tabRects[i];
                bool isOn = (i == data.SelectedTabIndex);

                if (evt.type == EventType.Repaint)
                {
                    style.Draw(r, contents[i], r.Contains(evt.mousePosition), isOn, isOn, false);
                }

                if (evt.type == EventType.MouseDown && r.Contains(evt.mousePosition))
                {
                    if (evt.button == 1)
                    {
                        int idx = i;
                        if (data.SelectedTabIndex != idx)
                        {
                            data.SelectedTabIndex = idx;
                            data.RefreshSelected();
                            data.Save();
                            EnsureTabVisible(idx, contentW);
                            Repaint();
                        }

                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Rename"), false, () =>
                        {
                            renaming = true;
                            renameBuffer = tabs[idx].Name;
                            data.SelectedTabIndex = idx;
                            EnsureTabVisible(idx, contentW);
                            Repaint();
                        });
                        menu.AddItem(new GUIContent("Delete Favorites…"), false, () =>
                        {
                            data.SelectedTabIndex = idx;
                            if (data.Tabs.Count <= 1)
                            {
                                EditorUtility.DisplayDialog("Delete Favorites", "You must have at least one favorites.", "OK");
                                return;
                            }
                            if (EditorUtility.DisplayDialog("Delete Favorites", $"Are you sure you want to delete the favorites \"{tabs[idx].Name}\"?", "Delete", "Cancel"))
                            {
                                data.RemoveSelectedTab();
                                renaming = false;
                                Repaint();
                            }
                        });
                        menu.ShowAsContext();
                        evt.Use();
                    }
                    else if (evt.button == 0)
                    {
                        if (!isOn)
                        {
                            data.SelectedTabIndex = i;
                            data.RefreshSelected();
                            data.Save();
                            EnsureTabVisible(i, contentW);
                            Repaint();
                        }

                        tabDragFrom = i;
                        tabDragStart = new Vector2(
                            evt.mousePosition.x - tabsScroll.x + tabsVisibleRect.x,
                            evt.mousePosition.y + tabsVisibleRect.y
                        );
                        tabInsertBefore = -1;
                        evt.Use();
                    }
                }
            }

            GUI.EndScrollView();

            float rx = fullRect.xMax - rightW + kPad;
            if (GUI.Button(new(rx, fullRect.y, 24f, h), "+", style))
            {
                data.AddTab("Favorites " + (data.Tabs.Count + 1));
                renaming = false;
                tabDragFrom = -1;
                tabInsertBefore = -1;
                Repaint();
            }
            rx += 24f + kPad;

            if (renaming)
            {
                Rect tf = new(rx, fullRect.y + 2f, 180f, h - 4f);
                GUI.SetNextControlName("RenameField");
                renameBuffer = GUI.TextField(tf, renameBuffer);
                if (GUI.GetNameOfFocusedControl() != "RenameField") GUI.FocusControl("RenameField");
                rx += 180f + 6f;

                if (GUI.Button(new(rx, fullRect.y, 32f, h), "OK", style))
                {
                    data.RenameSelected(renameBuffer);
                    renaming = false;
                    GUI.FocusControl(null);
                    Repaint();
                }
                rx += 32f + 6f;

                if (GUI.Button(new(rx, fullRect.y, 56f, h), "Cancel", style))
                {
                    renaming = false;
                    GUI.FocusControl(null);
                    Repaint();
                }
            }

            if (overflow)
            {
                float prev = tabsScroll.x;
                Rect sbRect = new(tabsVisibleRect.x, fullRect.y + h + kHScrollPad, viewW, hbar);
                float max = Mathf.Max(0f, contentW);
                tabsScroll.x = GUI.HorizontalScrollbar(sbRect, tabsScroll.x, viewW, 0f, max);
                tabsScroll.x = Mathf.Clamp(tabsScroll.x, 0f, Mathf.Max(0f, contentW - viewW));
                if (!Mathf.Approximately(prev, tabsScroll.x)) Repaint();
            }

            Event e = Event.current;
            if (e.type == EventType.MouseDrag && tabDragFrom != -1)
            {
                Vector2 guiDelta = e.mousePosition - tabDragStart;
                if (guiDelta.sqrMagnitude > 9f)
                {
                    const float edge = 12f;
                    if (e.mousePosition.x < tabsVisibleRect.x + edge)
                        tabsScroll.x = Mathf.Max(0f, tabsScroll.x - 12f);
                    else if (e.mousePosition.x > tabsVisibleRect.x + tabsVisibleRect.width - edge)
                        tabsScroll.x = Mathf.Min(Mathf.Max(0f, contentW - tabsVisibleRect.width), tabsScroll.x + 12f);

                    tabInsertBefore = GetTabInsertIndex(e.mousePosition.x);
                    Repaint();
                    e.Use();
                }
            }
            if (e.type == EventType.MouseUp && tabDragFrom != -1)
            {
                if (tabInsertBefore != -1 &&
                    tabInsertBefore != tabDragFrom &&
                    tabInsertBefore != tabDragFrom + 1)
                {
                    data.MoveTabInsert(tabDragFrom, tabInsertBefore);
                }
                tabDragFrom = -1;
                tabInsertBefore = -1;
                e.Use();
            }
            if (Event.current.type == EventType.Repaint && tabDragFrom != -1 && tabInsertBefore != -1)
                DrawTabInsertMarker(tabInsertBefore, h - 2f);
        }
        #endregion

        #region Draw: Grid/List
        private void DrawGrid()
        {
            FavoritesTab tab = data.GetSelected();
            IReadOnlyList<FavoritesItem> items = tab.Items;

            int tile = CurrentTilePx();
            int pad = CurrentPadPx();
            int thumb = Mathf.Max(kMinThumbPx, Mathf.RoundToInt(tile * kThumbToTile));

            float viewHeight = position.height - TabsTopHeight() - EditorStyles.toolbar.fixedHeight - 4f;

            if (items.Count == 0)
            {
                Rect r = GUILayoutUtility.GetRect(position.width, viewHeight);
                if (Event.current.type == EventType.Repaint)
                    GUI.Label(r, "Drag assets here to add to this favorites.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            int cols = Mathf.Max(1, Mathf.FloorToInt((position.width - 24f) / (tile + pad)));
            int rows = Mathf.CeilToInt(items.Count / Mathf.Max(1f, cols));
            float contentHeight = pad + rows * (tile + pad);

            using var sv = new GUILayout.ScrollViewScope(scroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
            scroll = sv.scrollPosition;

            bool isRepaint = Event.current.type == EventType.Repaint;
            float visibleMinY = scroll.y;
            float visibleMaxY = scroll.y + Mathf.Max(0f, viewHeight);

            int full = tile + pad;
            float y = pad;
            int idx = 0;
            for (int r = 0; r < rows; r++)
            {
                float rowMin = y - pad;
                float rowMax = y + tile + pad;
                bool rowVisible = rowMax >= visibleMinY && rowMin <= visibleMaxY;

                float x = pad;
                for (int c = 0; c < cols; c++)
                {
                    if (idx >= items.Count) break;

                    Rect rect = new Rect(x, y, tile, tile);
                    if (rowVisible)
                        DrawTileGrid(rect, idx, items[idx], thumb, isRepaint);

                    x += full;
                    idx++;
                }

                y += full;
            }

            GUILayoutUtility.GetRect(0, contentHeight, GUILayout.ExpandWidth(true));
            Rect contentRect = GUILayoutUtility.GetLastRect();
            HandleBackgroundClickInContent(contentRect);
        }

        private void DrawTileGrid(Rect rect, int index, FavoritesItem item, int thumbPx, bool isRepaint)
        {
            if (isRepaint) GUI.Box(rect, GUIContent.none);

            Rect thumbRect = new(rect.x + (rect.width - thumbPx) * 0.5f, rect.y + Mathf.Max(4f, 6f), thumbPx, thumbPx);

            Texture icon = IconCache.GetLargeOrFallback(item.Asset);
            if (icon) GUI.DrawTexture(thumbRect, icon, ScaleMode.ScaleToFit, true);

            bool selected = multiSel.Contains(index);
            if (selected) EditorGUI.DrawRect(rect, SelectionTint);

            Rect labelRect = rect;
            labelRect.yMin = rect.yMax - 18f;
            GUI.Label(labelRect, item.Asset ? item.Asset.name : "(Missing)", GridTileLabel);

            HandleTileEvents(rect, index, item);
        }

        private void DrawList()
        {
            FavoritesTab tab = data.GetSelected();
            IReadOnlyList<FavoritesItem> items = tab.Items;

            float viewHeight = position.height - TabsTopHeight() - EditorStyles.toolbar.fixedHeight - 4f;

            if (items.Count == 0)
            {
                Rect r = GUILayoutUtility.GetRect(position.width, viewHeight);
                if (Event.current.type == EventType.Repaint)
                    GUI.Label(r, "Drag assets here to add to this favorites.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            float contentHeight = items.Count * kListRowH;

            using GUILayout.ScrollViewScope sv = new(scroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
            scroll = sv.scrollPosition;

            bool isRepaint = Event.current.type == EventType.Repaint;
            float visibleMinY = scroll.y;
            float visibleMaxY = scroll.y + Mathf.Max(0f, viewHeight);

            float y = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                Rect row = new(0, y, position.width - 16f, kListRowH);

                if (row.yMax >= visibleMinY && row.y <= visibleMaxY)
                    DrawRow(row, i, items[i], isRepaint);

                y += kListRowH;
            }

            GUILayoutUtility.GetRect(0, contentHeight, GUILayout.ExpandWidth(true));
            Rect contentRect = GUILayoutUtility.GetLastRect();
            HandleBackgroundClickInContent(contentRect);
        }

        private void DrawRow(Rect row, int index, FavoritesItem item, bool isRepaint)
        {
            if (isRepaint)
            {
                if ((index & 1) == 0) EditorGUI.DrawRect(row, EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.02f) : new Color(0, 0, 0, 0.04f));
                if (multiSel.Contains(index)) EditorGUI.DrawRect(row, SelectionTint);
            }

            Rect iconRect = new(row.x + 6f, row.y + (row.height - kListIconPx) * 0.5f, kListIconPx, kListIconPx);
            Texture icon = IconCache.GetSmall(item.Asset);
            if (icon) GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);

            Rect labelRect = new(iconRect.xMax + 6f, row.y, row.width - iconRect.xMax - 12f, row.height);
            GUI.Label(labelRect, item.Asset ? item.Asset.name : "(Missing)");

            HandleRowEvents(row, index, item);
        }
        #endregion

        #region Input: Tiles
        private void HandleTileEvents(Rect rect, int index, FavoritesItem item)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                if (e.button == 0)
                {
                    if (e.clickCount == 2)
                    {
                        SetSingle(index);
                        OpenByType(item.Asset, e.mousePosition);
                        e.Use();
                        return;
                    }

                    bool shift = e.shift;
                    bool action = EditorGUI.actionKey;

                    if (!shift && !action)
                    {
                        if (!multiSel.Contains(index))
                        {
                            SetSingle(index);
                            lastSelAnchor = index;
                        }
                        else
                        {
                            ClearSelection();
                            AddToSelection(index);
                            lastSelAnchor = index;
                        }
                        e.Use();
                        return;
                    }

                    if (shift)
                    {
                        if (lastSelAnchor < 0) lastSelAnchor = index;
                        SelectRange(lastSelAnchor, index);
                        e.Use();
                        return;
                    }

                    if (action)
                    {
                        if (multiSel.Contains(index)) RemoveFromSelection(index);
                        else AddToSelection(index);
                        lastSelAnchor = index;
                        e.Use();
                        return;
                    }
                }
                else if (e.button == 1)
                {
                    if (!multiSel.Contains(index)) SetSingle(index);

                    GenericMenu menu = new();
                    menu.AddItem(new GUIContent("Open"), false, () =>
                    {
                        if (!OpenScene(item.Asset)) OpenScript(item.Asset);
                    });
                    menu.AddItem(new GUIContent("Ping"), false, () =>
                    {
                        if (item.Asset) EditorGUIUtility.PingObject(item.Asset);
                    });
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Remove"), false, () =>
                    {
                        FavoritesTab tab = data.GetSelected();
                        List<int> toRemove = new(multiSel);
                        toRemove.Sort();
                        for (int i = toRemove.Count - 1; i >= 0; i--) tab.RemoveAt(toRemove[i]);
                        data.RefreshSelected();
                        data.Save();
                        ClearSelection();
                    });
                    menu.ShowAsContext();
                    e.Use();
                }
            }

            if (e.type == EventType.MouseDrag && rect.Contains(e.mousePosition) && multiSel.Contains(index))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(DragSourceKey, this);
                DragAndDrop.objectReferences = CollectSelectedObjects();
                DragAndDrop.StartDrag("Move Items");
                e.Use();
            }
        }

        private void HandleRowEvents(Rect row, int index, FavoritesItem item)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && row.Contains(e.mousePosition))
            {
                if (e.button == 0)
                {
                    if (e.clickCount == 2)
                    {
                        SetSingle(index);
                        OpenByType(item.Asset, e.mousePosition);
                        e.Use();
                        return;
                    }

                    bool shift = e.shift;
                    bool action = EditorGUI.actionKey;

                    if (!shift && !action)
                    {
                        if (!multiSel.Contains(index))
                        {
                            SetSingle(index);
                            lastSelAnchor = index;
                        }
                        else
                        {
                            ClearSelection();
                            AddToSelection(index);
                            lastSelAnchor = index;
                        }
                        e.Use();
                        return;
                    }

                    if (shift)
                    {
                        if (lastSelAnchor < 0) lastSelAnchor = index;
                        SelectRange(lastSelAnchor, index);
                        e.Use();
                        return;
                    }

                    if (action)
                    {
                        if (multiSel.Contains(index)) RemoveFromSelection(index);
                        else AddToSelection(index);
                        lastSelAnchor = index;
                        e.Use();
                        return;
                    }
                }
                else if (e.button == 1)
                {
                    if (!multiSel.Contains(index)) SetSingle(index);

                    GenericMenu menu = new();
                    menu.AddItem(new GUIContent("Open"), false, () =>
                    {
                        if (!OpenScene(item.Asset)) OpenScript(item.Asset);
                    });
                    menu.AddItem(new GUIContent("Ping"), false, () =>
                    {
                        if (item.Asset) EditorGUIUtility.PingObject(item.Asset);
                    });
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Remove"), false, () =>
                    {
                        FavoritesTab tab = data.GetSelected();
                        List<int> toRemove = new List<int>(multiSel);
                        toRemove.Sort();
                        for (int i = toRemove.Count - 1; i >= 0; i--) tab.RemoveAt(toRemove[i]);
                        data.RefreshSelected();
                        data.Save();
                        ClearSelection();
                    });
                    menu.ShowAsContext();
                    e.Use();
                }
            }

            if (e.type == EventType.MouseDrag && row.Contains(e.mousePosition) && multiSel.Contains(index))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(DragSourceKey, this);
                DragAndDrop.objectReferences = CollectSelectedObjects();
                DragAndDrop.StartDrag("Move Items");
                e.Use();
            }
        }
        #endregion

        #region Draw: Size Bar
        private void DrawSizeBar()
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(string.Empty, EditorStyles.miniLabel, GUILayout.Width(30f));

                float val = Scale01;
                EditorGUI.BeginChangeCheck();

                float newVal = GUILayout.HorizontalSlider(val, 0f, 1f,
                    GUILayout.MinWidth(50f), GUILayout.MaxWidth(100f), GUILayout.ExpandWidth(false));

                Rect last = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(last, MouseCursor.ResizeHorizontal);

                if (Event.current.type == EventType.MouseDown && last.Contains(Event.current.mousePosition))
                {
                    sizeSliderDragging = true;
                }

                if (sizeSliderDragging && Event.current.type == EventType.MouseDrag) Repaint();

                if (EditorGUI.EndChangeCheck())
                {
                    data.ItemScale01 = newVal;
                    Repaint();
                }
            }
        }

        private Rect BottomSizeBarRect()
        {
            float h = EditorStyles.toolbar.fixedHeight;
            return new Rect(0f, position.height - h, position.width, h);
        }
        #endregion

        #region Input: Window
        private void HandleDnD()
        {
            Event e = Event.current;

            if (BottomSizeBarRect().Contains(e.mousePosition) || sizeSliderDragging)
                return;

            Rect viewRect = new(0f, TabsTopHeight(), position.width, position.height - TabsTopHeight() - EditorStyles.toolbar.fixedHeight);

            if (!viewRect.Contains(e.mousePosition)) return;

            EditorFavorites_Window source = DragAndDrop.GetGenericData(DragSourceKey) as EditorFavorites_Window;

            if (e.type == EventType.DragUpdated)
            {
                if (source == this)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    e.Use();
                    return;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                e.Use();
            }

            if (e.type == EventType.DragPerform)
            {
                if (source == this)
                {
                    int insertBefore = GetDropInsertIndex(e.mousePosition);
                    MoveSelectionToIndex(insertBefore);
                    DragAndDrop.AcceptDrag();
                    e.Use();
                    return;
                }

                string[] paths = DragAndDrop.paths;
                if (paths != null && paths.Length > 0)
                {
                    FavoritesTab tab = data.GetSelected();
                    foreach (string p in paths)
                    {
                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(p);
                        if (obj) tab.Add(obj);
                    }
                    data.RefreshSelected();
                    data.Save();
                    DragAndDrop.AcceptDrag();
                    e.Use();
                }
            }
        }

        private void HandleKeys()
        {
            Event e = Event.current;
            if (e.type != EventType.KeyDown) return;

            if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
            {
                if (multiSel.Count > 0)
                {
                    FavoritesTab tab = data.GetSelected();
                    List<int> toRemove = new(multiSel);
                    toRemove.Sort();
                    for (int i = toRemove.Count - 1; i >= 0; i--) tab.RemoveAt(toRemove[i]);
                    data.RefreshSelected();
                    data.Save();
                    ClearSelection();
                    e.Use();
                }
            }

            if (e.keyCode == KeyCode.A && EditorGUI.actionKey)
            {
                SelectAll();
                e.Use();
            }
        }

        private void DeselectOnBackground()
        {
            Event e = Event.current;
            if (e.type != EventType.MouseDown || e.button != 0) return;

            Rect viewRect = new(0f, TabsTopHeight(), position.width, position.height - TabsTopHeight() - EditorStyles.toolbar.fixedHeight);

            if (!viewRect.Contains(e.mousePosition)) return;

            if (HitTestIndex(e.mousePosition) == -1)
            {
                ClearSelection();
                e.Use();
            }
        }

        private void HandleBackgroundClickInContent(Rect contentRect)
        {
            Event e = Event.current;
            if ((e.type == EventType.MouseDown || e.type == EventType.ContextClick) &&
                contentRect.Contains(e.mousePosition))
            {
                if (!e.shift && !EditorGUI.actionKey)
                {
                    ClearSelection();
                    e.Use();
                }
            }
        }
        #endregion

        #region Selection Helpers
        private int HitTestIndex(Vector2 mousePos)
        {
            FavoritesTab tab = data.GetSelected();
            int count = tab.Items.Count;
            if (count == 0) return -1;

            float viewTop = TabsTopHeight();
            float localY = mousePos.y - viewTop + scroll.y;

            if (IsListMode())
            {
                int idx = Mathf.FloorToInt(localY / kListRowH);
                if (idx < 0 || idx >= count) return -1;
                float rowY = viewTop - scroll.y + idx * kListRowH;
                Rect rect = new(0f, rowY, position.width, kListRowH);
                return rect.Contains(mousePos) ? idx : -1;
            }

            int tile = CurrentTilePx();
            int pad = CurrentPadPx();
            int full = tile + pad;
            int cols = Mathf.Max(1, Mathf.FloorToInt((position.width - 24f) / (tile + pad)));
            int row = Mathf.FloorToInt(localY / full);
            int col = Mathf.FloorToInt((mousePos.x - pad) / full);
            if (row < 0 || col < 0) return -1;

            int idxGrid = row * cols + col;
            if (idxGrid < 0 || idxGrid >= count) return -1;

            float x = pad + col * full;
            float y = viewTop - scroll.y + row * full;
            Rect tileRect = new(x, y, tile, tile);
            return tileRect.Contains(mousePos) ? idxGrid : -1;
        }

        private void SetSingle(int index)
        {
            multiSel.Clear();
            multiSel.Add(index);
            lastSelAnchor = index;
            UpdateUnitySelection();
            Repaint();
        }

        private void ClearSelection()
        {
            multiSel.Clear();
            lastSelAnchor = -1;
            UpdateUnitySelection();
            Repaint();
        }

        private void AddToSelection(int index)
        {
            multiSel.Add(index);
            UpdateUnitySelection();
            Repaint();
        }

        private void RemoveFromSelection(int index)
        {
            multiSel.Remove(index);
            UpdateUnitySelection();
            Repaint();
        }

        private void SelectRange(int a, int b)
        {
            multiSel.Clear();
            if (a > b) (a, b) = (b, a);
            int count = data.GetSelected().Items.Count;
            a = Mathf.Clamp(a, 0, count - 1);
            b = Mathf.Clamp(b, 0, count - 1);
            for (int i = a; i <= b; i++) multiSel.Add(i);
            UpdateUnitySelection();
            Repaint();
        }

        private void SelectAll()
        {
            multiSel.Clear();
            int count = data.GetSelected().Items.Count;
            for (int i = 0; i < count; i++) multiSel.Add(i);
            UpdateUnitySelection();
            Repaint();
        }

        private UnityEngine.Object[] CollectSelectedObjects()
        {
            IReadOnlyList<FavoritesItem> items = data.GetSelected().Items;
            List<UnityEngine.Object> objs = new();
            foreach (int i in multiSel)
                if (i >= 0 && i < items.Count && items[i].Asset) objs.Add(items[i].Asset);
            return objs.ToArray();
        }

        private void MoveSelectionToIndex(int insertBefore)
        {
            List<int> indices = new(multiSel);
            indices.Sort();
            FavoritesTab tab = data.GetSelected();
            int pivot = 0;
            foreach (int i in indices)
            {
                int from = i + pivot;
                int to = insertBefore + pivot;
                if (from < to) to--;
                tab.Move(from, to);
                pivot++;
            }
            data.RefreshSelected();
            Repaint();
        }

        private void UpdateUnitySelection()
        {
            IReadOnlyList<FavoritesItem> items = data.GetSelected().Items;
            List<UnityEngine.Object> objs = new();
            foreach (int i in multiSel)
                if (i >= 0 && i < items.Count && items[i].Asset) objs.Add(items[i].Asset);
            if (objs.Count > 0) Selection.objects = objs.ToArray();
            else Selection.activeObject = null;
        }
        #endregion

        #region Reorder Helpers
        private int GetDropInsertIndex(Vector2 mousePos)
        {
            float viewTop = TabsTopHeight();
            float localY = mousePos.y - viewTop + scroll.y;

            int count = data.GetSelected().Items.Count;
            if (IsListMode())
            {
                int row = Mathf.FloorToInt(localY / kListRowH);
                return Mathf.Clamp(row, 0, count);
            }

            int tile = CurrentTilePx();
            int pad = CurrentPadPx();
            int tileFull = tile + pad;
            int cols = Mathf.Max(1, Mathf.FloorToInt((position.width - 24f) / (tile + pad)));
            int rowIdx = Mathf.FloorToInt(localY / tileFull);
            int colIdx = Mathf.FloorToInt((mousePos.x - pad) / tileFull);
            if (rowIdx < 0) return 0;
            if (colIdx < 0) colIdx = 0;

            int idx = rowIdx * cols + colIdx;
            return Mathf.Clamp(idx, 0, count);
        }

        private int GetTabInsertIndex(float mouseGuiX)
        {
            float contentX = mouseGuiX - tabsVisibleRect.x + tabsScroll.x;
            for (int i = 0; i < tabRects.Count; i++)
            {
                float mid = (tabRects[i].xMin + tabRects[i].xMax) * 0.5f;
                if (contentX < mid) return i;
            }
            return tabRects.Count;
        }

        private void DrawTabInsertMarker(int insertBefore, float tabsDrawH)
        {
            if (tabRects.Count == 0) return;

            float contentX = insertBefore >= tabRects.Count
                ? tabRects[^1].xMax + 1f
                : tabRects[insertBefore].xMin - 1f;

            float guiX = contentX - tabsScroll.x + tabsVisibleRect.x;
            float y = tabsVisibleRect.y + 1f;
            EditorGUI.DrawRect(new Rect(guiX, y, 2f, tabsDrawH), new Color(0.24f, 0.48f, 0.90f, 1f));
        }

        private void EnsureTabVisible(int index, float contentW)
        {
            if (index < 0 || index >= tabRects.Count) return;

            float left = tabRects[index].xMin;
            float right = tabRects[index].xMax;
            float viewLeft = tabsScroll.x;
            float viewRight = tabsScroll.x + tabsVisibleRect.width;

            if (left < viewLeft) tabsScroll.x = left;
            else if (right > viewRight) tabsScroll.x = right - tabsVisibleRect.width;

            float maxX = Mathf.Max(0f, contentW - tabsVisibleRect.width);
            tabsScroll.x = Mathf.Clamp(tabsScroll.x, 0f, maxX);
        }
        #endregion

        #region Open Helpers
        private bool OpenByType(Object obj, Vector2 guiMousePos)
        {
            if (!obj) return false;

            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) return false;

            string ext = Path.GetExtension(path).ToLowerInvariant();
            switch (ext)
            {
                case ".unity":
                    return OpenScene(obj);

                case ".prefab":
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                    AssetDatabase.OpenAsset(obj);
                    return true;

                case ".cs":
                case ".uxml":
                case ".shader":
                case ".compute":
                case ".cginc":
                case ".hlsl":
                case ".uss":
                case ".asmdef":
                case ".json":
                case ".txt":
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".tga":
                case ".psd":
                case ".asset":
                default:
                    return OpenScript(obj);
            }
        }

        private bool OpenScript(Object obj)
        {
            if (!obj) return false;
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) return false;
            return AssetDatabase.OpenAsset(obj);
        }

        private bool OpenScene(Object obj)
        {
            if (!obj) return false;

            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            OpenSceneMode mode = openScenesAdditively ? OpenSceneMode.Additive : OpenSceneMode.Single;
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(path, mode);
            if (!scene.IsValid())
                return false;

            return true;
        }
        #endregion

        #region Live Preview Pump
        private static class LivePreviewPump
        {
            private static readonly HashSet<EditorFavorites_Window> windows = new();
            private static bool subscribed;
            private const double kMaxSeconds = 1.5;
            private static double deadline;

            public static void EnableOneShot(EditorFavorites_Window w)
            {
                windows.Add(w);
                deadline = EditorApplication.timeSinceStartup + kMaxSeconds;

                try { AssetPreview.SetPreviewTextureCacheSize(512); } catch { }

                if (!subscribed)
                {
                    subscribed = true;
                    EditorApplication.update += Tick;
                }
            }

            private static void Tick()
            {
                bool changed = IconCache.PollPreviewCompletions();

                if (changed)
                    foreach (EditorFavorites_Window w in windows) w.Repaint();

                bool stillWaiting = IconCache.AnyWaitingPreviews();
                bool timedOut = EditorApplication.timeSinceStartup >= deadline;

                if (!stillWaiting || timedOut)
                {
                    EditorApplication.update -= Tick;
                    windows.Clear();
                    subscribed = false;
                }
            }
        }
        #endregion

        #region Icon Cache
        private static class IconCache
        {
            private static readonly Dictionary<int, Texture> s_Small = new(256);
            private static readonly Dictionary<int, Texture> s_Large = new(256);
            private static readonly HashSet<int> s_Waiting = new();

            public static Texture GetSmall(Object obj)
            {
                if (!obj) return null;
                int id = obj.GetInstanceID();
                if (s_Small.TryGetValue(id, out var tex) && tex) return tex;

                Texture icon = AssetPreview.GetMiniThumbnail(obj);
                if (!icon) icon = AssetPreview.GetMiniTypeThumbnail(obj.GetType());
                if (!icon) icon = EditorGUIUtility.FindTexture("DefaultAsset Icon");
                if (!icon) icon = Texture2D.grayTexture;

                s_Small[id] = icon;
                return icon;
            }

            public static Texture GetLargeOrFallback(Object obj)
            {
                if (!obj) return null;
                int id = obj.GetInstanceID();

                if (s_Large.TryGetValue(id, out var cached) && cached) return cached;

                Texture preview = AssetPreview.GetAssetPreview(obj);
                if (preview)
                {
                    s_Large[id] = preview;
                    s_Waiting.Remove(id);
                    return preview;
                }

                s_Waiting.Add(id);

                return GetSmall(obj);
            }

            public static bool PollPreviewCompletions()
            {
                if (s_Waiting.Count == 0) return false;

                bool changed = false;
                var snap = ListPool<int>.Get();
                try
                {
                    foreach (var id in s_Waiting) snap.Add(id);

                    for (int i = snap.Count - 1; i >= 0; i--)
                    {
                        int id = snap[i];
                        if (!AssetPreview.IsLoadingAssetPreview(id))
                        {
                            Object obj = EditorUtility.InstanceIDToObject(id);
                            Texture tex = obj ? AssetPreview.GetAssetPreview(obj) : null;

                            if (tex)
                            {
                                s_Large[id] = tex;
                                changed = true;
                            }
                            s_Waiting.Remove(id);
                        }
                    }
                }
                finally { ListPool<int>.Release(snap); }

                return changed;
            }

            public static bool AnyWaitingPreviews() => s_Waiting.Count > 0;

            public static void Clear()
            {
                s_Small.Clear();
                s_Large.Clear();
                s_Waiting.Clear();
            }

            private static class ListPool<T>
            {
                private static readonly Stack<List<T>> pool = new();
                public static List<T> Get() => pool.Count > 0 ? pool.Pop() : new List<T>(64);
                public static void Release(List<T> list) { list.Clear(); pool.Push(list); }
            }
        }
        #endregion
    }
}
#endif