using UnityEngine;
using System.Collections.Generic;
using Localizational.ReorderableList;

namespace Localizational.Editor {

    internal class LocCultureInfoListAdaptor : GenericListAdaptor<LocCultureInfo> {
        public LocCultureInfoListAdaptor(List<LocCultureInfo> list, ReorderableListControl.ItemDrawer<LocCultureInfo> itemDrawer, float itemHeight)
           : base(list, itemDrawer, itemHeight) {
        }

        public override void DrawItem(Rect position, int index) {
            base.DrawItem(position, index);
        }

        public LocCultureInfo GetCultureInfo(int itemIndex) {
            return this[itemIndex];
        }

        public override bool CanDrag(int index) {
            return false;
        }

        public override bool CanRemove(int index) {
            return !IsRoot(GetCultureInfo(index));
        }

        private bool IsRoot(LocCultureInfo info) {
            return info.EnglishName == "ROOT";
        }

    }
}