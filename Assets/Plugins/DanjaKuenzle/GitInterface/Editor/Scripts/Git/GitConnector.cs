using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace GitInterface {
    /// <summary>
    /// The Git Connector class contains the process class to create git processes and its also contains all available calls for git.
    /// </summary>
    public static class GitConnector {
        private const string VERSION = "--version";
        private const string GLOBAL_CONFIG = "config --global ";
        private const string PULL = "pull";
        private const string PUSH = "push";
        private const string ADD = "add ";
        private const string LOG = "log --date=format:\"%d.%m.%Y %H:%M:%S\" --pretty=format:\"Commit: %nSummary:%s %nDescription:%b %nDate:%cd %nAuthor: %cN %n\"";
        private const string BRANCH_LOCAL = "branch";
        private const string BRANCH_REMOTE = "branch -r";
        private const string STATUS = "status -s";
        private const string DIFFTOOL = "difftool ";
        private const string COMMIT = "commit ";

        private const string NO_GIT = "no-git";

        private static string GIT_INSTALL_FOLDER = "";
        private static string TEST_FILE_PATH = TextConstants.RESOURCES_DIRECTORY + "/TestCommit.txt";
        public static string GIT_IGNORE_FILE_PATH = TextConstants.WORKING_DIRECTORY + "/.gitignore";
        private static string REPO_URL;
        private static List<string> WarningsLogs;
        private static List<string> ErrorLogs;
        private static List<string> ExceptionLogs;
        public static string ErrorAndExceptionsBranches;
        public static string ErrorAndExceptionsChanges;

        /// <summary>
        /// Method Set Settings to set global git settings based on the entered information on the settings tab.
        /// </summary>
        /// <param name="userName">The string user Name.</param>
        /// <param name="email">The string email.</param>
        /// <param name="repoUrl">The string repository Url.</param>
        /// <param name="gitInstallFolder">The string git Installation Folder.</param>
        /// <param name="chosenDiffTool">The string chosen Difference Tool by the user.</param>
        /// <param name="createIgnoreFile">The bool create Ignore File.</param>
        /// <returns>
        /// Bool whether it was successful or not.
        /// </returns>
        internal static bool SetSettings(string userName, string email, string repoUrl, string gitInstallFolder, string chosenDiffTool, bool createIgnoreFile) {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();

            if (gitInstallFolder.Contains("\\")) {
                gitInstallFolder = gitInstallFolder.Replace("\\", "/");
            }
            bool isGitInstalled = false;
            if (gitInstallFolder.EndsWith("/")) {
                isGitInstalled = File.Exists(gitInstallFolder + "git.exe");
            } else {
                isGitInstalled = File.Exists(gitInstallFolder + "/" + "git.exe");
            }
            GIT_INSTALL_FOLDER = gitInstallFolder;
            if (isGitInstalled) {
                ExecuteGitCommand(VERSION); //check if git is installed
                ExecuteGitCommand(GLOBAL_CONFIG + "user.name \"" + userName + "\"");
                ExecuteGitCommand(GLOBAL_CONFIG + "user.email " + email);
                SetDiffTool(chosenDiffTool);

            } else {
                ErrorLogs.Add("Git Installation folder incorrect! Folder was: " + gitInstallFolder);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method Set Up Repository to set the repository url and to create a git Ignore file based on the entered information on the settings tab.
        /// A test commit & push is done with a test commit file to ensure commits are working.
        /// </summary>
        /// <param name="userName">The user Name.</param>
        /// <param name="email">The email.</param>
        /// <param name="repoUrl">The repository Url.</param>
        /// <param name="createIgnoreFile">The create Ignore File.</param>
        /// <returns>
        /// Bool whether it was successful or not.
        /// </returns>
        internal static bool SetUpRepository(string userName, string email, string repoUrl, bool createIgnoreFile) {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();
            string outputInit = ExecuteGitCommand("init"); //running init more than once is safe -> pick up newly addded templates
            string checkRemoteUrl = ExecuteGitCommand("remote -v");
            if (checkRemoteUrl == null || checkRemoteUrl.Length == 0 || !checkRemoteUrl.Contains(repoUrl)) {
                ExecuteGitCommand("remote add origin " + repoUrl); //https://github.com/Chuechli/GPR5100_GitInterface.git
                REPO_URL = repoUrl;
                //create test commit file
                File.Exists(TEST_FILE_PATH);
                File.AppendAllText(TEST_FILE_PATH, Environment.NewLine + "-------------------------------------------------------------------------" + Environment.NewLine + "Initialization Repo: " + repoUrl + Environment.NewLine + "-------------------------------------------------------------------------");
                ExecuteGitCommand(ADD + TEST_FILE_PATH.Replace(TextConstants.WORKING_DIRECTORY + "/", ""));
                if (createIgnoreFile) {
                    //create ignore file
                    TextAsset gitIgnoreFile = (TextAsset) Resources.Load("GitIgnore");
                    //Create file if it doesn't exist
                    if (!File.Exists(GIT_IGNORE_FILE_PATH)) {
                        File.WriteAllText(GIT_IGNORE_FILE_PATH, gitIgnoreFile.text);
                    }
                    ExecuteGitCommand(ADD + ".gitignore");
                }
                ExecuteGitCommand(COMMIT + "-m \"first commit\"");
                ExecuteGitCommand(BRANCH_LOCAL + " -M main");
                ExecuteGitCommand(PUSH + " -u origin main");
            } else {
                File.Exists(TEST_FILE_PATH);
                File.AppendAllText(TEST_FILE_PATH, Environment.NewLine + "-------------------------------------------------------------------------" + Environment.NewLine + "test push with user: " + userName + ", email: " + email + Environment.NewLine + "-------------------------------------------------------------------------");
                ExecuteGitCommand(ADD + TEST_FILE_PATH.Replace(TextConstants.WORKING_DIRECTORY + "/", ""));
                if (createIgnoreFile) {
                    //create ignore file
                    TextAsset gitIgnoreFile = (TextAsset) Resources.Load("GitIgnore");
                    //Create file if it doesn't exist
                    if (!File.Exists(GIT_IGNORE_FILE_PATH)) {
                        File.WriteAllText(GIT_IGNORE_FILE_PATH, gitIgnoreFile.text);
                    }
                    ExecuteGitCommand(ADD + ".gitignore");
                }
                ExecuteGitCommand(COMMIT + "-m \"test push with user: " + userName + ", email: " + email + "\"");
                ExecuteGitCommand(PUSH);
            }
            if (!string.IsNullOrWhiteSpace(GetErrorAndExceptionMessageFromGit())) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method Set Diff Tool to set chosen difference tool by the user.
        /// </summary>
        /// <param name="chosenDiffTool">The chosen Difference Tool.</param>
        private static void SetDiffTool(string chosenDiffTool) {
            ExecuteGitCommand(GLOBAL_CONFIG + "diff.tool " + chosenDiffTool);
            ExecuteGitCommand(GLOBAL_CONFIG + "difftool.prompt false");
        }
        /// <summary>
        /// Method Diff File to show the differences of a file between the local copy and remote copy (git difftool).
        /// </summary>
        /// <param name="item">The ChangesTreeElement item.</param>
        internal static void DiffFile(ChangesTreeElement item) {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();
            ExecuteGitCommand(DIFFTOOL + item.filePath, true);
        }
        /// <summary>
        /// Method Get Available Diff Tools to list all available (installed) difference tools for git.
        /// </summary>
        /// <returns>
        /// String of all available tools.
        /// </returns>
        internal static string GetAvailableDiffTools() {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();
            return ExecuteGitCommand(DIFFTOOL + "--tool-help");
        }
        /// <summary>
        /// Method Get Changes to list changes (changed files/folder) (git status).
        /// </summary>
        /// <returns>
        /// String of all changed files.
        /// </returns>
        public static string GetChanges() {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();
            ErrorAndExceptionsChanges = GetErrorAndExceptionMessageFromGit();
            return ExecuteGitCommand(STATUS);
        }
        /// <summary>
        /// Method Get History to list all logs/the whole history of commits (git log).
        /// </summary>
        /// <returns>
        /// String of all commits.
        /// </returns>
        public static string GetHistory() {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();
            return ExecuteGitCommand(LOG);
        }
        /// <summary>
        /// Method Initial Push to add, commit and push all changed file for the initial push.
        /// </summary>
        public static void InitialPush() {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();
            List<ChangeRecord> changeRecords = StringUtility.ParseGitChangesOutput(GetChanges());
            ExecuteGitCommand(ADD + "Assets/Plugins/DanjaKuenzle/GitInterface/Editor/Scripts/DTO/*");
            ExecuteGitCommand(ADD + "Assets/Plugins/DanjaKuenzle/GitInterface/Editor/Scripts/TreeView/*");
            ExecuteGitCommand(ADD + "Assets/Plugins/DanjaKuenzle/GitInterface/Editor/Scripts/Utility/*");
            ExecuteGitCommand(ADD + "Assets/Plugins/DanjaKuenzle/GitInterface/Editor/Scripts/GitConnector/*");
            foreach (var changeRecord in changeRecords) {
                ExecuteGitCommand(ADD + changeRecord.FilePath);
            }
            ExecuteGitCommand(COMMIT + "-m \"Initial Commit Unity Project\"");
            ExecuteGitCommand(PUSH);
        }
        /// <summary>
        /// Method Initial Pull to fetch all remote data (fetch --all) and in case of a merge conflict replace all local data (reset --hard origin/main).
        /// </summary>
        public static void InitialPull() {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();
            ExecuteGitCommand("fetch --all");
            ExecuteGitCommand("reset --hard origin/main");
        }
        /// <summary>
        /// Method Commit Selected Files to add, commit and push all selected changed files on the changes tab.
        /// </summary>
        /// <param name="changesTreeElements">The list of ChangesTreeElement changesTreeElements.</param>
        /// <param name="summary">The string summary.</param>
        /// <param name="description">The string description.</param>
        public static void CommitSelectedFiles(List<ChangesTreeElement> changesTreeElements, string summary, string description) {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();
            foreach (var item in changesTreeElements) {
                ExecuteGitCommand(ADD + item.filePath);
            }
            if (string.IsNullOrWhiteSpace(description)) {
                ExecuteGitCommand(COMMIT + "-m \"" + summary + "\"");
            } else {
                ExecuteGitCommand(COMMIT + "-m \"" + summary + "\" -m \"" + description + "\"");
            }
            ExecuteGitCommand(PUSH);
        }
        /// <summary>
        /// Method Get Branches Local to get all local branches (git branch).
        /// </summary>
        /// <returns>
        /// String of all local branches.
        /// </returns>
        public static string GetBranchesLocal() {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();
            ErrorAndExceptionsBranches = GetErrorAndExceptionMessageFromGit();
            return ExecuteGitCommand(BRANCH_LOCAL);
        }
        /// <summary>
        /// Method Get Branches Remote to get all remote branches (git branch -r).
        /// </summary>
        /// <returns>
        /// String of all remote branches.
        /// </returns>
        public static string GetBranchesRemote() {
            //if there are any errors, local branches method call catchs them
            return ExecuteGitCommand(BRANCH_REMOTE);
        }
        /// <summary>
        /// Method Get Error And Exception Message From Git to get all errors and exceptions from git commands.
        /// </summary>
        /// <returns>
        /// String of all error & exception messages.
        /// </returns>
        public static string GetErrorAndExceptionMessageFromGit() {
            string message = "";
            foreach (string errorLog in GitConnector.ErrorLogs) {
                message += errorLog + "\n";
            }
            foreach (string exceptionLog in GitConnector.ExceptionLogs) {
                message += exceptionLog + "\n";
            }
            return message;
        }
        /// <summary>
        /// Method Execute Git Command to execute a git command.
        /// </summary>
        /// <param name="@gitCommand">The string @gitCommand.</param>
        /// <param name="timeoutInMs">The int timeout In Ms. Default value 40000 (40s)</param>
        /// <returns>
        /// String of the command output.
        /// </returns>
        public static string ExecuteGitCommand(string @gitCommand, int timeoutInMs = 40000) {
            return ExecuteGitCommand(gitCommand, false, timeoutInMs);
        }
        /// <summary>
        /// Method Execute Git Command to execute a git command.
        /// </summary>
        /// <param name="@gitCommand">The string @gitCommand.</param>
        /// <param name="isShell">The bool is Shell.</param>
        /// <param name="timeoutInMs">The int timeout In Ms. Default value 40000 (40s)</param>
        /// <returns>
        /// String of the command output.
        /// </returns>
        public static string ExecuteGitCommand(string @gitCommand, bool isShell, int timeoutInMs = 40000) {
            // Strings that will catch the output from our process.
            string output = NO_GIT;
            string errorOutput = NO_GIT;
            try {
                // Set up our processInfo to run the git command and log to output and errorOutput.
                ProcessStartInfo processInfo = new ProcessStartInfo("git", @gitCommand) {
                    CreateNoWindow = !isShell,          // We want no visible pop-ups
                    UseShellExecute = isShell,        // Allows us to redirect input, output and error streams
                    RedirectStandardOutput = !isShell,  // Allows us to read the output stream
                    RedirectStandardError = !isShell,    // Allows us to read the error stream
                    WorkingDirectory = TextConstants.WORKING_DIRECTORY // Application.dataPath returns something like C:/MyWork/UnityProject/Asset back 1 level using "../" to get to project path -> Path.Combine(Application.dataPath, "../")
                };

                // Set up the Process
                Process process = new Process {
                    StartInfo = processInfo
                };

                try {
                    process.Start();  // Try to start it, catching any exceptions if it fails
                } catch (Exception e) {
                    // For now just assume its failed cause it can't find git.
                    UnityEngine.Debug.LogError("Git is not set-up correctly, git install folder (cmd) required to be on PATH: '" + GIT_INSTALL_FOLDER + "', and to be a git project.");
                    throw e;
                }
                // Read the results back from the process so we can get the output and check for errors
                output = process.StandardOutput.ReadToEnd();
                errorOutput = process.StandardError.ReadToEnd();
                if (timeoutInMs > 0) {
                    //no command should take longer than 40 seconds -> getting Diff tools takes the longest
                    if (!process.WaitForExit(timeoutInMs)) {
                        process.Kill();
                    }
                } else {
                    process.WaitForExit(); // Make sure we wait till the process has fully finished.
                }
                process.Close();        // Close the process ensuring it frees it resources.

                // Check for failure due to no git setup in the project itself or other fatal errors from git.
                if (output.Contains("fatal") || output == NO_GIT) { // || output == "") {
                    throw new Exception("Command: git " + @gitCommand + " Failed\n" + output + errorOutput);
                }
                // Log any errors.
                if (errorOutput != "" && !(@gitCommand.Contains(PUSH) && errorOutput.Contains("To " + REPO_URL))) {
                    if (errorOutput.Contains("warning")) {
                        WarningsLogs.Add("Git Warning: " + errorOutput);
                    } else if (errorOutput.Contains("error")) {
                        ErrorLogs.Add("Git Error: " + errorOutput);
                    } else if (errorOutput.Contains("fatal")) {
                        ExceptionLogs.Add("Git Exception: " + errorOutput);
                    } else {
                        //Infos
                        output = errorOutput;
                    }
                }
            } catch (Exception ex) {
                ExceptionLogs.Add("Git Exception: " + ex.Message);
            }

            return output;  // Return the output from git.
        }

        /// <summary>
        /// Method Get Version to get the currently installed git version.
        /// </summary>
        /// <returns>
        /// String of the current git version.
        /// </returns>
        internal static string GetVersion() {
            return ExecuteGitCommand(VERSION);
        }
        /// <summary>
        /// Method Update Version to update the currently installed git version to the newest version.
        /// </summary>
        /// <returns>
        /// String of output of the update.
        /// </returns>
        internal static string UpdateVersion() {
            WarningsLogs = new List<string>();
            ErrorLogs = new List<string>();
            ExceptionLogs = new List<string>();
            return ExecuteGitCommand("update-git-for-windows"); //update so it is newest version
        }
    }
}
