using System;
using UnityEditor;
using UnityEngine;

namespace SD.FolderManagement {

    public static class FolderManagerGUI {


        static FolderManagerGUI() {
            InitStyles();
        }

        #region Custom Styles

        public static GUIStyle defaultTitleStyle { get; private set; }

        public static GUIStyle defaultContainerStyle { get; private set; }

        public static GUIStyle defaultAddButtonStyle { get; private set; }

        public static GUIStyle defaultRemoveButtonStyle { get; private set; }

        private static void InitStyles() {
            defaultTitleStyle = new GUIStyle();
            defaultTitleStyle.border = new RectOffset(2, 2, 2, 1);
            defaultTitleStyle.margin = new RectOffset(5, 5, 5, 0);
            defaultTitleStyle.padding = new RectOffset(5, 5, 0, 0);
            defaultTitleStyle.alignment = TextAnchor.MiddleLeft;
            defaultTitleStyle.normal.background = FolderManagerResources.texTitleBackground;
            defaultTitleStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.8f, 0.8f, 0.8f)
                : new Color(0.2f, 0.2f, 0.2f);

            defaultContainerStyle = new GUIStyle();
            defaultContainerStyle.border = new RectOffset(2, 2, 1, 2);
            defaultContainerStyle.margin = new RectOffset(5, 5, 5, 5);
            defaultContainerStyle.padding = new RectOffset(1, 1, 2, 2);
            defaultContainerStyle.normal.background = FolderManagerResources.texContainerBackground;

            defaultAddButtonStyle = new GUIStyle();
            defaultAddButtonStyle.fixedWidth = 30;
            defaultAddButtonStyle.fixedHeight = 16;
            defaultAddButtonStyle.normal.background = FolderManagerResources.texAddButton;
            defaultAddButtonStyle.active.background = FolderManagerResources.texAddButtonActive;

            defaultRemoveButtonStyle = new GUIStyle();
            defaultRemoveButtonStyle.fixedWidth = 27;
            defaultRemoveButtonStyle.active.background = FolderManagerResources.CreatePixelTexture("Dark Pixel (List GUI)", new Color32(18, 18, 18, 255));
            defaultRemoveButtonStyle.imagePosition = ImagePosition.ImageOnly;
            defaultRemoveButtonStyle.alignment = TextAnchor.MiddleCenter;

        }

        #endregion

        #region Title Control

        private static GUIContent s_Temp = new GUIContent();

        public static void Title(GUIContent title) {
            Rect position = GUILayoutUtility.GetRect(title, defaultTitleStyle);
            position.height += 6;
            Title(position, title);
        }


        public static void Title(string title) {
            s_Temp.text = title;
            Title(s_Temp);
        }

        public static void Title(Rect position, GUIContent title) {
            if (Event.current.type == EventType.Repaint)
                defaultTitleStyle.Draw(position, title, false, false, false, false);
        }

        public static void Title(Rect position, string text) {
            s_Temp.text = text;
            Title(position, s_Temp);
        }

        #endregion

        #region Seperator 

        private static GUIStyle _seperator;

        public static void Seperator() {
            SetupSeperator();
            GUILayout.Box(GUIContent.none, _seperator, new GUILayoutOption[] { GUILayout.Height(1) });
        }

        public static void Seperator(Rect rect) {
            SetupSeperator();
            GUI.Box(new Rect(rect.x, rect.y, rect.width, 1), GUIContent.none, _seperator);
        }

        private static void SetupSeperator() {
            if (_seperator == null) {
                _seperator = new GUIStyle
                {
                    normal = {background = CreatePixelTexture(1, new Color(0.6f, 0.6f, 0.6f))},
                    stretchWidth = true,
                    margin = new RectOffset(0, 0, 7, 7)
                };
            }
        }

        #endregion

        public static Texture2D CreatePixelTexture(string name, Color color) {
            var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.name = name;
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.filterMode = FilterMode.Point;
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public static Texture2D CreatePixelTexture(int pxSize, Color col) {
            var tex = new Texture2D(pxSize, pxSize);
            for (int x = 0; x < pxSize; x++)
            for (int y = 0; y < pxSize; y++)
                tex.SetPixel(x, y, col);
            tex.Apply();
            return tex;
        }

    }
}