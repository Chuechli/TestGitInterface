using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GitInterface {
    /// <summary>
    /// The static String Utility class contains all utility methods to parse every git command output returned by the process class.
    /// </summary>
    public static class StringUtility {
        /// <summary>
        /// Method Parse Git Changes Output parses and sorts the output of a 'git status' command.
        /// DD - Deleted files
        /// M  - Changes not staged for commit
        /// ?? - Untracked files
        /// </summary>
        /// <param name="changes">The string changes.</param>
        /// <returns>
        /// List of ChangeRecord objects.
        /// </returns>
        public static List<ChangeRecord> ParseGitChangesOutput(string changes) {
            List<ChangeRecord> records = new List<ChangeRecord>();
            changes = changes.Replace("\n", "@");
            string[] lines = changes.Split('@');
            foreach (var item in lines) {
                if (!string.IsNullOrWhiteSpace(item)) {
                    string line = item.Trim();
                    string status = line.Substring(0, 2).Trim();
                    string fileName = line.Substring(line.IndexOf(" ") + 1);
                    if (string.Equals("D", status)) {
                        records.Add(new ChangeRecord(fileName.Substring(fileName.LastIndexOf("/") + 1), fileName, ChangeRecord.State.Deleted));
                    } else if (string.Equals("M", status)) {
                        records.Add(new ChangeRecord(fileName.Substring(fileName.LastIndexOf("/") + 1), fileName, ChangeRecord.State.Modified));
                    } else if (string.Equals("??", status)) {
                        records.Add(new ChangeRecord(fileName.Substring(fileName.LastIndexOf("/") + 1), fileName, ChangeRecord.State.New));
                    }
                }
            }
            records.Sort(delegate (ChangeRecord c1, ChangeRecord c2) {

                if (c1.FilePath.Contains("/") && c2.FilePath.Contains("/")) {
                    if (c1.FilePath.Substring(0, c1.FilePath.LastIndexOf("/")) == c2.FilePath.Substring(0, c2.FilePath.LastIndexOf("/"))) {
                        //in same folder
                        return c1.FilePath.CompareTo(c2.FilePath);
                    } else if (c1.FilePath.Substring(0, c1.FilePath.LastIndexOf("/")).Contains(c2.FilePath.Substring(0, c2.FilePath.LastIndexOf("/")))) {
                        //c2 is a file and in subfolder of c1
                        return 1;
                    } else if (c2.FilePath.Substring(0, c2.FilePath.LastIndexOf("/")).Contains(c1.FilePath.Substring(0, c1.FilePath.LastIndexOf("/")))) {
                        //c1 is a file and in subfolder of c2
                        return -1;
                    } else if (c1.FilePath.Contains(c2.FilePath.Substring(0, c2.FilePath.LastIndexOf("/")))) {
                        //c1 is subfolder of c2
                        return 1;
                    } else if (c2.FilePath.Contains(c1.FilePath.Substring(0, c1.FilePath.LastIndexOf("/")))) {
                        //c2 is subfolder of c1
                        return -1;
                    }
                    return c1.FilePath.CompareTo(c2.FilePath);
                } else if (c1.FilePath.Contains("/")) {
                    return 1;
                } else if (c2.FilePath.Contains("/")) {
                    return -1;
                } else {
                    return c1.FilePath.CompareTo(c2.FilePath);
                }
            });
            return records;
        }
        /// <summary>
        /// Method Parse Git History Output parses the output of a 'git log' command.
        /// </summary>
        /// <param name="history">The string history.</param>
        /// <returns>
        /// List of HistoryRecord objects.
        /// </returns>
        public static List<HistoryRecord> ParseGitHistoryOutput(string history) {
            List<HistoryRecord> records = new List<HistoryRecord>();
            string summaryText = "Summary:";
            string descriptionText = "Description:";
            string dateText = "Date:";
            string authorText = "Author:";

            history = history.Replace("Commit:", "§");
            string[] lines = history.Split('§');
            foreach (var item in lines) {
                if (!string.IsNullOrWhiteSpace(item)) {
                    string line = item.Trim();
                    string summary = ParseGitHistoryRecord(line, summaryText, descriptionText);
                    string description = ParseGitHistoryRecord(line, descriptionText, dateText);
                    string date = ParseGitHistoryRecord(line, dateText, authorText);
                    string author = ParseGitHistoryRecord(line, authorText, null);
                    records.Add(new HistoryRecord(summary, description, date, author));
                }
            }
            return records;
        }
        /// <summary>
        /// Method Parse Git History Record parses the partially processed data of the method Parse Git History Output.
        /// </summary>
        /// <param name="line">The string line.</param>
        /// <param name="currentField">The string currentField.</param>
        /// <param name="nextField">The string nextField.</param>
        /// <returns>
        /// String of the value.
        /// </returns>
        private static string ParseGitHistoryRecord(string line, string currentField, string nextField) {
            string fieldValue = line.Substring(line.IndexOf(currentField) + currentField.Length);
            if (!string.IsNullOrWhiteSpace(nextField)) {
                fieldValue = fieldValue.Substring(0, fieldValue.IndexOf(nextField));
            }
            return fieldValue.Trim();
        }
        /// <summary>
        /// Method Parse Git Branches Output parses the output of a 'git branch' and 'git branch -r' command.
        /// </summary>
        /// <param name="localBranches">The string local Branches.</param>
        /// <param name="remoteBranches">The string remote Branches.</param>
        /// <returns>
        /// List of BranchRecord objects.
        /// </returns>
        public static List<BranchRecord> ParseGitBranchesOutput(string localBranches, string remoteBranches) {
            List<BranchRecord> records = new List<BranchRecord>();
            foreach (var item in ParseBranch(localBranches, BranchRecord.Type.Local)) {
                records.Add(item);
            }
            foreach (var item in ParseBranch(remoteBranches, BranchRecord.Type.Remote)) {
                records.Add(item);
            }
            return records;
        }
        /// <summary>
        /// Method Parse Branch parses the partially processed data of the method Parse Git Branches Output.
        /// </summary>
        /// <param name="branches">The string branches.</param>
        /// <param name="type">The BranchRecord.Type type.</param>
        /// <returns>
        /// List of BranchRecord objects.
        /// </returns>
        private static List<BranchRecord> ParseBranch(string branches, BranchRecord.Type type) {
            List<BranchRecord> records = new List<BranchRecord>();
            branches = branches.Replace("\n", "§");
            string[] branchesLines = branches.Split('§');
            foreach (var item in branchesLines) {
                if (!string.IsNullOrWhiteSpace(item)) {
                    string line = item.Trim();
                    if (line.StartsWith("*")) {
                        //currentBranch
                        records.Add(new BranchRecord(line, type, true));
                    } else {
                        records.Add(new BranchRecord(line, type, false));
                    }
                }
            }
            return records;
        }
        /// <summary>
        /// Method Parse Diff Tools parses the output of a 'git difftool' command.
        /// </summary>
        /// <param name="diffTools">The string diff Tools.</param>
        /// <returns>
        /// String array of all diff tools.
        /// </returns>
        internal static string[] ParseDiffTools(string diffTools) {
            if (string.IsNullOrWhiteSpace(diffTools)) {
                return new string[] { "not available" };
            } else {
                diffTools = diffTools.Substring(diffTools.IndexOf(":") + 1);
                diffTools = diffTools.Substring(0, diffTools.IndexOf("\n\n")).Trim();
                diffTools = diffTools.Replace("\n\t\t", "@");
                string[] toolArray = diffTools.Split('@');
                return toolArray;
            }
        }
    }
}
