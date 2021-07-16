using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace GitInterface {
    /// <summary>
    /// The Plugin States class keeps the states of the plugin at all times.
    /// </summary>
    public class PluginStates {
        /// <value>Checks if the gitData XML file exists and therefor if the settings have been saved.</value>
        public static bool SettingsSaved { get => File.Exists(TextConstants.GIT_DATA_PATH); }
        /// <value>Checks if the repository is initialized, meaning if the variable 'RepositoryInitialized' has been set to true in the repository registry.</value>
        public static bool RepositoryInitialized { get => bool.Parse(PlayerPrefs.GetString("RepositoryInitialized", "false")); set => PlayerPrefs.SetString("RepositoryInitialized", value.ToString()); }
        /// <value>Checks if the repository is set up, meaning if the variable 'RepositorySetUp' has been set to true in the repository registry.</value>
        public static bool RepositorySetUp { get => bool.Parse(PlayerPrefs.GetString("RepositorySetUp", "false")); set => PlayerPrefs.SetString("RepositorySetUp", value.ToString()); }
        /// <value>Checks if the diff tool has been chosen in the settings and therefor all available diff tools not have to be reloaded again when the plugin is being opened.</value>
        public static bool DiffToolExists { get => SettingsSaved && !string.IsNullOrWhiteSpace(GitData.Instance.serializedData.ChosenDiffTool); }
    }
}
