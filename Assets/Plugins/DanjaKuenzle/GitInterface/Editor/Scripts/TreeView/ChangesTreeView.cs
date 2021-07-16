using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace GitInterface {
    /// <summary>
    /// The Changes Tree View class contains the tree view of all changes records for the Changes tab.
    /// </summary>
    internal class ChangesTreeView : TreeViewWithTreeModel<ChangesTreeElement> {
        private List<ChangeRecord> files;

        const float kRowHeights = 20f;
        const float kToggleWidth = 18f;
        public bool showControls = true;

        enum Columns {
            Name, //fileName
            State,
            FilePath,
        }
        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result) {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0) {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null) {
                    for (int i = current.children.Count - 1; i >= 0; i--) {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        public ChangesTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<ChangesTreeElement> model) : base(state, multicolumnHeader, model) {
            // Custom setup
            rowHeight = kRowHeights;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = kToggleWidth;

            Reload();
        }

        /// <summary>
        /// Note we We only build the visible rows, only the backend has the full tree information.
        /// The treeview only creates info for the row list.
        /// </summary>
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
            var rows = base.BuildRows(root);
            return rows;
        }
        protected override void RowGUI(RowGUIArgs args) {
            var item = (TreeViewItem<ChangesTreeElement>) args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i) {
                CellGUI(args.GetCellRect(i), item, (Columns) args.GetColumn(i), ref args);
            }
        }
        void CellGUI(Rect cellRect, TreeViewItem<ChangesTreeElement> item, Columns column, ref RowGUIArgs args) {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column) {
                case Columns.Name: {
                        // Do toggle
                        Rect toggleRect = cellRect;
                        toggleRect.x += GetContentIndent(item);
                        toggleRect.width = kToggleWidth;
                        if (toggleRect.xMax < cellRect.xMax)
                            item.data.Enabled = EditorGUI.Toggle(toggleRect, item.data.Enabled); // hide when outside cell rect

                        // Default icon and label
                        args.rowRect = cellRect;
                        base.RowGUI(args);
                    }
                    break;
                case Columns.State: {
                        EditorGUI.LabelField(cellRect, item.data.state.ToString());
                    }
                    break;

                case Columns.FilePath: {
                        EditorGUI.LabelField(cellRect, item.data.filePath);
                    }
                    break;
            }
        }

        /// <summary>
        /// Misc
        /// </summary>
        protected override bool CanMultiSelect(TreeViewItem item) {
            return false;
        }
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth) {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("File Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 70,
                    minWidth = 50,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("State"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 20,
                    minWidth = 10,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("File Path"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 70,
                    autoResize = true,
                    allowToggleVisibility = false
                }
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(Columns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }
    }
}
