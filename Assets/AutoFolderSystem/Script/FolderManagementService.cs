using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FolderManagementService
{
    private AssetAutomationSettings settings;
    private const string EXCLUDE_FOLDER_NAME = "AutoFolderSystem"; 

    public FolderManagementService(AssetAutomationSettings settings)
    {
        this.settings = settings;
    }

    public void AddFolderToList(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("경로 오류", "폴더 경로를 입력해주세요.", "확인");
            return;
        }

        if (!path.StartsWith("Assets/"))
        {
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }
            else
            {
                path = Path.Combine("Assets", path.TrimStart('/')).Replace('\\', '/');
            }
        }
        path = path.TrimEnd('/');

        if (path.Contains($"Assets/{EXCLUDE_FOLDER_NAME}")) 
        {
            EditorUtility.DisplayDialog("경고", $"'{EXCLUDE_FOLDER_NAME}' 폴더는 인식되지 않도록 설정되어 추가할 수 없습니다.", "확인");
            return;
        }


        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentFolder = Path.GetDirectoryName(path);
            string folderName = Path.GetFileName(path);

            if (string.IsNullOrEmpty(parentFolder) || !AssetDatabase.IsValidFolder(parentFolder))
            {
                EditorUtility.DisplayDialog("경로 오류", "유효한 상위 폴더가 없거나 Assets 폴더 내에 있지 않습니다.", "확인");
                return;
            }

            AssetDatabase.CreateFolder(parentFolder, folderName);
            AssetDatabase.Refresh();
            Debug.Log($"[AssetAutomation] 폴더 생성됨: {path}");
        }

        if (settings.TargetFolders.Count > 0 && settings.TargetFolders[0] == path)
        {
            Debug.LogWarning($"[AssetAutomation] '{path}' 폴더는 이미 선택된 폴더입니다.");
            return;
        }

        settings.TargetFolders.Clear();
        settings.TargetFolders.Add(path);
        settings.SaveSettings();
        Debug.Log($"[AssetAutomation] 선택된 폴더: {path}");
    }

    public void RemoveFolderFromList(string path)
    {
        if (settings.TargetFolders.Contains(path))
        {
            if (EditorUtility.DisplayDialog("🚨 경고: 실제 폴더 영구 삭제 🚨",
                                            $"'{path}' 폴더와 그 안의 모든 내용이 실제 프로젝트에서 영구적으로 삭제됩니다.\n\n이 작업은 되돌릴 수 없습니다. 계속하시겠습니까?",
                                            "예, 영구 삭제", "아니오"))
            {
                bool deletedAsset = AssetDatabase.DeleteAsset(path);
                if (deletedAsset)
                {
                    settings.TargetFolders.Remove(path);
                    settings.SaveSettings();
                    Debug.Log($"[AssetAutomation] 폴더 및 내용이 실제 프로젝트에서 제거됨: {path}");
                }
                else
                {
                    EditorUtility.DisplayDialog("오류", $"폴더 '{path}'를 삭제할 수 없습니다. 열려있는 파일이 있는지 확인하거나 권한을 확인하십시오.", "확인");
                    Debug.LogError($"[AssetAutomation] 실제 폴더 삭제 실패: {path}");
                }
            }
            else
            {
                Debug.Log($"[AssetAutomation] 폴더 '{path}' 삭제가 취소되었습니다.");
            }
        }
    }

    public List<string> GetAllProjectFolders()
    {
        List<string> allAssetsFolders = new List<string>();
        string[] allGuids = AssetDatabase.FindAssets("t:Folder", new[] { "Assets" });
        foreach (string guid in allGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            if (path.Contains($"Assets/{EXCLUDE_FOLDER_NAME}") || path == $"Assets/{EXCLUDE_FOLDER_NAME}")
            {
                continue;
            }

            if (AssetDatabase.IsValidFolder(path))
            {
                allAssetsFolders.Add(path);
            }
        }
        return allAssetsFolders.OrderBy(p => p).ToList();
    }
}