using System;
using System.Collections.Generic;
using UnityEngine;

namespace SD.FolderManagement.Model {
    [Serializable]
    public class TreeElement {
        [NonSerialized] private List<TreeElement> _children;

        [SerializeField] private int _depth;

        [SerializeField] private int _id;

        [SerializeField] private string _name;

        [NonSerialized] private TreeElement _parent;

        public TreeElement() { }

        public TreeElement(string name, int depth, int id) {
            _name = name;
            _id = id;
            _depth = depth;
        }

        #region Properties

        public int Depth {
            get { return _depth; }
            set { _depth = value; }
        }

        public TreeElement Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        public List<TreeElement> Children {
            get { return _children; }
            set { _children = value; }
        }

        public bool HasChildren {
            get { return _children != null && _children.Count > 0; }
        }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public int Id {
            get { return _id; }
            set { _id = value; }
        }

        #endregion
    }
}