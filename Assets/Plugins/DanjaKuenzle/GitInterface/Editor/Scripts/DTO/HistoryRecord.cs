using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GitInterface {
    /// <summary>
    /// The History Record class is being used as a data transfer object to hold the parsed git data about the history/commits (git log).
    /// </summary>
    public class HistoryRecord {
        private string summary;
        private string description;
        private string date;
        private string author;
        /// <value>Gets & Sets the value of Summary.</value>
        public string Summary { get => summary; set => summary = value; }
        /// <value>Gets & Sets the value of Description.</value>
        public string Description { get => description; set => description = value; }
        /// <value>Gets & Sets the value of Date.</value>
        public string Date { get => date; set => date = value; }
        /// <value>Gets & Sets the value of Author.</value>
        public string Author { get => author; set => author = value; }

        /// <summary>
        /// Constructor to initialize a history record.
        /// </summary>
        /// <param name="summary">The string summary.</param>
        /// <param name="description">The string description.</param>
        /// <param name="date">The string date.</param>
        /// <param name="author">The string author.</param>
        public HistoryRecord(string summary, string description, string date, string author) {
            this.Summary = summary;
            this.Description = description;
            this.Date = date;
            this.Author = author;
        }
    }
}
