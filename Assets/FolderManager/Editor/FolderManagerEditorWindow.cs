using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SD.FolderManagement;
using SD.FolderManagement.Model;
using SD.FolderManagement.Utility;
using UnityEditor.IMGUI.Controls;

namespace SD.FolderManagement.Editor {

    internal class FolderManagerEditorWindow : EditorWindow {

        private static FolderManagerEditorWindow _editor;
        private Vector2 _createScrollPosition;
        private Vector2 _scrollPosition;

        [NonSerialized]
        private bool _initialized;

        [SerializeField]
        private TreeViewState _treeViewState;

        [SerializeField]
        private MultiColumnHeaderState _multiColumnHeaderState;
        private SearchField _searchField;
        private FolderTreeView _treeView;
        private FolderTreeAsset _asset;

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
            FolderManager.Initialize();
            return _editor;
        }

        #region Initialization

        private void OnEnable() {
            _editor = this;
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        private void OnDestroy() {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView) {
            DrawGUI();
        }

        private void DrawGUI() {
            SceneView.lastActiveSceneView.Repaint();
        }

        private void Initialize() {
            if (!_initialized) {
                if (_asset == null) return;
                if (_treeViewState == null) {
                    _treeViewState = new TreeViewState();
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
                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;
                _initialized = true;
            }
        }

        private IList<FolderTreeElement> GetData() {
            if (_asset != null && _asset.TreeElements != null && _asset.TreeElements.Count > 0) {
                return _asset.TreeElements;
            } else {
                return null;
            }
            //  return new List<FolderTreeElement>() { new FolderTreeElement("NAME", -1, 1), new FolderTreeElement("Test", 0, 2), new FolderTreeElement("DOD", 0, 3) };
        }
        #endregion

        private void OnSelectionChange() {
            if (!_initialized) {
                return;
            }
            var treeAsset = Selection.activeObject as FolderTreeAsset;

            if (treeAsset != null && treeAsset != _asset) {
                _asset = treeAsset;
                _treeView.TreeModel.SetData(GetData());
                _treeView.Reload();
            }
        }

        private void SetTreeAsset(FolderTreeAsset treeAsset) {
            _asset = treeAsset;
            _initialized = false;
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
            Initialize();
            if (WindowUtility.ShouldShowWindow()) {
                FolderManagerGUI.Title("FolderManager");
                EditorGUILayout.Space();

                ShowContextSettings();
                ShowFolderStructureWindow();

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

            if (_asset != null) {
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
            if (_asset == null) {
                //      EditorUtility.DisplayDialog("Select Texture", "You must select a texture first!", "OK");
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
            if (GUI.Button(newPosition, "Create new tree")) {
                //     object userData;
                // EditorUtility.DisplayCustomMenu(new Rect(20,20,50,50),"Box", 0, Callback, userData));
            }
            newPosition.y += controlHeight;

            if (GUI.Button(newPosition, "Load Tree")) {
                string path = EditorUtility.OpenFilePanel("", "", "asset");
                if (path.Length != 0) {
                    WWW www = new WWW("file:///" + path);
                    Debug.Log(www.text);
                }
            }
            newPosition.y += controlHeight;
            Rect windowRect = new Rect(100, 100, 200, 200);
            BeginWindows();

            // All GUI.Window or GUILayout.Window must come inside here
            windowRect = GUILayout.Window(1, windowRect, DoWindow, "Hi There");

            EndWindows();
            if (GUI.Button(newPosition, "Load Tree")) {
              
            }
            

        }

        private void DoWindow(int unusedWindowID) {
            GUILayout.Button("Hi");
            GUI.DragWindow();
        }

        private void Callback(object userData, string[] options, int selected) {
            throw new NotImplementedException();
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
