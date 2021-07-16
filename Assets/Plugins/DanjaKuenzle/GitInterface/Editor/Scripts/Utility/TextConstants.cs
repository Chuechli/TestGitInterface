using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GitInterface {
    /// <summary>
    /// The Text Constants class contains contants of string values that are used more than once.
    /// </summary>
    public static class TextConstants {
        public const string TOOL_NAME = "Git Interface";
        public const string SKIN_NAME = "GUISkin";
        public const string VERSION = "1.0";
        public const string EMAIL = "danja.kuenzle@swissonline.ch";
        public const string NAME = "Danja Künzle";
        public const string TAB_NAME_README = "Read Me";
        public const string TAB_NAME_VERSIONS = "Versions";

        public const string TAB_NAME_ABOOUT = "About";
        public const string TAB_NAME_SETTINGS = "Settings";
        public const string TAB_NAME_INITIALIZE = "Initialize";
        public const string TAB_NAME_CHANGES = "Changes";
        public const string TAB_NAME_HISTORY = "History";
        public const string TAB_NAME_BRANCHES = "Branches";

        public const string ERROR_MESSAGE_LONG = "Getting {0} from Git not successful! \nThere were warnings / errors:\n{1}";
        public const string ERROR_MESSAGE_SHORT = "There were warnings / errors:\n{0}";
        public const string ERROR_MESSAGE_EMPTY = "{0} must not be empty!";
        public const string ERROR_MESSAGE_INVALID = "{0} is not valid!";

        public const string OK = "OK";

        public static readonly string GIT_DATA_PATH = Application.dataPath + "/Plugins/DanjaKuenzle/GitInterface/Editor/Resources/User/GitData.xml";
        public static readonly string RESOURCES_DIRECTORY = Application.dataPath + "/Plugins/DanjaKuenzle/GitInterface/Editor/Resources";
        public static readonly string WORKING_DIRECTORY = Application.dataPath.Replace("/Assets", "");
    }
}
