Git Interface 1.0
____________________________________

##Description
Do you need an easy way to upload the current work progress of your Unity Project to Git? Then 'Git Interface' is the perfect Plugin for you!
Doesn't matter if you start a new Unity Project or if you want to upload the content of an ongoing Unity Project to Git - 'Git Interface' has it covered.
You can also monitor all of your project's branches, see, select and commit changed files as well as see the whole commit history of your Unity Project. 

##Features
-- Changes --
View, commit and compare all of your changed files in a clear overview.
The name of the file/folder will be shown as well as the whole path to it.
All changes are marked as either 'New', 'Modified' or 'Deleted' for a better understanding to what exactly happened to that specific file or path.
For all 'Changed' files a comparison between the current version and the version on Git is available. For your Difference Tool of choice please see the "Settings" Feature below.
All files, none, single files as well as multiple files or whole folder strucutres can be selected for commit.
The amount of changed files as well as selected files is shown on the right side of the view.
A mandatory summary and a optional description can be entered for the commit.
All changed files can be reloaded - in case some files changed in the meantime.

-- History --
All commits are listed by date in descending order.
Foreach listed commit, the summary, the description if available, the timestamp and the commit user name is shown.

-- Branches --
View all branches of your Git/Unity project listed as local or remote branch.
Your current local branch will be marked with an asterix '*'.

-- Settings --
Git Configuration:
Enter the path to your installed Git so it can be verified if Git is installed on your Copmuter.
Enter and set your git credential and settings. (Name, Email)
Choose your favourite Difference Tool for file comparisons (see 'Changes' feature) from all of your installed and supported git diff tools.
Enter your remote repository URL from git and choose whether you want a .gitIgnore file to be created for you or not. (a default .gitIgnore for standard Unity Projects would then be generated automatically)

Git Version:
Check the currently installed git version.
Update to the newest git version.

-- Initialization -- 
Choose between 2 initialization options:
1. In case your Git Repository is empty:
-> Push whole Unity Project to Remote Git Repository

2. In case your Git Repository is NOT empty and you created a new Unity Project:
-> Pull all remote data from your Remote Git Repository. In case of a merge (your file exists locally AND remotely but with different content) your local copy will be replaced by the remote version.

-- About --
Description of 'Git Interface' such as current version, creator email for feedback, bug reports or questions,creator name.
'Read Me' file as text and all versions/releases so far and its descriptions

##Documentation
Full documentation can be found as attached PDF.

##Compatibility
Currently on supports Windows.
Tested compatibility only with Unity version 2019.4.27f1.

##Limitations:
- Merging is not supported and therefor only suitable for a single-person-usage
- Pulling after the initial pull (see 'Initialization') is not supported
- Creating Branches is not supported
- Sometimes git processes create a co-process that causes a lock file to be created and the process is stuck, please restart Unity or kill all git process withing Unity (Task Manager) and remove the lock file afterwards (inside .git folder)

##Guide / First Time Usage:
1. GitHub/Git must be installed
2. Import Plugin 'GitIiterface' and open via Tools -> Git Interface (opening it takes a while..)
3. Git must be at least on version '2.32.0.windows.1'
	-> Check version via 'Settings' Tab ('git --version')
	-> If version is older than the required version, use Update Version via 'Settings' Tab ('git update-git-for-windows')
4. Check if 'Git Credential Manager' for git is installed
	-> Open Programm 'Git Bash': enter 'git credential-manager version'
	-> If Credential Manager is not installed, uninstall Git and freshly install it again (tick 'use Credential Manager' in during the installation setup)
5. If you dont have a Git Repository yet to where you want to upload your Unity Project:
	-> Create a repository on Git Hub website (NO .gitIgnore file in case you want a default .gitIgnore file for Unity Project generated)
	-> add a .gitIgnore file in case you don't want a default generated one
6. Go to 'Settings' Tab in Git Interface Window, enter your settings and press 'Check & Set Settings for Git' then press 'Set Up Repository'
7. Go to 'Initialization' Tab where you can initialize your project (see 'Feature: Initialization' above for more information)
8. A pop up will appear during the Initialization where you have to enter a authentication token or authorize via web browser - this is a 1-time popup!