using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SJW
{
    public class FolderManagementService
    {
        private readonly AssetAutomationSettings settings;

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
                path = Path.Combine("Assets", path.TrimStart('/')).Replace('\\', '/');
            path = path.TrimEnd('/');

            if (settings.ExcludedFolders.Any(excluded => path.StartsWith(excluded + "/") || path == excluded))
            {
                EditorUtility.DisplayDialog("경고", $"'{path}'는 제외 폴더 목록에 포함되어 추가할 수 없습니다.", "확인");
                return;
            }

            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentFolder = Path.GetDirectoryName(path);
                string folderName = Path.GetFileName(path);
                if (string.IsNullOrEmpty(parentFolder) || !AssetDatabase.IsValidFolder(parentFolder) ||
                    settings.ExcludedFolders.Any(excluded =>
                        parentFolder.StartsWith(excluded + "/") || parentFolder == excluded))
                {
                    EditorUtility.DisplayDialog("경로 오류", "유효한 상위 폴더가 없거나 Assets 폴더 내에 있지 않거나 제외 폴더에 포함되어 있습니다.", "확인");
                    return;
                }

                AssetDatabase.CreateFolder(parentFolder, folderName);
            }

            if (settings.TargetFolders.Count > 0 && settings.TargetFolders[0] == path)
                return;

            settings.TargetFolders.Clear();
            settings.TargetFolders.Add(path);
            settings.SaveSettings();
        }

        public void RemoveFolderFromList(string path)
        {
            if (settings.TargetFolders.Remove(path))
                settings.SaveSettings();
        }

        public List<string> GetAllProjectFolders()
        {
            List<string> allAssetsFolders = new List<string>();
            string[] allGuids = AssetDatabase.FindAssets("t:Folder", new[] { "Assets" });
            foreach (string guid in allGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path) &&
                    !settings.ExcludedFolders.Any(excluded => path.StartsWith(excluded + "/") || path == excluded))
                    allAssetsFolders.Add(path);
            }

            return allAssetsFolders.OrderBy(p => p).ToList();
        }
    }
}