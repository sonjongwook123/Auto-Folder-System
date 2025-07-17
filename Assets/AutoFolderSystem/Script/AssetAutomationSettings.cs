using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SJW
{
    public class AssetAutomationSettings : ScriptableObject
    {
        private static AssetAutomationSettings instance;

        public static AssetAutomationSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabase.LoadAssetAtPath<AssetAutomationSettings>("Assets/AutoFolderSystem/AssetAutomationSettings.asset");
                    if (instance == null)
                    {
                        instance = CreateInstance<AssetAutomationSettings>();
                        AssetDatabase.CreateAsset(instance, "Assets/AutoFolderSystem/AssetAutomationSettings.asset");
                        AssetDatabase.SaveAssets();
                    }
                }
                return instance;
            }
        }

        [SerializeField] private List<string> targetFolders = new List<string>();
        [SerializeField] private string selectedExtension = ".png";
        [SerializeField] private List<string> customExtensions = new List<string>();
        [SerializeField] private List<string> excludedFolders = new List<string> { "Assets/AutoFolderSystem" };
        [SerializeField] private string sourceMoveFolder = "Assets";
        [SerializeField] private string destinationMoveFolder = "Assets";
        [SerializeField] private string fileNameContainsForMove = "";
        [SerializeField] private string fileNameExcludesForMove = "";
        [SerializeField] private bool createSubfolder = false;
        [SerializeField] private string subfolderNamePrefix = "";
        [SerializeField] private string deleteTargetFolder = "Assets";
        [SerializeField] private bool includeSubfoldersForDelete = false;
        [SerializeField] private string fileNameContainsForDelete = "";
        [SerializeField] private string fileNameExcludesForDelete = "";
        [SerializeField] private string renameSourceFolder = "Assets";
        [SerializeField] private string renameOldString = "";
        [SerializeField] private string renameNewString = "";
        [SerializeField] private string renameFileNameContains = "";
        [SerializeField] private string renameFileNameExcludes = "";
        [SerializeField] private string renamePrefix = "";
        [SerializeField] private string renameSuffix = "";
        [SerializeField] private List<string> selectedFiles = new List<string>();

        public List<string> TargetFolders { get => targetFolders; set => targetFolders = value; }
        public string SelectedExtension { get => selectedExtension; set => selectedExtension = value; }
        public List<string> CustomExtensions { get => customExtensions; set => customExtensions = value; }
        public List<string> ExcludedFolders { get => excludedFolders; set => excludedFolders = value; }
        public string SourceMoveFolder { get => sourceMoveFolder; set => sourceMoveFolder = value; }
        public string DestinationMoveFolder { get => destinationMoveFolder; set => destinationMoveFolder = value; }
        public string FileNameContainsForMove { get => fileNameContainsForMove; set => fileNameContainsForMove = value; }
        public string FileNameExcludesForMove { get => fileNameExcludesForMove; set => fileNameExcludesForMove = value; }
        public bool CreateSubfolder { get => createSubfolder; set => createSubfolder = value; }
        public string SubfolderNamePrefix { get => subfolderNamePrefix; set => subfolderNamePrefix = value; }
        public string DeleteTargetFolder { get => deleteTargetFolder; set => deleteTargetFolder = value; }
        public bool IncludeSubfoldersForDelete { get => includeSubfoldersForDelete; set => includeSubfoldersForDelete = value; }
        public string FileNameContainsForDelete { get => fileNameContainsForDelete; set => fileNameContainsForDelete = value; }
        public string FileNameExcludesForDelete { get => fileNameExcludesForDelete; set => fileNameExcludesForDelete = value; }
        public string RenameSourceFolder { get => renameSourceFolder; set => renameSourceFolder = value; }
        public string RenameOldString { get => renameOldString; set => renameOldString = value; }
        public string RenameNewString { get => renameNewString; set => renameNewString = value; }
        public string RenameFileNameContains { get => renameFileNameContains; set => renameFileNameContains = value; }
        public string RenameFileNameExcludes { get => renameFileNameExcludes; set => renameFileNameExcludes = value; }
        public string RenamePrefix { get => renamePrefix; set => renamePrefix = value; }
        public string RenameSuffix { get => renameSuffix; set => renameSuffix = value; }
        public List<string> SelectedFiles { get => selectedFiles; set => selectedFiles = value; }

        public void AddCustomExtension(string extension)
        {
            if (!string.IsNullOrEmpty(extension) && !customExtensions.Contains(extension))
            {
                customExtensions.Add(extension);
                SaveSettings();
            }
        }

        public void RemoveCustomExtension(string extension)
        {
            if (customExtensions.Contains(extension))
            {
                customExtensions.Remove(extension);
                SaveSettings();
            }
        }

        public void AddExcludedFolder(string folderPath)
        {
            if (!string.IsNullOrEmpty(folderPath) && !excludedFolders.Contains(folderPath))
            {
                excludedFolders.Add(folderPath);
                SaveSettings();
            }
        }

        public void RemoveExcludedFolder(string folderPath)
        {
            if (excludedFolders.Contains(folderPath))
            {
                excludedFolders.Remove(folderPath);
                SaveSettings();
            }
        }

        public void ClearAllSettings()
        {
            targetFolders.Clear();
            selectedExtension = ".png";
            customExtensions.Clear();
            excludedFolders.Clear();
            excludedFolders.Add("Assets/Editor");
            excludedFolders.Add("Assets/AutoFolderSystem");
            sourceMoveFolder = "Assets";
            destinationMoveFolder = "Assets";
            fileNameContainsForMove = "";
            fileNameExcludesForMove = "";
            createSubfolder = false;
            subfolderNamePrefix = "";
            deleteTargetFolder = "Assets";
            includeSubfoldersForDelete = false;
            fileNameContainsForDelete = "";
            fileNameExcludesForDelete = "";
            renameSourceFolder = "Assets";
            renameOldString = "";
            renameNewString = "";
            renameFileNameContains = "";
            renameFileNameExcludes = "";
            renamePrefix = "";
            renameSuffix = "";
            selectedFiles.Clear();
            SaveSettings();
        }

        public void SaveSettings()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}