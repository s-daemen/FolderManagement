using System.Collections.Generic;
using UnityEngine;

namespace SD.FolderManagement {
    public static class PopupManager {
        public static PopupMenu CurrentPopup { get; set; }

        public static bool HasPopupControl() {
            return CurrentPopup != null;
        }

        public static void StartPopupGUI() {
            if (CurrentPopup != null && Event.current.type != EventType.Layout &&
                Event.current.type != EventType.Repaint) CurrentPopup.Draw();
        }

        public static void EndPopupGUI() {
            if (CurrentPopup != null && (Event.current.type == EventType.Layout ||
                                         Event.current.type == EventType.Repaint)) CurrentPopup.Draw();
        }
    }


    public class PopupMenu {
        public delegate void MenuFunction();

        public delegate void MenuFunctionData(object userData);

        // GUI variables
        public static GUIStyle backgroundStyle;

        // public static Texture2D expandRight;
        public static float itemHeight;

        public static GUIStyle selectedLabel;
        private bool close;
        private float currentItemHeight;
        private MenuItem groupToDraw;

        public List<MenuItem> menuItems = new List<MenuItem>();

        public float minWidth;

        // State
        private Rect position;

        private string selectedPath;

        public PopupMenu() {
            SetupGUI();
        }

        public Vector2 Position {
            get { return position.position; }
        }

        public void SetupGUI() {
            backgroundStyle = new GUIStyle(GUI.skin.box);
            backgroundStyle.contentOffset = new Vector2(2, 2);
            itemHeight = GUI.skin.label.CalcHeight(new GUIContent("text"), 100);

            selectedLabel = new GUIStyle(GUI.skin.label);
            selectedLabel.normal.background = FolderManagerGUI.CreatePixelTexture(1, new Color(0.4f, 0.4f, 0.4f));
        }

        public void Show(Vector2 pos, float MinWidth = 40) {
            minWidth = MinWidth;
            position = calculateRect(pos, menuItems, minWidth);
            selectedPath = "";
            PopupManager.CurrentPopup = this;
        }

        #region Nested MenuItem

        public class MenuItem {
            // -!Separator
            public GUIContent content;

            // -Executable Item
            public MenuFunction func;

            public MenuFunctionData funcData;

            // --Group
            public bool group;

            public Rect groupPos;

            public string path;

            // -Non-executables
            public bool separator;

            public List<MenuItem> subItems;
            public object userData;

            public MenuItem() {
                separator = true;
            }

            public MenuItem(string _path, GUIContent _content, bool _group) {
                path = _path;
                content = _content;
                group = _group;

                if (group)
                    subItems = new List<MenuItem>();
            }

            public MenuItem(string _path, GUIContent _content, MenuFunction _func) {
                path = _path;
                content = _content;
                func = _func;
            }

            public MenuItem(string _path, GUIContent _content, MenuFunctionData _func, object _userData) {
                path = _path;
                content = _content;
                funcData = _func;
                userData = _userData;
            }

            public void Execute() {
                if (funcData != null)
                    funcData(userData);
                else if (func != null)
                    func();
            }
        }

        #endregion

        #region Creation

        public void AddItem(GUIContent content, bool on, MenuFunctionData func, object userData) {
            string path;
            var parent = AddHierarchy(ref content, out path);
            if (parent != null)
                parent.subItems.Add(new MenuItem(path, content, func, userData));
            else
                menuItems.Add(new MenuItem(path, content, func, userData));
        }

        public void AddItem(GUIContent content, bool on, MenuFunction func) {
            string path;
            var parent = AddHierarchy(ref content, out path);
            if (parent != null)
                parent.subItems.Add(new MenuItem(path, content, func));
            else
                menuItems.Add(new MenuItem(path, content, func));
        }

        public void AddSeparator(string path) {
            var content = new GUIContent(path);
            var parent = AddHierarchy(ref content, out path);
            if (parent != null)
                parent.subItems.Add(new MenuItem());
            else
                menuItems.Add(new MenuItem());
        }

        private MenuItem AddHierarchy(ref GUIContent content, out string path) {
            path = content.text;
            if (path.Contains("/")) {
                // is inside a group
                var subContents = path.Split('/');
                var folderPath = subContents[0];

                // top level group
                var parent =
                    menuItems.Find(item => item.content != null && item.content.text == folderPath && item.group);
                if (parent == null)
                    menuItems.Add(parent = new MenuItem(folderPath, new GUIContent(folderPath), true));
                // additional level groups
                for (var groupCnt = 1; groupCnt < subContents.Length - 1; groupCnt++) {
                    var folder = subContents[groupCnt];
                    folderPath += "/" + folder;
                    if (parent == null)
                        Debug.LogError("Parent is null!");
                    else if (parent.subItems == null)
                        Debug.LogError("Subitems of " + parent.content.text + " is null!");
                    var subGroup =
                        parent.subItems.Find(item => item.content != null && item.content.text == folder && item.group);
                    if (subGroup == null)
                        parent.subItems.Add(subGroup = new MenuItem(folderPath, new GUIContent(folder), true));
                    parent = subGroup;
                }

                // actual item
                path = content.text;
                content = new GUIContent(subContents[subContents.Length - 1], content.tooltip);
                return parent;
            }
            return null;
        }

        #endregion

        #region Drawing

        public void Draw() {
            var inRect = DrawGroup(position, menuItems);

            while (groupToDraw != null && !close) {
                var group = groupToDraw;
                groupToDraw = null;
                if (group.group)
                    if (DrawGroup(group.groupPos, group.subItems))
                        inRect = true;
            }

            if (!inRect || close) PopupManager.CurrentPopup = null;
            FolderManager.RepaintClients();
        }

        private bool DrawGroup(Rect pos, List<MenuItem> menuItems) {
            var rect = calculateRect(pos.position, menuItems, minWidth);

            var clickRect = new Rect(rect);
            clickRect.xMax += 20;
            clickRect.xMin -= 20;
            clickRect.yMax += 20;
            clickRect.yMin -= 20;
            var inRect = clickRect.Contains(Event.current.mousePosition);

            currentItemHeight = backgroundStyle.contentOffset.y;
            GUI.BeginGroup(extendRect(rect, backgroundStyle.contentOffset), GUIContent.none, backgroundStyle);
            for (var itemCnt = 0; itemCnt < menuItems.Count; itemCnt++) {
                DrawItem(menuItems[itemCnt], rect);
                if (close) break;
            }
            GUI.EndGroup();

            return inRect;
        }

        private void DrawItem(MenuItem item, Rect groupRect) {
            if (item.separator) {
                if (Event.current.type == EventType.Repaint)
                    FolderManagerGUI.Seperator(new Rect(backgroundStyle.contentOffset.x + 1, currentItemHeight + 1,
                        groupRect.width - 2, 1));
                currentItemHeight += 3;
            }
            else {
                var labelRect = new Rect(backgroundStyle.contentOffset.x, currentItemHeight, groupRect.width,
                    itemHeight);

                if (labelRect.Contains(Event.current.mousePosition))
                    selectedPath = item.path;

                var selected = selectedPath == item.path || selectedPath.Contains(item.path + "/");
                GUI.Label(labelRect, item.content, selected ? selectedLabel : GUI.skin.label);

                if (item.group) {
                    GUI.DrawTexture(
                        new Rect(labelRect.x + labelRect.width - 12, labelRect.y + (labelRect.height - 12) / 2, 12, 12),
                        Texture2D.blackTexture);
                    if (selected) {
                        item.groupPos = new Rect(groupRect.x + groupRect.width + 4, groupRect.y + currentItemHeight - 2,
                            0, 0);
                        groupToDraw = item;
                    }
                }
                else if (selected && (Event.current.type == EventType.MouseDown ||
                                      Event.current.button != 1 && Event.current.type == EventType.MouseUp)) {
                    item.Execute();
                    close = true;
                    Event.current.Use();
                }

                currentItemHeight += itemHeight;
            }
        }

        private static Rect extendRect(Rect rect, Vector2 extendValue) {
            rect.x -= extendValue.x;
            rect.y -= extendValue.y;
            rect.width += extendValue.x + extendValue.x;
            rect.height += extendValue.y + extendValue.y;
            return rect;
        }

        private static Rect calculateRect(Vector2 position, List<MenuItem> menuItems, float minWidth) {
            Vector2 size;
            float width = minWidth, height = 0;

            for (var itemCnt = 0; itemCnt < menuItems.Count; itemCnt++) {
                var item = menuItems[itemCnt];
                if (item.separator) {
                    height += 3;
                }
                else {
                    width = Mathf.Max(width, GUI.skin.label.CalcSize(item.content).x + (item.group ? 22 : 10));
                    height += itemHeight;
                }
            }

            size = new Vector2(width, height);
            var down = position.y + size.y <= Screen.height;
            return new Rect(position.x, position.y - (down ? 0 : size.y), size.x, size.y);
        }

        #endregion
    }


    public class GenericMenu {
        private static PopupMenu _popup;

        public GenericMenu() {
            _popup = new PopupMenu();
        }

        public Vector2 Position {
            get { return _popup.Position; }
        }

        public void ShowAsContext() {
            _popup.Show(Event.current.mousePosition);
        }

        public void Show(Vector2 pos, float minWidth = 40) {
            _popup.Show(pos, minWidth);
        }

        public void AddItem(GUIContent content, bool on, PopupMenu.MenuFunctionData func, object userData) {
            _popup.AddItem(content, on, func, userData);
        }

        public void AddItem(GUIContent content, bool on, PopupMenu.MenuFunction func) {
            _popup.AddItem(content, on, func);
        }

        public void AddSeparator(string path) {
            _popup.AddSeparator(path);
        }
    }
}