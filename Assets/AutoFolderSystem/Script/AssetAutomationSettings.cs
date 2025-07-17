using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AssetAutomationSettings : ScriptableObject
{
    private static AssetAutomationSettings instance;
    public static AssetAutomationSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = AssetDatabase.LoadAssetAtPath<AssetAutomationSettings>(SETTINGS_PATH);
                if (instance == null)
                {
                    string directoryPath = Path.GetDirectoryName(SETTINGS_PATH).Replace('\\', '/');
                    if (!AssetDatabase.IsValidFolder(directoryPath))
                    {
                        string currentPath = "Assets";
                        string[] folders = directoryPath.Split('/');
                        for (int i = 1; i < folders.Length; i++)
                        {
                            string nextPath = Path.Combine(currentPath, folders[i]).Replace('\\', '/');
                            if (!AssetDatabase.IsValidFolder(nextPath))
                                AssetDatabase.CreateFolder(currentPath, folders[i]);
                            currentPath = nextPath;
                        }
                    }
                    instance = CreateInstance<AssetAutomationSettings>();
                    AssetDatabase.CreateAsset(instance, SETTINGS_PATH);
                    AssetDatabase.SaveAssets();
                }
            }
            return instance;
        }
    }

    public string SelectedExtension = ".png";
    public List<string> TargetFolders = new List<string>();
    public List<string> CustomExtensions = new List<string>();
    public List<string> ExcludedFolders = new List<string> { "Assets/AutoFolderSystem", "Assets/Editor" };
    public string SourceMoveFolder = "Assets/";
    public string DestinationMoveFolder = "Assets/";
    public string FileNameContainsForMove = "";
    public string FileNameExcludesForMove = "";
    public bool CreateSubfolder = false;
    public string SubfolderNamePrefix = "";
    public string DeleteTargetFolder = "Assets/";
    public string FileNameContainsForDelete = "";
    public string FileNameExcludesForDelete = "";
    public bool IncludeSubfoldersForDelete = true;

    private const string SETTINGS_PATH = "Assets/Editor/AssetAutomation/AssetAutomationSettings.asset";

    public void SaveSettings()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public void AddCustomExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            EditorUtility.DisplayDialog("확장자 오류", "확장자를 입력해주세요.", "확인");
            return;
        }

        if (!extension.StartsWith("."))
            extension = "." + extension;

        extension = extension.ToLower();
        if (CustomExtensions.Contains(extension) || new[] { ".png", ".jpg", ".fbx", ".wav", ".mp3", ".ogg" }.Contains(extension))
        {
            EditorUtility.DisplayDialog("확장자 오류", $"확장자 '{extension}'는 이미 존재하거나 기본 확장자입니다.", "확인");
            return;
        }

        CustomExtensions.Add(extension);
        SaveSettings();
    }

    public void RemoveCustomExtension(string extension)
    {
        if (CustomExtensions.Remove(extension) && SelectedExtension == extension)
            SelectedExtension = ".png";
        SaveSettings();
    }

    public void AddExcludedFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("경로 오류", "폴더 경로를 입력해주세요.", "확인");
            return;
        }

        if (!path.StartsWith("Assets/"))
            path = Path.Combine("Assets", path.TrimStart('/')).Replace('\\', '/');
        path = path.TrimEnd('/');

        if (ExcludedFolders.Contains(path))
        {
            EditorUtility.DisplayDialog("경로 오류", $"폴더 '{path}'는 이미 제외 목록에 있습니다.", "확인");
            return;
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            EditorUtility.DisplayDialog("경로 오류", $"폴더 '{path}'는 유효한 Assets 내 폴더가 아닙니다.", "확인");
            return;
        }

        ExcludedFolders.Add(path);
        SaveSettings();
    }

    public void RemoveExcludedFolder(string path)
    {
        if (ExcludedFolders.Remove(path))
            SaveSettings();
    }

    public void ClearAllSettings()
    {
        SelectedExtension = ".png";
        TargetFolders.Clear();
        CustomExtensions.Clear();
        ExcludedFolders.Clear();
        ExcludedFolders.Add("Assets/AutoFolderSystem");
        ExcludedFolders.Add("Assets/Editor");
        SourceMoveFolder = "Assets/";
        DestinationMoveFolder = "Assets/MovedAssets/";
        FileNameContainsForMove = "";
        FileNameExcludesForMove = "";
        CreateSubfolder = true;
        SubfolderNamePrefix = "";
        DeleteTargetFolder = "Assets/";
        FileNameContainsForDelete = "";
        FileNameExcludesForDelete = "";
        IncludeSubfoldersForDelete = true;
        SaveSettings();
    }
}