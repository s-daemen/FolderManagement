using System;
using System.Collections.Generic;
using System.Linq;
using SD.FolderManagement.Model;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SD.FolderManagement {
    public class TreeViewItem<T> : TreeViewItem where T : TreeElement {
        public TreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName) {
            Data = data;
        }

        public T Data { get; set; }
    }

    public class TreeViewWithModel<T> : TreeView where T : TreeElement {
        // Dragging

        private const string GENERIC_DRAG_ID = "GenericDragColumnDragging";

        private readonly List<TreeViewItem> _rows = new List<TreeViewItem>();


        public TreeViewWithModel(TreeViewState state, TreeModel<T> model) : base(state) {
            Init(model);
        }

        public TreeViewWithModel(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model)
            : base(state, multiColumnHeader) {
            Init(model);
        }

        public TreeModel<T> TreeModel { get; private set; }

        public event Action TreeChanged;
        public event Action<IList<TreeViewItem>> BeforeDroppingDraggedItems;

        private void Init(TreeModel<T> model) {
            TreeModel = model;
            TreeModel.ModelChanged += ModelChanged;
        }

        private void ModelChanged() {
            if (TreeChanged != null)
                TreeChanged();

            Reload();
        }


        protected override TreeViewItem BuildRoot() {
            var depthForHiddenRoot = -1;
            return new TreeViewItem<T>(TreeModel.Root.Id, depthForHiddenRoot, TreeModel.Root.Name, TreeModel.Root);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
            if (TreeModel.Root == null) Debug.LogError("tree model root is null. did you call SetData()?");

            _rows.Clear();
            if (!string.IsNullOrEmpty(searchString)) {
                Search(TreeModel.Root, searchString, _rows);
            }
            else {
                if (TreeModel.Root != null && TreeModel.Root.HasChildren)
                    AddChildrenRecursive(TreeModel.Root, 0, _rows);
            }

            // We still need to setup the child parent information for the rows since this 
            // information is used by the TreeView internal logic (navigation, dragging etc)
            SetupParentsAndChildrenFromDepths(root, _rows);

            return _rows;
        }

        private void AddChildrenRecursive(T parent, int depth, ICollection<TreeViewItem> newRows) {
            foreach (var treeElement in parent.Children) {
                var child = (T) treeElement;
                var item = new TreeViewItem<T>(child.Id, depth, child.Name, child);
                newRows.Add(item);

                if (!child.HasChildren) continue;
                if (IsExpanded(child.Id)) AddChildrenRecursive(child, depth + 1, newRows);
                else item.children = CreateChildListForCollapsedParent();
            }
        }

        private void Search(T searchFromThis, string search, List<TreeViewItem> result) {
            if (string.IsNullOrEmpty(search))
                throw new ArgumentException("Invalid search: cannot be null or empty", "search");

            const int kItemDepth = 0; // tree is flattened when searching

            var stack = new Stack<T>();
            foreach (var element in searchFromThis.Children)
                stack.Push((T) element);
            while (stack.Count > 0) {
                var current = stack.Pop();
                // Matches search?
                if (current.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    result.Add(new TreeViewItem<T>(current.Id, kItemDepth, current.Name, current));

                if (current.Children == null || current.Children.Count <= 0) continue;
                foreach (var element in current.Children) stack.Push((T) element);
            }
            SortSearchResult(result);
        }

        protected virtual void SortSearchResult(List<TreeViewItem> rows) {
            rows.Sort((x, y) => EditorUtility.NaturalCompare(x.displayName,
                y.displayName)); // sort by displayName by default, can be overriden for multicolumn solutions
        }

        protected override IList<int> GetAncestors(int id) {
            return TreeModel.GetAncestors(id);
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id) {
            return TreeModel.GetDescendantsThatHaveChildren(id);
        }

        protected override bool CanStartDrag(CanStartDragArgs args) {
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args) {
            if (hasSearch)
                return;
            DragAndDrop.PrepareStartDrag();
            var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
            DragAndDrop.SetGenericData(GENERIC_DRAG_ID, draggedRows);
            DragAndDrop.objectReferences = new Object[] { }; // this IS required for dragging to work
            var title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
            DragAndDrop.StartDrag(title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args) {
            // Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
            var draggedRows = DragAndDrop.GetGenericData(GENERIC_DRAG_ID) as List<TreeViewItem>;
            if (draggedRows == null)
                return DragAndDropVisualMode.None;

            // Parent item is null when dragging outside any tree view items.
            switch (args.dragAndDropPosition) {
                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.BetweenItems: {
                    var validDrag = ValidDrag(args.parentItem, draggedRows);
                    if (args.performDrop && validDrag) {
                        var parentData = ((TreeViewItem<T>) args.parentItem).Data;
                        OnDropDraggedElementsAtIndex(draggedRows, parentData,
                            args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
                    }
                    return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                }

                case DragAndDropPosition.OutsideItems: {
                    if (args.performDrop)
                        OnDropDraggedElementsAtIndex(draggedRows, TreeModel.Root, TreeModel.Root.Children.Count);

                    return DragAndDropVisualMode.Move;
                }
                default:
                    Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
                    return DragAndDropVisualMode.None;
            }
        }

        public virtual void OnDropDraggedElementsAtIndex(List<TreeViewItem> draggedRows, T parent, int insertIndex) {
            if (BeforeDroppingDraggedItems != null)
                BeforeDroppingDraggedItems(draggedRows);

            var draggedElements = new List<TreeElement>();
            foreach (var x in draggedRows)
                draggedElements.Add(((TreeViewItem<T>) x).Data);

            var selectedIDs = draggedElements.Select(x => x.Id).ToArray();
            TreeModel.MoveElements(parent, insertIndex, draggedElements);
            SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
        }

        private bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems) {
            var currentParent = parent;
            while (currentParent != null) {
                if (draggedItems.Contains(currentParent))
                    return false;
                currentParent = currentParent.parent;
            }
            return true;
        }
    }
}