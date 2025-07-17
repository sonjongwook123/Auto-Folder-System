using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AssetAutomationSettings : ScriptableObject
{
    private static AssetAutomationSettings _instance;
    public static AssetAutomationSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = LoadOrCreateSettings();
            }
            return _instance;
        }
    }

    public string SelectedExtension = ".png";
    public List<string> TargetFolders = new List<string>();
    public List<string> CustomExtensions = new List<string>();

    public string SourceMoveFolder = "Assets/";
    public string DestinationMoveFolder = "Assets/MovedAssets/";
    public string FileNameContainsForMove = "";
    public string FileNameExcludesForMove = "";
    public bool CreateSubfolder = true;
    public string SubfolderNamePrefix = "";

    public string DeleteTargetFolder = "Assets/";
    public string FileNameContainsForDelete = "";
    public string FileNameExcludesForDelete = "";
    public bool IncludeSubfoldersForDelete = true;

    private const string SETTINGS_PATH = "Assets/Editor/AssetAutomation/AssetAutomationSettings.asset";

    private static AssetAutomationSettings LoadOrCreateSettings()
    {
        AssetAutomationSettings settings = AssetDatabase.LoadAssetAtPath<AssetAutomationSettings>(SETTINGS_PATH);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<AssetAutomationSettings>();
            if (!AssetDatabase.IsValidFolder("Assets/Editor/AssetAutomation"))
            {
                AssetDatabase.CreateFolder("Assets/Editor", "AssetAutomation");
            }
            AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AssetAutomation] 새로운 설정 파일 생성: " + SETTINGS_PATH);
        }
        return settings;
    }

    public void SaveSettings()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[AssetAutomation] 설정이 저장되었습니다.");
    }

    public void LoadSettings()
    {
        // Settings are automatically loaded when the ScriptableObject is accessed.
        // We ensure data integrity on load/access if needed.
        TargetFolders = TargetFolders.Where(AssetDatabase.IsValidFolder).ToList();
        CustomExtensions = CustomExtensions.Select(ext => ext.ToLower()).Distinct().ToList();
    }

    public void AddCustomExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            EditorUtility.DisplayDialog("확장자 오류", "확장자를 입력해주세요.", "확인");
            return;
        }

        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }

        extension = extension.ToLower();
        if (CustomExtensions.Contains(extension) || new[] { ".png", ".jpg", ".fbx", ".wav", ".mp3", ".ogg" }.Contains(extension))
        {
            EditorUtility.DisplayDialog("확장자 오류", $"확장자 '{extension}'는 이미 존재하거나 기본 확장자입니다.", "확인");
            return;
        }

        CustomExtensions.Add(extension);
        SaveSettings();
        Debug.Log($"[AssetAutomation] 사용자 정의 확장자 추가됨: {extension}");
    }

    public void RemoveCustomExtension(string extension)
    {
        if (CustomExtensions.Contains(extension))
        {
            CustomExtensions.Remove(extension);
            
            if (SelectedExtension == extension)
            {
                SelectedExtension = ".png";
            }
            SaveSettings();
            Debug.Log($"[AssetAutomation] 사용자 정의 확장자 제거됨: {extension}");
        }
    }

    public void ClearAllSettings()
    {
        SelectedExtension = ".png";
        TargetFolders.Clear();
        CustomExtensions.Clear();

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
        Debug.Log("[AssetAutomation] 모든 설정이 초기화되었습니다.");
    }
}