using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GitInterface {
    /// <summary>
    /// The Change Record class is being used as a data transfer object to hold the parsed git data about changes (git status).
    /// </summary>
    public class ChangeRecord {
        private string fileName;
        private string filePath;
        private State state;
        /// <value>Gets & Sets the value of File Path.</value>
        public string FilePath { get => filePath; set => filePath = value; }
        /// <value>Gets & Sets the value of State Object.</value>
        public State StateObject { get => state; set => state = value; }

        /// <summary>
        /// The enum State contains all states of changes (New, Modified, Deleted).
        /// </summary>
        public enum State {
            New,
            Modified,
            Deleted
        }
        /// <summary>
        /// Constructor to initialize a change record.
        /// </summary>
        /// <param name="fileName">The string file Name.</param>
        /// <param name="filePath">The string file Path.</param>
        /// <param name="state">The State state.</param>
        public ChangeRecord(string fileName, string filePath, State state) {
            this.fileName = fileName;
            this.FilePath = filePath;
            this.state = state;
        }
    }
}