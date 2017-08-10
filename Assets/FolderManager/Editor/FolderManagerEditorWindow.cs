using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SD.FolderManagement.Model;
using SD.FolderManagement.Utility;
using UnityEditor.IMGUI.Controls;

namespace SD.FolderManagement.Editor {

    public class FolderManagerEditorWindow : EditorWindow {

        private static FolderManagerEditorWindow _editor;
        private Vector2 _createScrollPosition;
        private Vector2 _scrollPosition;

        public static FolderTreeCache folderTreeCache;

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
            _editor.minSize = new Vector2(800, 600);
            _editor.titleContent = new GUIContent("FolderManager", "Make your life suck a little less!");
            _editor.Focus();
            _editor.Repaint();
            FolderManager.ReInit();
            return _editor;
        }

        #region Initialization

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

            folderTreeCache = new FolderTreeCache(DirectoryUtility.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))));
            folderTreeCache.SetupCacheEvents();
        }

        private void ElementAdded(FolderTreeElement obj) {
            _initialized = false;
        }

        private void OnDestroy() {
            EditorUtility.SetDirty(folderTreeCache.FolderTree);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            FolderManagerCallbacks.OnAddElement -= ElementAdded;
            FolderManagerCallbacks.OnDeleteElement -= ElementAdded;

            FolderManager.ClientRepaints -= Repaint;
            SceneView.onSceneGUIDelegate -= OnSceneGUI;

            folderTreeCache.ClearCacheEvents();
        }

        private void OnSceneGUI(SceneView sceneView) {
            DrawGUI();
        }

        private void DrawGUI() {
            SceneView.lastActiveSceneView.Repaint();
        }

        private void Initialize() {
            if (!_initialized) {
                if (folderTreeCache.FolderTree == null) return;
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
                _treeViewState.Cache = folderTreeCache;
                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;
                _initialized = true;
            }
        }



        private IList<FolderTreeElement> GetData() {
            if (folderTreeCache.FolderTree != null && folderTreeCache.FolderTree.TreeElements != null && folderTreeCache.FolderTree.TreeElements.Count > 0) {
                return folderTreeCache.FolderTree.TreeElements;

            } else {
                return null;
            }

        }
        #endregion

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        private static bool AutoOpenCanvas(int instanceID, int line) {
            if (Selection.activeObject != null && Selection.activeObject is FolderTree) {
                string NodeCanvasPath = AssetDatabase.GetAssetPath(instanceID);
                FolderManagerEditorWindow.OpenEditor();
                folderTreeCache.LoadFolderTree(NodeCanvasPath);
                return true;
            }
            return false;
        }

        private Rect FolderTreeViewRect() {
            return new Rect(20, 30, 350, 300);
        }

        private Rect ToolbarRect() {
            return new Rect(20f, 10f, 350, 20f);
        }

        private Rect BottomToolbarRect() {
            return new Rect(20f, position.height - 18f, position.width - 40f, 16f);
        }

        private void OnGUI() {
            GetEditor();
            folderTreeCache.AssureFolderTree();

            Initialize();
            if (WindowUtility.ShouldShowWindow()) {
                FolderManagerGUI.Title("FolderManager");
                EditorGUILayout.Space();
                PopupManager.StartPopupGUI();
                ShowContextSettings();
                ShowFolderStructureWindow();
                FMInputManager.HandleInputEvents(_treeViewState);

                FMInputManager.HandleLateInputEvents(_treeViewState);
                PopupManager.EndPopupGUI();
            }
        }

        private void SearchBar(Rect toolbarRect) {
            TreeView.searchString = _searchField.OnGUI(toolbarRect, TreeView.searchString);
        }

        private void ShowContextSettings() {
            float maxWidth = position.width * 0.5f;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            Rect positionCheck = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(maxWidth));
            FolderManagerGUI.Title(positionCheck, "Manage folder tree");
            _createScrollPosition = GUILayout.BeginScrollView(_createScrollPosition, "HelpBox", GUILayout.MaxHeight(350), GUILayout.MaxWidth(maxWidth));

            if (folderTreeCache.FolderTree != null) {
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
            DrawSettings(positionCheck);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCurrentTree(Rect rect) {
            if (folderTreeCache.FolderTree == null) {
                EditorGUILayout.LabelField("You need to select or create a folder tree first!");
                return;
            }
            _treeView.OnGUI(rect);
        }

        private void DrawSettings(Rect pos) {
            float fullWindowWidth = pos.width + 30;
            float controlHeight = pos.height * 1.5f;
            Rect newPosition = new Rect(20, 30, 50, 50);
            newPosition.width = fullWindowWidth / 2;
            newPosition.x += newPosition.width / 2;
            newPosition.height = controlHeight;
            newPosition.y += controlHeight;
            if (GUILayout.Button(new GUIContent("Create Folder Tree", "Create a Folder Tree")))
            {
                folderTreeCache.LoadFolderTree("");

                _initialized = false;
            }



            if (GUILayout.Button(new GUIContent("Load Folder Tree", "Loads the Folder Tree from a Save File in the Assets Folder"))) {
                string path = EditorUtility.OpenFilePanel("Load Folder Tree", FolderManager.SavesFolderPath(), "asset");
                if (!path.Contains(DirectoryUtility.GetAppDataPath())) {
                    if (!string.IsNullOrEmpty(path))
                        ShowNotification(new GUIContent("You should select an asset inside your project folder!"));
                } else {
                    folderTreeCache.LoadFolderTree(path);
                    _initialized = false;
                }

            }

            if (GUILayout.Button(new GUIContent("Save Folder Tree", "Saves the Folder Tree in the Assets Folder"))) {
                string path = EditorUtility.SaveFilePanelInProject("Save Folder Tree", "Folder Tree", "asset", "", FolderManager.SavesFolderPath());
                if (!string.IsNullOrEmpty(path))
                    folderTreeCache.SaveFolderTree(path);
                _initialized = false;
            }

            if (GUILayout.Button(new GUIContent("Generate Folder Tree", "Generates the Folder Tree")))
            {
                FolderManager.Generate(folderTreeCache.FolderTree);
            }


        }

        private void ShowFolderStructureWindow() {
            FolderManagerGUI.Title("Created Trees");
            EditorGUILayout.Space();
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, "HelpBox");

            GUILayout.EndScrollView();
        }
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
