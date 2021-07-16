using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GitInterface {
    /// <summary>
    /// The Branches Tree View class contains the tree view of all branches records for the Branches tab.
    /// </summary>
    public class BranchesTreeView : TreeView {
        private List<BranchRecord> records;
        /// <summary>
        /// Constructor to initialize the Tree View object.
        /// </summary>
        /// <param name="treeViewState">The TreeViewState tree View State.</param>
        /// <param name="records">The list of BranchRecord records.</param>
        public BranchesTreeView(TreeViewState treeViewState, List<BranchRecord> records) : base(treeViewState) {
            this.records = records;
            Reload();
        }
        /// <summary>
        /// Method to build the root structure of the tree view.
        /// </summary>
        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>();
            allItems.Add(new TreeViewItem { id = 1, depth = 0, displayName = "Local branches" });
            int id = 2;
            foreach (var item in records) {
                if (BranchRecord.Type.Local == item.TypeObject) {
                    allItems.Add(new TreeViewItem { id = id, depth = 1, displayName = item.BranchName });
                    id++;
                }
            }
            allItems.Add(new TreeViewItem { id = id, depth = 0, displayName = "Remote branches" });
            id++;
            foreach (var item in records) {
                if (BranchRecord.Type.Remote == item.TypeObject) {
                    allItems.Add(new TreeViewItem { id = id, depth = 1, displayName = item.BranchName });
                    id++;
                }
            }

            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);

            // Return root of the tree
            return root;
        }
    }
}
