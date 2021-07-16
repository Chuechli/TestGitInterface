using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GitInterface {
    /// <summary>
    /// The Tree Helper class contains utility methods to change, parse and initialize the data for the tree (view).
    /// </summary>
    public class TreeHelper {
        [SerializeField] TreeViewState m_TreeViewState;
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        private ChangesTreeView treeView;

        internal ChangesTreeView TreeView { get => treeView; }

        public TreeHelper(List<ChangeRecord> gitChangeRecords) {
            InitializedTreeView(gitChangeRecords);
        }


        private void InitializedTreeView(List<ChangeRecord> gitChangeRecords) {
            if (m_TreeViewState == null) {
                m_TreeViewState = new TreeViewState();
            }
            bool firstInit = m_MultiColumnHeaderState == null;
            var headerState = ChangesTreeView.CreateDefaultMultiColumnHeaderState(100f);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
            m_MultiColumnHeaderState = headerState;

            var multiColumnHeader = new MultiColumnHeader(headerState);
            if (firstInit)
                multiColumnHeader.ResizeToFit();

            var treeModel = new TreeModel<ChangesTreeElement>(GetData(gitChangeRecords));

            treeView = new ChangesTreeView(m_TreeViewState, multiColumnHeader, treeModel);

            TreeView.Reload();
        }

        public void SetNewData(List<ChangeRecord> gitChangeRecords) {
            treeView.treeModel.SetData(GetData(gitChangeRecords));
            TreeView.Reload();
        }

        private IList<ChangesTreeElement> GetData(List<ChangeRecord> gitChangeRecords) {
            List<ChangesTreeElement> treeElements = new List<ChangesTreeElement>();

            var root = new ChangesTreeElement("Root", -1, 0);
            treeElements.Add(root);

            List<string> createdPaths = new List<string>();
            Dictionary<string, int> createdPathings = new Dictionary<string, int>();
            int id = 1;
            foreach (var record in gitChangeRecords) {
                bool containedPath = false;
                if (record.FilePath.Contains("/")) {
                    string checkPath = record.FilePath.Substring(0, record.FilePath.LastIndexOf("/") + 1);
                    string checkFileName = record.FilePath.Substring(record.FilePath.LastIndexOf("/") + 1);
                    if (createdPathings.ContainsKey(checkPath)) {
                        containedPath = true;
                        treeElements.Add(new ChangesTreeElement(record.StateObject, record.FilePath, checkFileName, createdPathings[checkPath] + 1, id));
                        id++;
                    }
                }
                if (record.FilePath.Contains("/") && !containedPath) {
                    string[] splits = record.FilePath.Split('/');
                    string pathing = "";
                    int countDepth = 0;
                    foreach (var split in splits) {
                        pathing += split;
                        ChangeRecord.State state = record.StateObject;
                        //
                        if (!string.Equals(split, splits[splits.Length - 1])) {
                            //is folder
                            pathing += "/";
                            if (!(string.Equals(split, splits[splits.Length - 2]) && string.IsNullOrWhiteSpace(splits[splits.Length - 1]))) {
                                //is NOT last folder which has no children so state will always be "modified"
                                state = ChangeRecord.State.Modified;
                            }
                        }

                        if (!createdPathings.ContainsKey(pathing)) {
                            if (!string.IsNullOrWhiteSpace(split)) {
                                treeElements.Add(new ChangesTreeElement(state, pathing, split, countDepth, id));
                                createdPaths.Add(split);
                                createdPathings.Add(pathing, countDepth);
                                id++;
                            }
                        }
                        countDepth++;
                    }
                }
            }
            return treeElements;
        }

        public List<ChangesTreeElement> GetDeepestTickedElements() {
            List<ChangesTreeElement> tickedElements = new List<ChangesTreeElement>();
            foreach (ChangesTreeElement item in treeView.treeModel.GetData()) {
                if (item.Enabled && !item.hasChildren && !string.Equals(item.name, "Root")) {
                    tickedElements.Add(item);
                }
            }
            return tickedElements;
        }

        public List<ChangesTreeElement> GetDeepestElements() {
            List<ChangesTreeElement> deepestElements = new List<ChangesTreeElement>();
            foreach (ChangesTreeElement item in treeView.treeModel.GetData()) {
                if (!item.hasChildren && !string.Equals(item.name, "Root")) {
                    deepestElements.Add(item);
                }
            }
            return deepestElements;
        }

        public void ChangeTickAllTickedElements(bool isTicked) {
            foreach (ChangesTreeElement item in treeView.treeModel.GetData()) {
                item.Enabled = isTicked;
            }
        }

        internal class MultiColumnHeader : UnityEditor.IMGUI.Controls.MultiColumnHeader {
            Mode m_Mode;

            public enum Mode {
                MinimumHeaderWithoutSorting
            }

            public MultiColumnHeader(MultiColumnHeaderState state) : base(state) {
                mode = Mode.MinimumHeaderWithoutSorting;
            }

            public Mode mode {
                get {
                    return m_Mode;
                }
                set {
                    m_Mode = value;
                    switch (m_Mode) {
                        case Mode.MinimumHeaderWithoutSorting:
                            canSort = false;
                            height = DefaultGUI.minimumHeight;
                            break;
                    }
                }
            }
        }
    }
}