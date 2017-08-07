using UnityEngine;
using System.Collections;
using Localizational.ReorderableList;
using UnityEditor;

namespace Localizational.Editor {

    internal class LocCultureInfoMenuControl : ReorderableListControl {
        static readonly GUIContent commandTranslate = new GUIContent("Translate");
        static readonly GUIContent commandUpdate = new GUIContent("Update");
        static readonly GUIContent commandExport = new GUIContent("Export");

        public LocCultureInfoMenuControl() : base(ReorderableListFlags.HideAddButton | ReorderableListFlags.DisableContextMenu) { }

        //Nothing in here is used ATM, the context menu is disabled
        protected override void AddItemsToMenu(GenericMenu menu, int itemIndex, IReorderableListAdaptor adaptor) {
            menu.AddItem(commandTranslate, false, defaultContextHandler, commandTranslate);
            menu.AddItem(commandUpdate, false, defaultContextHandler, commandUpdate);
            menu.AddItem(commandExport, false, defaultContextHandler, commandExport);
        }

        protected override bool HandleCommand(string commandName, int itemIndex, IReorderableListAdaptor adaptor) {
            LocCultureInfoListAdaptor smartAdaptor = adaptor as LocCultureInfoListAdaptor;

            if (smartAdaptor == null)
                return false;

            switch (commandName) {
                case "Translate":
                    OnTranslateClick(smartAdaptor.GetCultureInfo(itemIndex));
                    return true;
                case "Update":
                    OnUpdateClick(smartAdaptor.GetCultureInfo(itemIndex));
                    return true;
                case "Export":
                    OnExportClick(smartAdaptor.GetCultureInfo(itemIndex));
                    return true;
            }

            return false;
        }

        protected override void OnItemInserted(ItemInsertedEventArgs args) {
            //base.OnItemInserted(args);
        }

        protected override void OnItemRemoving(ItemRemovingEventArgs args) {
            LocCultureInfoListAdaptor smartAdaptor = args.adaptor as LocCultureInfoListAdaptor;

            if (smartAdaptor == null) {
                return;
            }

            LocCultureInfo info = smartAdaptor.GetCultureInfo(args.itemIndex);

            if (EditorUtility.DisplayDialog("Delete " + info.EnglishName + "?",
                "Are you sure you want to delete " + info.EnglishName + " and all of its content from the project? You cannot undo this action.",
                "Yes, delete it.", "Cancel")) {
                //LanguageHelper.DeleteLanguage(info);
                base.OnItemRemoving(args);
            } else {
                args.Cancel = true;
            }
        }

        public void OnTranslateClick(LocCultureInfo info) {
            Debug.Log("Translate: " + info.EnglishName);
        }

        public void OnUpdateClick(LocCultureInfo info) {
            Debug.Log("Update: " + info.EnglishName);
        }

        public void OnExportClick(LocCultureInfo info) {
            Debug.Log("Export: " + info.EnglishName);
        }
    }
}
