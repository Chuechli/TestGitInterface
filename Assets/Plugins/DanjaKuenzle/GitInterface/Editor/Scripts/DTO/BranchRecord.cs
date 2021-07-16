using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GitInterface {
    /// <summary>
    /// The Branch Record class is being used as a data transfer object to hold the parsed git data about branches (git branch).
    /// </summary>
    public class BranchRecord {
        private string branchName;
        private Type type;
        private bool isCurrentBranch;
        /// <value>Gets & Sets the value of Branch Name.</value>
        public string BranchName { get => branchName; set => branchName = value; }
        /// <value>Gets & Sets the value of Type Object.</value>
        public Type TypeObject { get => type; set => type = value; }
        /// <value>Gets & Sets the value of Is Current Branch.</value>
        public bool IsCurrentBranch { get => isCurrentBranch; set => isCurrentBranch = value; }

        /// <summary>
        /// The enum Type contains all types of branches (Local, Remote).
        /// </summary>
        public enum Type {
            Local,
            Remote
        }

        /// <summary>
        /// Constructor to initialize a branch record.
        /// </summary>
        /// <param name="branchName">The string branch Name.</param>
        /// <param name="type">The Type type.</param>
        /// <param name="isCurrentBranch">The bool is Current Branch.</param>
        public BranchRecord(string branchName, Type type, bool isCurrentBranch) {
            this.branchName = branchName;
            this.type = type;
            this.isCurrentBranch = isCurrentBranch;
        }
    }
}
