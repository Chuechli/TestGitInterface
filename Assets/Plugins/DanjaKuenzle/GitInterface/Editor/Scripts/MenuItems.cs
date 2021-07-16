using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GitInterface {
    /// <summary>
    /// The Menu Items class creates the Menu Item(s) for the Git Interface Plugin (Tools/GitInterface)
    /// </summary>
    public static class MenuItems {
        /// <summary>
        /// Creates the root menu item for the Git Interface Plugin
        /// </summary>
        [MenuItem("Tools/GitInterface _g")]
        private static void ShowGitInterfaceWindow() {
            GitInterfaceWindow.ShowGitInterfaceWindow();
        }
    }
}
