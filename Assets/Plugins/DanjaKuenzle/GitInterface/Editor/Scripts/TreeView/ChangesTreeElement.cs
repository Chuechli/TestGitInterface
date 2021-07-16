using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GitInterface {
    /// <summary>
    /// The Changes Tree Element class contains the tree element of all changes records for the Changes tab.
    /// </summary>
    [Serializable]
    public class ChangesTreeElement : TreeElement {
        private bool enabled;
        public ChangeRecord.State state;
        public string filePath;
        /// <value>Gets & Sets the value of Enabled.</value>
        public bool Enabled {
            get => enabled;
            set {
                if (enabled != value) {
                    SetEnabledForAllChildren(this, value);
                }
                enabled = value;
            }
        }
        /// <summary>
        /// Constructor to initialize the Changes Tree Element.
        /// </summary>
        /// <param name="state">The ChangeRecord.State state.</param>
        /// <param name="filePath">The string file Path.</param>
        /// <param name="name">The string name.</param>
        /// <param name="depth">The int depth.</param>
        /// <param name="id">The int id.</param>
        public ChangesTreeElement(ChangeRecord.State state, string filePath, string name, int depth, int id) : base(name, depth, id) {
            this.state = state;
            this.filePath = filePath;
            Enabled = true;
        }
        /// <summary>
        /// Constructor to initialize the Changes Tree Element.
        /// </summary>
        /// <param name="name">The string name.</param>
        /// <param name="depth">The int depth.</param>
        /// <param name="id">The int id.</param>
        public ChangesTreeElement(string name, int depth, int id) : base(name, depth, id) {
            Enabled = true;
        }
        /// <summary>
        /// Method Set Enabled For All Children to set enabled for all child elements.
        /// </summary>
        /// <param name="item">The ChangesTreeElement item.</param>
        /// <param name="enabled">The bool enabled.</param>
        private void SetEnabledForAllChildren(ChangesTreeElement item, bool enabled) {
            if (item.hasChildren) {
                foreach (ChangesTreeElement child in item.children) {
                    child.enabled = enabled;
                    SetEnabledForAllChildren(child, enabled);
                }
            }
        }
    }
}
