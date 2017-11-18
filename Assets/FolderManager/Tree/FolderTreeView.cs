using System;
using System.Collections.Generic;
using System.Linq;
using SD.FolderManagement.Model;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace SD.FolderManagement {
    public class FolderTreeView : TreeViewWithModel<FolderTreeElement> {
        public enum SortOption {
            Name
        }

        private const float ROW_HEIGHTS = 20f;
        private const float TOGGLE_WIDTH = 18f;


        private static readonly Texture2D[] _icons = {
            EditorGUIUtility.FindTexture("Folder Icon"),
            EditorGUIUtility.FindTexture("AudioSource Icon"),
            EditorGUIUtility.FindTexture("Camera Icon"),
            EditorGUIUtility.FindTexture("Windzone Icon"),
            EditorGUIUtility.FindTexture("GameObject Icon")
        };

        private readonly SortOption[] _sortOptions = {
            SortOption.Name,
            SortOption.Name,
            SortOption.Name
        };

        public FolderTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader,
            TreeModel<FolderTreeElement> model) : base(state, multicolumnHeader, model) {
            Assert.AreEqual(_sortOptions.Length, Enum.GetValues(typeof(ElementColumns)).Length,
                "Ensure number of sort options are in sync with number of MyColumns enum values");

            // Custom setup
            rowHeight = ROW_HEIGHTS;
            columnIndexForTreeFoldouts = 1;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset =
                (ROW_HEIGHTS - EditorGUIUtility.singleLineHeight) *
                0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = TOGGLE_WIDTH;
            multicolumnHeader.sortingChanged += OnSortingChanged;

            Reload();
        }

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result) {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            var stack = new Stack<TreeViewItem>();
            for (var i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0) {
                var current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                    for (var i = current.children.Count - 1; i >= 0; i--) stack.Push(current.children[i]);
            }
        }

        protected override void ContextClicked() {
            SetSelection(new List<int> {-1});
            Event.current.Use();
        }

        protected override void ContextClickedItem(int id) {
            FindItem(id, rootItem);
            Event.current.Use();
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
            var rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        private void OnSortingChanged(MultiColumnHeader multicolumnheader) {
            SortIfNeeded(rootItem, GetRows());
        }

        private void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows) {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
                return; // No column to sort for (just use the order the data are in)

            // Sort the roots of the existing tree items
            SortByMultipleColumns();
            TreeToList(root, rows);
            Repaint();
            FolderManager.RepaintClients();
        }

        private void SortByMultipleColumns() {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var myTypes = rootItem.children.Cast<TreeViewItem<FolderTreeElement>>();
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (var i = 1; i < sortedColumns.Length; i++) {
                var sortOption = _sortOptions[sortedColumns[i]];
                var ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption) {
                    case SortOption.Name:
                        orderedQuery = orderedQuery.ThenBy(l => l.Data.Name, ascending);
                        break;
                    default:
                        orderedQuery = orderedQuery.ThenBy(l => l.Data.Name, ascending);
                        break;
                }
            }
            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        private IOrderedEnumerable<TreeViewItem<FolderTreeElement>> InitialOrder(
            IEnumerable<TreeViewItem<FolderTreeElement>> myTypes, int[] history) {
            var sortOption = _sortOptions[history[0]];
            var ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption) {
                case SortOption.Name:
                    return myTypes.Order(l => l.Data.Name, ascending);
                default:
                    Assert.IsTrue(false, "Unhandled enum");
                    break;
            }

            // default
            return myTypes.Order(l => l.Data.Name, ascending);
        }

        private int GetIconIndex(TreeViewItem<FolderTreeElement> item) {
            return 0;
        }

        protected override void RowGUI(RowGUIArgs args) {
            var item = (TreeViewItem<FolderTreeElement>) args.item;

            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), item, (ElementColumns) args.GetColumn(i), ref args);
        }

        private void CellGUI(Rect cellRect, TreeViewItem<FolderTreeElement> item, ElementColumns column,
            ref RowGUIArgs args) {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column) {
                case ElementColumns.Icon: {
                    GUI.DrawTexture(cellRect, _icons[GetIconIndex(item)], ScaleMode.ScaleToFit);
                    break;
                }
                case ElementColumns.Name: {
                    // Do toggle
                    var toggleRect = cellRect;
                    toggleRect.x += GetContentIndent(item);
                    toggleRect.width = TOGGLE_WIDTH;
                    if (toggleRect.xMax < cellRect.xMax)
                        item.Data.Selected =
                            EditorGUI.Toggle(toggleRect, item.Data.Selected); // hide when outside cell rect

                    // Default icon and label
                    args.rowRect = cellRect;
                    base.RowGUI(args);
                }
                    break;
                case ElementColumns.Comment: {
                    cellRect.xMin += 5f;
                    if (column == ElementColumns.Name)
                        item.Data.Name = GUI.TextField(cellRect, item.Data.Name);
                    if (column == ElementColumns.Comment)
                        item.Data.Comment = GUI.TextField(cellRect, item.Data.Comment);
                    break;
                }
            }
        }

        //Renaming

        protected override bool CanRename(TreeViewItem item) {
            var renameRect = GetRenameRect(treeViewRect, 0, item);
            return renameRect.width > 25 || Event.current.button == 0;
        }

        protected override void RenameEnded(RenameEndedArgs args) {
            if (args.acceptedRename) {
                var element = TreeModel.Find(args.itemID);
                element.Name = args.newName;
                Reload();
            }
        }
        /*
                protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item) {
                    Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
                    CenterRectUsingSingleLineHeight(ref cellRect);

                    return base.GetRenameRect(cellRect, row, item);
                }*/

        // Misc

        protected override bool CanMultiSelect(TreeViewItem item) {
            return true;
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth) {
            var columns = new[] {
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByType"),
                        "Filter by folder type"),
                    contextMenuText = "Type",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent("Name", "Filter by name."),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 250,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent("Note",
                        "Filter by notes."),
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 70,
                    minWidth = 60,
                    autoResize = true
                }
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(ElementColumns)).Length,
                "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }

        private enum ElementColumns {
            Icon,
            Name,
            Comment
        }

        #region Lookups

        public TreeViewItem GetSelectedItem() {
            return IsSelected(state.lastClickedID) ? FindItem(state.lastClickedID, rootItem) : null;
        }

        public IList<TreeViewItem> GetSelectedItemsAsItems() {
            return FindRows(GetSelection());
        }

        #endregion
    }

    internal static class ExtensionMethods {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector,
            bool ascending) {
            if (ascending) return source.OrderBy(selector);
            return source.OrderByDescending(selector);
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector,
            bool ascending) {
            if (ascending) return source.ThenBy(selector);
            return source.ThenByDescending(selector);
        }
    }
}