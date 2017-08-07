using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Localizational;
using Localizational.ReorderableList;

namespace Localizational.Editor {

    public class LocalizationalEditorWindow : EditorWindow {


        [SerializeField]
        LocCultureInfoCollection _allCultures = null;

        [SerializeField]
        LocCultureInfoCollection _availableCultures = null;

        [SerializeField]
        LocCultureInfoCollection _nonAvailableCultures = null;

        private CreateLanguageMenuControl createListContextMenu;
        private CreateLanguageListAdaptor createListAdaptor;
        private LocCultureInfoMenuControl languageListContextMenu;
        private LocCultureInfoListAdaptor languageListAdaptor;
        SettingsMenuControl settingsContextMenu;
        SettingsListAdaptor settingsAdaptor;

        [SerializeField]
        private Vector2 _scrollPosition = Vector2.zero;

        [SerializeField]
        private Vector2 _createScrollPosition = Vector2.zero;

        [SerializeField]
        private bool _isInitialized = false;

        [SerializeField]
        List<string> settingsList = new List<string>();

        private static LocalizationalEditorWindow _editor;

        public static LocalizationalEditorWindow Editor {
            get {
                GetEditor();
                return _editor;
            }
        }

        private static void GetEditor() {
            if (_editor == null) {
                OpenLocalizational();
            }
        }

        [MenuItem("Tool/Localizational")]
        private static LocalizationalEditorWindow OpenLocalizational() {
            _editor = GetWindow<LocalizationalEditorWindow>();
            _editor.minSize = new Vector2(800, 600);

            LocalizationalWorkSpace.Initialize();
            _editor.titleContent = new GUIContent("Localizational", "Make Translating easy!");

            return _editor;
        }

        private void Initialize() {


            if (_availableCultures == null) {
                _allCultures = LocCultureXMLHelper.Deserialize(LocalizationalWorkSpace.CultureInfoCollectionFilePath());

                if (_allCultures.Version != LocCultureInfoCollection.LATEST_VERSION) {

                    LocalizationalWorkSpace.GenerateCultureInfoCollection(_allCultures);

                    _allCultures = LocCultureXMLHelper.Deserialize(LocalizationalWorkSpace.CultureInfoCollectionFilePath());
                }
                InitializeCultureCollections();
            }

            settingsList.Clear();
            settingsList.Add("SETTINGS");
            settingsList.Add("AUTOTRANSLATE");

            _isInitialized = true;
            GUIUtility.keyboardControl = 0;
        }

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

        public void InitializeCultureCollections(bool reloadAllCultures = false) {

            if (reloadAllCultures) {
                _allCultures = LocCultureXMLHelper.Deserialize(LocalizationalWorkSpace.CultureInfoCollectionFilePath());
            }

            _availableCultures = LanguageHelper.CheckAndSaveAvailableLanguages(_allCultures);
            _nonAvailableCultures = LanguageHelper.GetNonAvailableLanguages(_allCultures);

            _availableCultures.CultureInfos.Sort((a, b) => string.Compare(a.EnglishName, b.EnglishName, StringComparison.Ordinal));
            _nonAvailableCultures.CultureInfos.Sort((a, b) => string.Compare(a.EnglishName, b.EnglishName, StringComparison.Ordinal));

            createListAdaptor = new CreateLanguageListAdaptor(_nonAvailableCultures.CultureInfos, DrawCreateLanguageItem, 15);
            createListContextMenu = new CreateLanguageMenuControl();

            languageListAdaptor = new LocCultureInfoListAdaptor(_availableCultures.CultureInfos, DrawAvailableLanguageItem, 28);
            languageListContextMenu = new LocCultureInfoMenuControl();

            settingsAdaptor = new SettingsListAdaptor(settingsList, DrawSettingsItem, 110);
            settingsContextMenu = new SettingsMenuControl();
        }

        private void OnGUI() {
            GetEditor();

            if (LocWindowUtility.ShouldShowWindow()) {
                if (!_isInitialized) {
                    Initialize();
                }

                if (createListContextMenu == null ||
                    createListAdaptor == null ||
                    settingsContextMenu == null ||
                    settingsAdaptor == null ||
                    languageListAdaptor == null ||
                    languageListContextMenu == null) {
                    InitializeCultureCollections(true);
                }


                //Show settings
                ReorderableListGUI.Title("Smart Localization");
                EditorGUILayout.Space();

                ShowCreateAndSettingsActions();
                ShowCreatedLanguages();
            }
        }
        #region GUI Sections
        private void ShowCreateAndSettingsActions() {
            float maxWidth = position.width * 0.5f;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            Rect positionCheck = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(maxWidth));
            ReorderableListGUI.Title(positionCheck, "Add / Update Languages");
            _createScrollPosition = GUILayout.BeginScrollView(_createScrollPosition, GUILayout.MaxHeight(350), GUILayout.MaxWidth(maxWidth));
            createListContextMenu.Draw(createListAdaptor);
            GUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            positionCheck = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(maxWidth));
            ReorderableListGUI.Title(positionCheck, "Settings");
            settingsContextMenu.Draw(settingsAdaptor);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }



        private string DrawSettingsItem(Rect pos, string label) {
            if (label == "SETTINGS") {
                DrawSettingsActions(pos);
            }
            return label;
        }

        public void DrawSettingsActions(Rect pos) {
            float fullWindowWidth = pos.width + 30;
            float controlHeight = pos.height * 0.16f;
            Rect newPosition = pos;
            newPosition.width = fullWindowWidth;
            newPosition.height = controlHeight;
            GUI.Label(newPosition, "Settings", EditorStyles.boldLabel);
            if (GUI.Button(newPosition, "Create new culture")) {
                //    CreateLanguageWindow.ShowWindow(this);
            }
            newPosition.y += controlHeight;

            if (GUI.Button(newPosition, "Export All Languages")) {
                //   BulkUpdateWindow.ShowWindow(BulkUpdateWindow.BulkUpdateMethod.Export, this);
            }
            newPosition.y += controlHeight;

            if (GUI.Button(newPosition, "Import All Languages")) {
                //   BulkUpdateWindow.ShowWindow(BulkUpdateWindow.BulkUpdateMethod.Import, this);
            }
        }

        private LocCultureInfo DrawCreateLanguageItem(Rect pos, LocCultureInfo info) {
            float fullWindowWidth = pos.width + 30;
            Rect newPosition = pos;
            newPosition.width = fullWindowWidth * 0.5f;
            GUI.Label(newPosition, info.EnglishName + " - " + info.LanguageCode);

            float buttonWidth = fullWindowWidth * 0.2f;
            newPosition.width = buttonWidth;
            newPosition.x = fullWindowWidth - newPosition.width;

            if (GUI.Button(newPosition, "Create")) {
                OnCreateLanguageClick(info);
            }
            newPosition.x -= buttonWidth;
            //  if (GUI.Button(newPosition, "Import")) {
            //   LanguageImportWindow.ShowWindow(info, OnInitializeCollectionsCallback);
            //  }
            return info;
        }

        public LocCultureInfo DrawAvailableLanguageItem(Rect pos, LocCultureInfo info) {
            if (info.EnglishName != "ROOT") {
                float fullWindowWidth = pos.width;
                Rect newPosition = pos;
                newPosition.width = fullWindowWidth * 0.4f;
                GUI.Label(pos, info.EnglishName + " - " + info.LanguageCode);

                float buttonWidth = fullWindowWidth * 0.2f;
                buttonWidth = Mathf.Clamp(buttonWidth, 70, 120);

                newPosition.width = buttonWidth;
                newPosition.x = fullWindowWidth - buttonWidth;

                if (GUI.Button(newPosition, "Update")) {
                    //      LanguageUpdateWindow.ShowWindow(info, this);
                }
                newPosition.x -= buttonWidth;
                if (GUI.Button(newPosition, "Export")) {
                    //     LanguageExportWindow.ShowWindow(info);
                }
                newPosition.x -= buttonWidth;
                if (GUI.Button(newPosition, "Translate")) {
                    //      OnTranslateButtonClick(info);
                }
            } else {
                pos.width += 28;
                if (GUI.Button(position, "Edit Root Language File")) {
                    //   OnRootEditClick();
                }
            }
            return info;
        }

        private void ShowCreatedLanguages() {
            if (languageListContextMenu == null || languageListAdaptor == null) {
                this.Close();
            }

            ReorderableListGUI.Title("Created Languages");
            EditorGUILayout.Space();
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            languageListContextMenu.Draw(languageListAdaptor);
            GUILayout.EndScrollView();
        }


        #region Event Handlers

        private void OnCreateLanguageClick(LocCultureInfo info) {
            LocCultureInfo chosenCulture = _allCultures.FindCulture(info);
            Debug.Log(chosenCulture);
            if (chosenCulture == null) {
                Debug.LogError("The language: " + info.EnglishName + " could not be created");
                return;
            }
            LanguageHelper.CreateNewLanguage(chosenCulture.LanguageCode);
            InitializeCultureCollections();
        }
        #endregion
    }
    #endregion


}
