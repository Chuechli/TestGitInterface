using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Xml.Serialization;
using System.Text;

namespace GitInterface {
    /// <summary>
    /// The singleton Git Data class contains the entered user data from the settings tab.
    /// </summary>
    public sealed class GitData {
        private static readonly GitData instance = new GitData();

        public SerializedData serializedData;

        /// <summary>
        /// Method to initialize the serializedData object and save it to XML File (Assets/Plugins/DanjaKuenzle/GitInterface/Editor/Resources/User/GitData.xml).
        /// </summary>
        /// <param name="userName">The string user name.</param>
        /// <param name="email">The string email.</param>
        /// <param name="repoUrl">The string repository url (https).</param>
        /// <param name="gitInstallFolder">The string git installation folder.</param>
        /// <param name="chosenDiffTool">The string chosen difference tool by the user.</param>
        /// <param name="gitDiffOptions">The string array of all difference tools that are available for git.</param>
        public void InitializeData(string userName, string email, string repoUrl, string gitInstallFolder, string chosenDiffTool, string[] gitDiffOptions) {
            serializedData = new SerializedData(userName, email, repoUrl, gitInstallFolder, chosenDiffTool, gitDiffOptions);
            SerializeToXmlFile();
        }

        /// <summary>
        /// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit.
        /// </summary>
        static GitData() {
        }

        private GitData() {
        }

        /// <summary>
        /// The singleton of this Git Data class.
        /// </summary>
        /// <returns>
        /// The singleton of this Git Data class.
        /// </returns>
        public static GitData Instance {
            get {
                return instance;
            }
        }

        /// <summary>
        /// The Serialized Data class contains serializable user data from the settings tab.
        /// </summary>
        [Serializable]
        public class SerializedData {
            private string userName;
            private string email;
            private string repoUrl;
            private string gitInstallFolder;
            private string chosenDiffTool;
            private string[] gitDiffOptions;
            /// <value>Gets the value of UserName.</value>
            public string UserName { get => userName; set => userName = value; }
            /// <value>Gets the value of Email.</value>
            public string Email { get => email; set => email = value; }
            /// <value>Gets the value of RepoUrl.</value>
            public string RepoUrl { get => repoUrl; set => repoUrl = value; }
            /// <value>Gets the value of GitInstallFolder.</value>
            public string GitInstallFolder { get => gitInstallFolder; set => gitInstallFolder = value; }
            /// <value>Gets the value of ChosenDiffTool.</value>
            public string ChosenDiffTool { get => chosenDiffTool; set => chosenDiffTool = value; }
            /// <value>Gets the value of GitDiffOptions.</value>
            public string[] GitDiffOptions { get => gitDiffOptions; set => gitDiffOptions = value; }

            public SerializedData() {
            }

            /// <summary>
            /// Constructor to initialize the serializedData object and save it to XML File.
            /// </summary>
            /// <param name="userName">The string user name.</param>
            /// <param name="email">The string email.</param>
            /// <param name="repoUrl">The string repository url (https).</param>
            /// <param name="gitInstallFolder">The string git installation folder.</param>
            /// <param name="chosenDiffTool">The string chosen difference tool by the user.</param>
            /// <param name="gitDiffOptions">All string array of difference tools that are available for git.</param>
            public SerializedData(string userName, string email, string repoUrl, string gitInstallFolder, string chosenDiffTool, string[] gitDiffOptions) {
                this.UserName = userName;
                this.Email = email;
                this.RepoUrl = repoUrl;
                this.GitInstallFolder = gitInstallFolder;
                this.ChosenDiffTool = chosenDiffTool;
                this.GitDiffOptions = gitDiffOptions;
            }
        }

        /// <summary>
        /// Method to serialize the serialized data to an xml file (Assets/Plugins/DanjaKuenzle/GitInterface/Editor/Resources/User/GitData.xml).
        /// </summary>
        private static void SerializeToXmlFile() {
            Directory.CreateDirectory(TextConstants.GIT_DATA_PATH.Substring(0, TextConstants.GIT_DATA_PATH.LastIndexOf("/")));
            XmlSerializer ser = new XmlSerializer(typeof(SerializedData));
            TextWriter writer = new StreamWriter(TextConstants.GIT_DATA_PATH);
            ser.Serialize(writer, instance.serializedData);
            writer.Close();
        }

        /// <summary>
        /// Method to deserialize the serialized data to an xml file (Assets/Plugins/DanjaKuenzle/GitInterface/Editor/Resources/User/GitData.xml).
        /// </summary>
        public static void DeserializeToXmlFile() {
            XmlSerializer ser = new XmlSerializer(typeof(SerializedData));
            var fileStream = new FileStream(TextConstants.GIT_DATA_PATH, FileMode.Open);
            SerializedData serializedData = (SerializedData) ser.Deserialize(fileStream);
            if (instance.serializedData == null) {
                instance.serializedData = new SerializedData(serializedData.UserName, serializedData.Email, serializedData.RepoUrl, serializedData.GitInstallFolder, serializedData.ChosenDiffTool, serializedData.GitDiffOptions);
            } else {
                instance.serializedData.UserName = serializedData.UserName;
                instance.serializedData.Email = serializedData.Email;
                instance.serializedData.RepoUrl = serializedData.RepoUrl;
                instance.serializedData.GitInstallFolder = serializedData.GitInstallFolder;
                instance.serializedData.ChosenDiffTool = serializedData.ChosenDiffTool;
                instance.serializedData.GitDiffOptions = serializedData.GitDiffOptions;
            }
            fileStream.Close();
        }
    }

}
