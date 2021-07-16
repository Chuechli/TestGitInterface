using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GitInterface {
    /// <summary>
    /// The Git Interface Window class contains the whole window for the Git Interface plugin.
    /// Every tab is built in this class. (Settings, About, Changes, Branches, Initialize, History)
    /// </summary>
    public class GitInterfaceWindow : EditorWindow {
        public static GitInterfaceWindow instance;
        private int tabIndex;
        private int tabIndexAbout;
        private Vector2 scrollPos;
        private GUISkin skin;

        //Settings
        private string userName;
        private string email;
        private string repoUrl;
        private string gitInstallFolder = "C:/Program Files/Git/cmd";
        private string chosenDiffTool;
        private bool createIgnoreFile;
        private int selectedDiffTool = 0;

        //Changes
        int changedFiles;
        string commitSummary;
        string commitDescription;
        TreeHelper treeHelper;
        Vector2 scrollPosChanges;
        string[] gitDiffOptions;

        [SerializeField] TreeViewState branchesTreeViewState;
        BranchesTreeView branchesTreeView;

        //History
        Vector2 historyScroll;
        //Branches
        Vector2 branchesScroll;

        //About
        private Vector2 scrollPosAboutReadMe;
        private Vector2 scrollPosAboutVersions;

        /// <summary>
        /// Method Show Git Interface Window show the whole window 'GitInterface' with a logo
        /// </summary>
        public static void ShowGitInterfaceWindow() {
            var editorAsm = typeof(Editor).Assembly;
            var inspWndType = editorAsm.GetType("UnityEditor.InspectorWindow");
            instance = (GitInterfaceWindow) EditorWindow.GetWindow<GitInterfaceWindow>(inspWndType);
            GUIContent title = new GUIContent(TextConstants.TOOL_NAME);
            Texture2D logo = Resources.Load("UI/GitLogo") as Texture2D;
            ;
            title.image = logo;
            instance.titleContent = title;
        }
        /// <summary>
        /// Method On Enable contains are procedures that must be initalized first when the window is being enabled based on the plugin's state.
        /// Such as reading and fetching the serialized user data (XML) and initalizing the views/tabs (Changes Tree View, branches tree view etc.) 
        /// </summary>
        private void OnEnable() {
            skin = (GUISkin) Resources.Load("UI/" + TextConstants.SKIN_NAME);
            if (PluginStates.SettingsSaved) {
                LoadSettings();
                name = GitData.Instance.serializedData.UserName;
                email = GitData.Instance.serializedData.Email;
                repoUrl = GitData.Instance.serializedData.RepoUrl;
                gitInstallFolder = GitData.Instance.serializedData.GitInstallFolder;
                chosenDiffTool = GitData.Instance.serializedData.ChosenDiffTool;
                gitDiffOptions = GitData.Instance.serializedData.GitDiffOptions;
                selectedDiffTool = FindCurrentDiffToolInArray();
            } else {
                gitDiffOptions = StringUtility.ParseDiffTools(GitConnector.GetAvailableDiffTools());
                selectedDiffTool = FindCurrentDiffToolInArray();
            }
            if (PluginStates.RepositoryInitialized) {
                InitializeViews();
            }
        }
        /// <summary>
        /// Method Find Current Diff Tool In Array finds the currently chosen diff tool in the diff tool options
        /// </summary>
        /// <returns>
        /// Int index of current selected diff tool.
        /// </returns>
        private int FindCurrentDiffToolInArray() {
            for (int i = 0; i < gitDiffOptions.Length; i++) {
                if (string.Equals(gitDiffOptions[i], chosenDiffTool)) { //also handles null
                    return i;
                }
            }
            return 0;
        }
        /// <summary>
        /// Method On GUI draws the tabs
        /// </summary>
        private void OnGUI() {
            DrawTabs();
        }
        void Callback(object obj) {
            Debug.Log("Selected: " + obj);
        }
        /// <summary>
        /// Method Draw Tabs draws all the tabs depending on the plugin's state
        /// Settings are not saved or Repository is not yet Set Up -> Settings, About
        /// Settings are saved and Repository is Set Up but repository is not yet initialized -> Initialize, Settings, About
        /// Settings are saved, Repository is Set Up and initialized -> Changes, History, Branches, Settings, About
        /// </summary>
        private void DrawTabs() {
            using (new EditorGUILayout.HorizontalScope()) {
                if (!PluginStates.SettingsSaved || !PluginStates.RepositorySetUp) {
                    tabIndex = GUILayout.Toolbar(tabIndex, texts: new[] { TextConstants.TAB_NAME_SETTINGS, TextConstants.TAB_NAME_ABOOUT });
                } else {
                    if (!PluginStates.RepositoryInitialized) {
                        tabIndex = GUILayout.Toolbar(tabIndex, texts: new[] { TextConstants.TAB_NAME_INITIALIZE, TextConstants.TAB_NAME_SETTINGS, TextConstants.TAB_NAME_ABOOUT });
                    } else {
                        tabIndex = GUILayout.Toolbar(tabIndex, texts: new[] { TextConstants.TAB_NAME_CHANGES, TextConstants.TAB_NAME_HISTORY, TextConstants.TAB_NAME_BRANCHES, TextConstants.TAB_NAME_SETTINGS, TextConstants.TAB_NAME_ABOOUT });
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.Label(new GUIContent("                           ")); //needs empty space to get last rect
            }
            if (tabIndex == 0) {
                if (!PluginStates.SettingsSaved || !PluginStates.RepositorySetUp) {
                    DrawTabSettings();
                } else {
                    if (!PluginStates.RepositoryInitialized) {
                        DrawTabInitialize();
                    } else {
                        DrawTabChanges();
                    }
                }
            } else if (tabIndex == 1) {
                if (!PluginStates.SettingsSaved || !PluginStates.RepositorySetUp) {
                    DrawTabAbout();
                } else {
                    if (!PluginStates.RepositoryInitialized) {
                        DrawTabSettings();
                    } else {
                        DrawTabHistory();
                    }
                }
            } else if (tabIndex == 2) {
                if (!PluginStates.RepositoryInitialized) {
                    DrawTabAbout();
                } else {
                    DrawTabBranches();
                }
            } else if (tabIndex == 3) {
                DrawTabSettings();
            } else if (tabIndex == 4) {
                DrawTabAbout();
            }
        }
        /// <summary>
        /// Method Get Changes gets all parsed change record records from the string utility
        /// </summary>
        private List<ChangeRecord> GetChanges() {
            List<ChangeRecord> gitChangeRecords = StringUtility.ParseGitChangesOutput(GitConnector.GetChanges());
            changedFiles = gitChangeRecords.Count;
            return gitChangeRecords;
        }
        /// <summary>
        /// Method Draw Tab Changes draws the whole tab 'Changes'
        /// </summary>
        private void DrawTabChanges() {
            if (!string.IsNullOrWhiteSpace(GitConnector.ErrorAndExceptionsChanges)) {
                EditorGUILayout.HelpBox("Getting changes from Git not successful! \nThere were warnings / errors:\n" + GitConnector.ErrorAndExceptionsChanges, MessageType.Warning);
            }
            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button(new GUIContent("All", "Tick all elements"))) {
                    treeHelper.ChangeTickAllTickedElements(true);
                }
                if (GUILayout.Button(new GUIContent("None", "Untick all elements"))) {
                    treeHelper.ChangeTickAllTickedElements(false);
                }
                if (PluginStates.DiffToolExists) {
                    ChangesTreeElement changesTreeItem = null;
                    if (!treeHelper.TreeView.HasSelection()
                        || !treeHelper.GetDeepestElements().Contains(treeHelper.TreeView.treeModel.Find(treeHelper.TreeView.GetSelection()[0]))
                        || treeHelper.TreeView.treeModel.Find(treeHelper.TreeView.GetSelection()[0]).state != ChangeRecord.State.Modified) {
                        GUI.enabled = false;
                    } else {
                        int selectedItem = treeHelper.TreeView.GetSelection()[0];
                        changesTreeItem = treeHelper.TreeView.treeModel.Find(selectedItem);
                    }
                    if (GUILayout.Button(new GUIContent("Compare File", "Compare the local version of the selected file with the remote version"))) {
                        GitConnector.DiffFile(changesTreeItem);
                    }
                    GUI.enabled = true;
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(changedFiles + " changed Files", GUILayout.Width(EditorGUIUtility.labelWidth));
            }

            //tree files
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosChanges)) {
                scrollPosChanges = scrollView.scrollPosition;
                treeHelper.TreeView.OnGUI(new Rect(10, 5, position.width - 20, position.height - 275));
            }
            BottomToolBarChanges();

            bool enablePushToMasterButton = true;
            EditorGUILayout.LabelField("Commit summary");
            commitSummary = EditorGUILayout.TextField(commitSummary);
            if (string.IsNullOrWhiteSpace(commitSummary)) {
                EditorGUILayout.HelpBox(string.Format(TextConstants.ERROR_MESSAGE_EMPTY, "Commit Summary"), MessageType.Warning);
                enablePushToMasterButton = false;
            }
            EditorGUILayout.LabelField("Commit description");
            commitDescription = EditorGUILayout.TextArea(commitDescription, GUILayout.Height(EditorGUIUtility.singleLineHeight * 4));
            using (new EditorGUILayout.HorizontalScope()) {
                GUI.enabled = enablePushToMasterButton && treeHelper.GetDeepestTickedElements().Count > 0;
                if (GUILayout.Button(new GUIContent("Commit", "Commit all selected files with the summary (and description"), GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f))) {
                    GitConnector.CommitSelectedFiles(treeHelper.GetDeepestTickedElements(), commitSummary, commitDescription);
                    commitSummary = "";
                    commitDescription = "";
                    treeHelper.SetNewData(GetChanges());
                }
            }
        }
        /// <summary>
        /// Method Button Tool Bar Changes draw all buttons at the bottom for tab 'Changes'
        /// </summary>
        void BottomToolBarChanges() {
            using (new EditorGUILayout.HorizontalScope()) {

                if (GUILayout.Button("Expand All")) {
                    treeHelper.TreeView.ExpandAll();
                }
                if (GUILayout.Button("Collapse All")) {
                    treeHelper.TreeView.CollapseAll();
                }
                if (GUILayout.Button("Reload Changes")) {
                    treeHelper.SetNewData(GetChanges());
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Selected files: " + treeHelper.GetDeepestTickedElements().Count, GUILayout.Width(EditorGUIUtility.labelWidth));
            }
        }
        /// <summary>
        /// Method Draw Tab History draws the whole tab 'History'
        /// </summary>
        private void DrawTabHistory() {
            string gitHistory = GitConnector.GetHistory();
            string errorAndExceptions = GitConnector.GetErrorAndExceptionMessageFromGit();
            if (!string.IsNullOrWhiteSpace(errorAndExceptions)) {
                EditorGUILayout.HelpBox(string.Format(TextConstants.ERROR_MESSAGE_LONG, TextConstants.TAB_NAME_HISTORY.ToLower(), errorAndExceptions), MessageType.Warning);
            }
            using (var scrollView = new EditorGUILayout.ScrollViewScope(historyScroll)) {
                historyScroll = scrollView.scrollPosition;
                foreach (HistoryRecord item in StringUtility.ParseGitHistoryOutput(gitHistory)) {
                    Rect rect = EditorGUILayout.BeginHorizontal("box");
                    Handles.color = Color.gray;
                    Handles.DrawLine(new Vector2(rect.x + 6, rect.y), new Vector2(rect.x + 6, rect.y + rect.height + 4));
                    EditorGUI.DrawRect(new Rect(new Vector2(rect.position.x + 1, rect.position.y + rect.height / 2 - 5f), new Vector2(10f, 10f)), Color.black);
                    GUILayout.Space(17.0f);
                    Rect vertical = EditorGUILayout.BeginVertical();
                    GUIStyle _titleStyle = new GUIStyle();
                    _titleStyle.fontSize = 13;
                    GUIStyle italicStyle = new GUIStyle();
                    italicStyle.fontStyle = FontStyle.Italic;
                    EditorGUILayout.LabelField(item.Summary, _titleStyle);
                    if (!string.IsNullOrWhiteSpace(item.Description)) {
                        int addSpace = 1;
                        if (item.Description.Contains("\n")) {
                            //check how many line breaks the description has and add it to labelfield (multiply wih singleLineHeight)
                            addSpace = item.Description.Replace("\n", "§").Split('§').Length;
                        }
                        EditorGUILayout.LabelField(item.Description, italicStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight * addSpace));
                    }
                    EditorGUILayout.LabelField(item.Date + "   " + item.Author);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        /// <summary>
        /// Method Draw Tab Branches draws the whole tab 'Branches'
        /// </summary>
        private void DrawTabBranches() {
            EditorGUILayout.HelpBox("Current branch is marked with an asterix '" + "*'", MessageType.Info);
            if (!string.IsNullOrWhiteSpace(GitConnector.ErrorAndExceptionsBranches)) {
                EditorGUILayout.HelpBox(string.Format(TextConstants.ERROR_MESSAGE_LONG, TextConstants.TAB_NAME_BRANCHES.ToLower(), GitConnector.ErrorAndExceptionsBranches), MessageType.Warning);
            }
            if (GUILayout.Button(new GUIContent("Reload", "Reloads all existing branches"), GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f))) {
                branchesTreeView = new BranchesTreeView(branchesTreeViewState, StringUtility.ParseGitBranchesOutput(GitConnector.GetBranchesLocal(), GitConnector.GetBranchesRemote()));
            }
            using (var scrollView = new EditorGUILayout.ScrollViewScope(branchesScroll)) {
                branchesScroll = scrollView.scrollPosition;
                branchesTreeView.OnGUI(new Rect(0, 0f, position.width, position.height));
                branchesTreeView.ExpandAll();
            }
        }
        /// <summary>
        /// Method Draw Tab Initialize draws the whole tab 'Initialize'
        /// </summary>
        public void DrawTabInitialize() {
            using (new EditorGUILayout.VerticalScope()) {
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("To start using Git, initialize your git repository!", skin.label);
                    GUILayout.FlexibleSpace();
                }

                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Push initial Unity project to Git", GUILayout.Height(1.5f * EditorGUIUtility.singleLineHeight))) {
                        GitConnector.InitialPush();
                        if (!string.IsNullOrWhiteSpace(GitConnector.GetErrorAndExceptionMessageFromGit())) {
                            string message = GitConnector.GetErrorAndExceptionMessageFromGit();
                            EditorUtility.DisplayDialog("Push not successful!", string.Format(TextConstants.ERROR_MESSAGE_SHORT, message), TextConstants.OK);
                        } else {
                            PluginStates.RepositoryInitialized = true;
                            InitializeViews();
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("or", GUILayout.Width(EditorGUIUtility.labelWidth / 10));
                    GUILayout.FlexibleSpace();
                }
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Pull remote Unity project data from Git", GUILayout.Height(1.5f * EditorGUIUtility.singleLineHeight))) {
                        GitConnector.InitialPull();
                        if (!string.IsNullOrWhiteSpace(GitConnector.GetErrorAndExceptionMessageFromGit())) {
                            string message = GitConnector.GetErrorAndExceptionMessageFromGit();
                            EditorUtility.DisplayDialog("Pull not successful!", string.Format(TextConstants.ERROR_MESSAGE_SHORT, message), TextConstants.OK);
                        } else {
                            PluginStates.RepositoryInitialized = true;
                            InitializeViews();
                        }

                    }
                    GUILayout.FlexibleSpace();
                }
            }
        }
        /// <summary>
        /// Method Draw Tab Settings draws the whole tab 'Settings'
        /// </summary>
        public void DrawTabSettings() {
            EditorGUILayout.LabelField("Git Configuration", skin.label);
            bool enableButton = true;
            gitInstallFolder = EditorGUILayout.TextField(new GUIContent("Path to Git cmd", "Installation folder e.g. 'C:/Program Files/Git/cmd'"), gitInstallFolder);
            if (string.IsNullOrWhiteSpace(gitInstallFolder)) {
                EditorGUILayout.HelpBox(string.Format(TextConstants.ERROR_MESSAGE_EMPTY, "Path to Git"), MessageType.Warning);
                enableButton = false;
            }
            userName = EditorGUILayout.TextField("Name", userName);
            if (string.IsNullOrWhiteSpace(userName)) {
                EditorGUILayout.HelpBox(string.Format(TextConstants.ERROR_MESSAGE_EMPTY, "Name"), MessageType.Warning);
                enableButton = false;
            }
            email = EditorGUILayout.TextField("Email", email);
            if (string.IsNullOrWhiteSpace(email) || !ValidatorUtility.IsValidEmail(email)) {
                EditorGUILayout.HelpBox(string.Format(TextConstants.ERROR_MESSAGE_INVALID, "Email"), MessageType.Warning);
                enableButton = false;
            }
            using (new EditorGUILayout.HorizontalScope()) {
                selectedDiffTool = EditorGUILayout.Popup(new GUIContent("Choose Diff Tool", "The tool you want to use to show differences in Changes view (must already be installed)"), selectedDiffTool, gitDiffOptions, GUILayout.Width(260f));
                if (GUILayout.Button(new GUIContent("Reload Diff Tools", "Reload all installed diff tool options for git 'difftool --tool -help'"))) {
                    gitDiffOptions = StringUtility.ParseDiffTools(GitConnector.GetAvailableDiffTools());
                    selectedDiffTool = FindCurrentDiffToolInArray();
                }
                GUILayout.FlexibleSpace();
            }


            EditorGUILayout.LabelField("Repository Configuration", skin.label);
            repoUrl = EditorGUILayout.TextField("Remote URL (origin)", repoUrl);
            if (string.IsNullOrWhiteSpace(repoUrl) || !Uri.IsWellFormedUriString(repoUrl, UriKind.Absolute)) {
                EditorGUILayout.HelpBox(string.Format(TextConstants.ERROR_MESSAGE_INVALID, "URL"), MessageType.Warning);
                enableButton = false;
            }
            if (!File.Exists(GitConnector.GIT_IGNORE_FILE_PATH)) {
                createIgnoreFile = true;
            }
            createIgnoreFile = EditorGUILayout.Toggle(new GUIContent("Create .gitIgnore", "Creates default .gitIgnore file for Unity projects if not already exists - must exist for a smooth adding of all intial files of a project"), createIgnoreFile);

            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                GUI.enabled = enableButton;
                if (GUILayout.Button("Check & Set Settings for Git", GUILayout.Height(1.5f * EditorGUIUtility.singleLineHeight))) {
                    chosenDiffTool = gitDiffOptions[selectedDiffTool];
                    bool setSettingsSuccessful = GitConnector.SetSettings(userName, email, repoUrl, gitInstallFolder, chosenDiffTool, createIgnoreFile);
                    string message = GitConnector.GetErrorAndExceptionMessageFromGit();
                    if (setSettingsSuccessful) {
                        bool doSaveSettings = true;
                        if (!string.IsNullOrWhiteSpace(GitConnector.GetErrorAndExceptionMessageFromGit())) {
                            message += "Are you sure you want to save the settings?";
                            if (!EditorUtility.DisplayDialog("Check successful!", "However there were some warnings:\n" + message, "Yes", "No")) {
                                doSaveSettings = false;
                            }
                        }
                        if (doSaveSettings) {
                            string errorMessages = SaveSettings(userName, email, repoUrl, gitInstallFolder, chosenDiffTool, gitDiffOptions);
                            if (string.IsNullOrWhiteSpace(errorMessages)) {
                                EditorUtility.DisplayDialog("Check & save settings successful!", "Settings are saved under '" + TextConstants.GIT_DATA_PATH + "'", TextConstants.OK);
                            } else {
                                EditorUtility.DisplayDialog("Save not successful!", string.Format(TextConstants.ERROR_MESSAGE_SHORT, errorMessages), TextConstants.OK);
                            }
                        }
                    } else {
                        EditorUtility.DisplayDialog("Check was not ok!", string.Format(TextConstants.ERROR_MESSAGE_SHORT, message), TextConstants.OK);
                    }
                }
            }
            if (PluginStates.SettingsSaved) {
                GUI.enabled = true;
            } else {
                GUI.enabled = false;
            }
            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Set Up Repository", "Init, Set Remote, Create gitIgnore, first test commit"), GUILayout.Height(1.5f * EditorGUIUtility.singleLineHeight))) {
                    bool setUpSuccessful = GitConnector.SetUpRepository(GitData.Instance.serializedData.UserName, GitData.Instance.serializedData.Email, GitData.Instance.serializedData.RepoUrl, createIgnoreFile);
                    if (setUpSuccessful) {
                        if (PluginStates.RepositorySetUp) {
                            //new settings? and new repository init / setup
                            EditorUtility.DisplayDialog("Set up of repository successfull!", "Repository has been setup once before.", TextConstants.OK);
                        } else {
                            //first time
                            EditorUtility.DisplayDialog("Set up of repository successfull!", "You can now initialize your repository by PUSHING your current work into an empty remote Git repository or PULLING (MERGING from remote repository) from the remote Git repository -> Your local work will be merged in case of conflicts.", TextConstants.OK);
                        }
                        PluginStates.RepositorySetUp = true;
                    } else {
                        string message = GitConnector.GetErrorAndExceptionMessageFromGit();
                        EditorUtility.DisplayDialog("Set Up of repository was not ok!", string.Format(TextConstants.ERROR_MESSAGE_SHORT, message), TextConstants.OK);
                    }
                }
            }
            GUI.enabled = true;
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Git Version", skin.label);
            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Check Version", GUILayout.Height(1.5f * EditorGUIUtility.singleLineHeight))) {
                    string gitVersion = GitConnector.GetVersion();
                    string message = GitConnector.GetErrorAndExceptionMessageFromGit();
                    if (string.IsNullOrWhiteSpace(message)) {
                        EditorUtility.DisplayDialog("Version Check OK", gitVersion, TextConstants.OK);
                    } else {
                        EditorUtility.DisplayDialog("Version Check not OK", string.Format(TextConstants.ERROR_MESSAGE_SHORT, message), TextConstants.OK);
                    }
                }
            }
            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Update Version", GUILayout.Height(1.5f * EditorGUIUtility.singleLineHeight))) {
                    string gitUpdateVersion = GitConnector.UpdateVersion();
                    string message = GitConnector.GetErrorAndExceptionMessageFromGit();
                    if (string.IsNullOrWhiteSpace(message)) {
                        EditorUtility.DisplayDialog("Version Update OK", gitUpdateVersion, TextConstants.OK);
                    } else {
                        EditorUtility.DisplayDialog("Version Update not OK", string.Format(TextConstants.ERROR_MESSAGE_SHORT, message), TextConstants.OK);
                    }
                }
            }
        }

        private string SaveSettings(string userName, string email, string repoUrl, string gitInstallFolder, string chosenDiffTool, string[] gitDiffOptions) {
            try {
                GitData.Instance.InitializeData(userName, email, repoUrl, gitInstallFolder, chosenDiffTool, gitDiffOptions);
            } catch (Exception ex) {
                return ex.Message;
            }
            return null;
        }
        /// <summary>
        /// Method Draw Tab About draws the whole tab 'About'
        /// </summary>
        public void DrawTabAbout() {
            EditorGUILayout.LabelField("Git Interface", skin.label);
            float originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 70f;
            EditorGUILayout.LabelField("Version:", TextConstants.VERSION);
            EditorGUILayout.LabelField("Email:", TextConstants.EMAIL);
            EditorGUILayout.LabelField("Creator:", TextConstants.NAME);
            EditorGUIUtility.labelWidth = originalWidth;
            using (new EditorGUILayout.HorizontalScope()) {
                tabIndexAbout = GUILayout.Toolbar(tabIndexAbout, texts: new[] { TextConstants.TAB_NAME_README, TextConstants.TAB_NAME_VERSIONS });
            }
            if (tabIndexAbout == 0) {
                DrawTabReadMe();
            } else if (tabIndexAbout == 1) {
                DrawTabVersions();
            }
        }
        /// <summary>
        /// Method Draw Tab Versions draws the whole sub tab 'Versions' in the Tab 'About'
        /// </summary>
        private void DrawTabVersions() {
            TextAsset versions = (TextAsset) Resources.Load("About/Versions");
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosAboutVersions)) {
                //GUI.enabled = false;
                scrollPosAboutVersions = scrollView.scrollPosition;
                GUILayout.TextArea(versions.text, skin.textArea, GUILayout.ExpandHeight(true));
            }
        }
        /// <summary>
        /// Method Draw Tab Read Me draws the whole sub tab 'Read Me' in the Tab 'About'
        /// </summary>
        private void DrawTabReadMe() {
            TextAsset readMe = (TextAsset) Resources.Load("About/ReadMe");
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosAboutReadMe)) {
                scrollPosAboutReadMe = scrollView.scrollPosition;
                GUILayout.TextArea(readMe.text, skin.textArea, GUILayout.ExpandHeight(true));
            }
        }
        /// <summary>
        /// Method Load Settings loads the serialized user data from the xml file
        /// </summary>
        private void LoadSettings() {
            GitData.DeserializeToXmlFile();
        }
        /// <summary>
        /// Method Initialize Views inializes are view data such as tree views for branches tab and changes tab
        /// </summary>
        private void InitializeViews() {
            List<ChangeRecord> gitChangeRecords = GetChanges();
            treeHelper = new TreeHelper(gitChangeRecords);
            if (branchesTreeViewState == null)
                branchesTreeViewState = new TreeViewState();

            branchesTreeView = new BranchesTreeView(branchesTreeViewState, StringUtility.ParseGitBranchesOutput(GitConnector.GetBranchesLocal(), GitConnector.GetBranchesRemote()));
        }
    }
}
