using Localizational.ReorderableList;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Localizational.Editor {
    internal class CreateLanguageListAdaptor : GenericListAdaptor<LocCultureInfo> {
        public CreateLanguageListAdaptor(List<LocCultureInfo> list, ReorderableListControl.ItemDrawer<LocCultureInfo> itemDrawer, float itemHeight)
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
            return false;
        }
    }
}
