using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SD.FolderManagement.Model;
using SD.FolderManagement.Utility;
using UnityEditor.IMGUI.Controls;

namespace SD.FolderManagement.Editor {

    public class FolderManagerEditorWindow : EditorWindow {

        public static FolderTreeCache Cache;

        private static FolderManagerEditorWindow _editor;
        private Vector2 _createScrollPosition;


        [NonSerialized]
        private bool _initialized;

        [SerializeField]
        private FolderTreeViewState _treeViewState;

        [SerializeField]
        private MultiColumnHeaderState _multiColumnHeaderState;
        private SearchField _searchField;
        private FolderTreeView _treeView;

        #region Properties

        public FolderTreeView TreeView {
            get { return _treeView; }
        }

        #endregion

        public static FolderManagerEditorWindow Editor {
            get {
                GetEditor();
                return _editor;
            }
        }

        private static void GetEditor() {
            if (_editor == null) {
                OpenEditor();
            }
        }

        [MenuItem("Tool/FolderManagement")]
        private static FolderManagerEditorWindow OpenEditor() {
            _editor = GetWindow<FolderManagerEditorWindow>();
            _editor.minSize = new Vector2(800, 400);
            _editor.titleContent = new GUIContent("FolderManager", "Make your life suck a little less!");
            _editor.Focus();
            _editor.Repaint();
            FolderManager.ReInit();
            return _editor;
        }

        #region Initialization

        private void Initialize() {
            if (!_initialized) {
                if (Cache.FolderTree == null) return;
                if (_treeViewState == null) {
                    _treeViewState = new FolderTreeViewState();
                }
                bool firstInit = _multiColumnHeaderState == null;
                var headerState = FolderTreeView.CreateDefaultMultiColumnHeaderState(FolderTreeViewRect().width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(_multiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(_multiColumnHeaderState, headerState);
                _multiColumnHeaderState = headerState;

                var multiColumnHeader = new FolderMultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new TreeModel<FolderTreeElement>(GetData());
                _treeView = new FolderTreeView(_treeViewState, multiColumnHeader, treeModel);
                _treeViewState.View = _treeView;
                _treeViewState.Cache = Cache;
                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;
                _initialized = true;
            }
        }

        private void OnEnable() {
            _editor = this;
            FolderManager.Initialize();

            FolderManager.ClientRepaints -= Repaint;
            FolderManager.ClientRepaints += Repaint;

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;

            FolderManagerCallbacks.OnAddElement -= ElementAdded;
            FolderManagerCallbacks.OnAddElement += ElementAdded;

            FolderManagerCallbacks.OnDeleteElement -= ElementAdded;
            FolderManagerCallbacks.OnDeleteElement += ElementAdded;

            Cache = new FolderTreeCache(DirectoryUtility.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))));
            Cache.SetupCacheEvents();
        }

        private void ElementAdded(FolderTreeElement obj) {
            _initialized = false;
        }

        private void OnDestroy() {
            EditorUtility.SetDirty(Cache.FolderTree);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            FolderManagerCallbacks.OnAddElement -= ElementAdded;
            FolderManagerCallbacks.OnDeleteElement -= ElementAdded;

            FolderManager.ClientRepaints -= Repaint;
            SceneView.onSceneGUIDelegate -= OnSceneGUI;

            Cache.ClearCacheEvents();
        }

        private void OnSceneGUI(SceneView sceneView) {
            DrawGUI();
        }

        private void DrawGUI() {
            SceneView.lastActiveSceneView.Repaint();
        }

        private IList<FolderTreeElement> GetData() {
            if (Cache.FolderTree != null && Cache.FolderTree.TreeElements != null && Cache.FolderTree.TreeElements.Count > 0) {
                return Cache.FolderTree.TreeElements;

            } else {
                return null;
            }

        }

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        private static bool AutoOpenCanvas(int instanceID, int line) {
            if (Selection.activeObject != null && Selection.activeObject is FolderTree) {
                string folderTreePath = AssetDatabase.GetAssetPath(instanceID);
                OpenEditor();
                Cache.LoadFolderTree(folderTreePath);
                return true;
            }
            return false;
        }

        #endregion

        #region GUI

        private Rect FolderTreeViewRect() {
            return new Rect(20, 30, 350, 280);
        }

        private Rect ToolbarRect() {
            return new Rect(20f, 10f, 350, 20f);
        }

        private Rect BottomToolbarRect() {
            return new Rect(20f, 320, 350, 20f);
        }

        private void OnGUI() {
            GetEditor();
            Cache.AssureFolderTree();

            Initialize();
            if (WindowUtility.ShouldShowWindow()) {
                FolderManagerGUI.Title("FolderManager");
                EditorGUILayout.Space();
                PopupManager.StartPopupGUI();
                ShowBody();
                FMInputManager.HandleInputEvents(_treeViewState);

                FMInputManager.HandleLateInputEvents(_treeViewState);
                PopupManager.EndPopupGUI();
            }
        }

        #region GUI -> Elements

        private void SearchBar(Rect toolbarRect) {
            TreeView.searchString = _searchField.OnGUI(toolbarRect, TreeView.searchString);
        }

        private void ShowBody() {
            float maxWidth = position.width * 0.5f;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            Rect positionCheck = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(maxWidth));

            FolderManagerGUI.Title(positionCheck, "Manage folder tree");

            _createScrollPosition = GUILayout.BeginScrollView(_createScrollPosition, "HelpBox", GUILayout.MaxHeight(350), GUILayout.MaxWidth(maxWidth));

            if (Cache.FolderTree != null) {
                SearchBar(ToolbarRect());
            }

            DrawCurrentTree(FolderTreeViewRect());

            GUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            positionCheck = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(maxWidth));

            FolderManagerGUI.Title(positionCheck, "Settings");
            _createScrollPosition = GUILayout.BeginScrollView(_createScrollPosition, "HelpBox", GUILayout.MaxHeight(350), GUILayout.MaxWidth(maxWidth));

            DrawSettings();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCurrentTree(Rect rect) {
            if (Cache.FolderTree == null) {
                EditorGUILayout.LabelField("You need to select or create a folder tree first!");
                return;
            }
            _treeView.OnGUI(rect);
            BottomToolBar(BottomToolbarRect());
        }

        private void BottomToolBar(Rect rect) {
            GUILayout.BeginArea(rect);
            using (new EditorGUILayout.HorizontalScope()) {
                GUIStyle style = "miniButton";
                if (GUILayout.Button("Expand All", style)) {
                    _treeView.ExpandAll();
                }
                if (GUILayout.Button("Collapse All", style)) {
                    _treeView.CollapseAll();
                }
                GUILayout.FlexibleSpace();
                GUILayout.Label(Cache.FolderTree != null
                    ? AssetDatabase.GetAssetPath(Cache.FolderTree)
                    : string.Empty);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndArea();
        }

        private void DrawSettings() {

            Cache.FolderTree.TreeName = EditorGUILayout.TextField(new GUIContent("Tree Name"), Cache.FolderTree.TreeName);
            GUILayout.Space(6);

            if (GUILayout.Button(new GUIContent("Create Folder Tree", "Create a Folder Tree"))) {
                Cache.LoadFolderTree("");

                _initialized = false;
            }

            if (GUILayout.Button(new GUIContent("Load Folder Tree", "Loads the Folder Tree from a Save File in the Assets Folder"))) {
                string path = EditorUtility.OpenFilePanel("Load Folder Tree", FolderManager.SavesFolderPath(), "asset");
                if (!path.Contains(DirectoryUtility.GetAppDataPath())) {
                    if (!string.IsNullOrEmpty(path))
                        ShowNotification(new GUIContent("You should select an asset inside your project folder!"));
                } else {
                    Cache.LoadFolderTree(path);
                    _initialized = false;
                }
            }

            if (GUILayout.Button(new GUIContent("Save Folder Tree", "Saves the Folder Tree in the Assets Folder"))) {
                string path = EditorUtility.SaveFilePanelInProject("Save Folder Tree", Cache.FolderTree.TreeName, "asset", "", FolderManager.SavesFolderPath());
                if (!string.IsNullOrEmpty(path))
                    Cache.SaveFolderTree(path);
                _initialized = false;
            }

            if (GUILayout.Button(new GUIContent("Generate Folder Tree", "Generates the Folder Tree"))) {
                if (!FolderManager.Generate(Cache.FolderTree)) {
                    ShowNotification(new GUIContent("To generate a tree, you must first add folders to it!"));
                }
                else {
                    ShowNotification(new GUIContent("Successfully generated a folder tree! Go check it out in the project folder!"));
                }
            }
        }
       
        #endregion

        #endregion

    }

    internal class FolderMultiColumnHeader : MultiColumnHeader {
        Mode m_Mode;

        public enum Mode {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public FolderMultiColumnHeader(MultiColumnHeaderState state)
            : base(state) {
            mode = Mode.DefaultHeader;
        }

        public Mode mode {
            get {
                return m_Mode;
            }
            set {
                m_Mode = value;
                switch (m_Mode) {
                    case Mode.LargeHeader:
                        canSort = true;
                        height = 37f;
                        break;
                    case Mode.DefaultHeader:
                        canSort = true;
                        height = DefaultGUI.defaultHeight;
                        break;
                    case Mode.MinimumHeaderWithoutSorting:
                        canSort = false;
                        height = DefaultGUI.minimumHeight;
                        break;
                }
            }
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex) {
            // Default column header gui
            base.ColumnHeaderGUI(column, headerRect, columnIndex);

            // Add additional info for large header
            if (mode == Mode.LargeHeader) {
                // Show example overlay stuff on some of the columns
                if (columnIndex > 2) {
                    headerRect.xMax -= 3f;
                    var oldAlignment = EditorStyles.largeLabel.alignment;
                    EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
                    GUI.Label(headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.alignment = oldAlignment;
                }
            }
        }
    }
}
