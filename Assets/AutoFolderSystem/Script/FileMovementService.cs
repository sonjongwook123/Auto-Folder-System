using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SJW
{
    public class FileMovementService
    {
        private readonly AssetAutomationSettings settings;

        public FileMovementService(AssetAutomationSettings settings)
        {
            this.settings = settings;
        }

        public void BatchMoveFiles()
        {
            if (string.IsNullOrEmpty(settings.SourceMoveFolder) || string.IsNullOrEmpty(settings.DestinationMoveFolder))
            {
                EditorUtility.DisplayDialog("오류", "원본 폴더와 대상 폴더를 모두 지정해야 합니다.", "확인");
                return;
            }

            if (!AssetDatabase.IsValidFolder(settings.SourceMoveFolder) || !AssetDatabase.IsValidFolder(settings.DestinationMoveFolder))
            {
                EditorUtility.DisplayDialog("오류", "유효하지 않은 폴더 경로입니다.", "확인");
                return;
            }

            string[] includeFilters = string.IsNullOrEmpty(settings.FileNameContainsForMove)
                ? new string[0]
                : settings.FileNameContainsForMove.Split(',').Select(f => f.Trim().ToLower()).ToArray();
            string[] excludeFilters = string.IsNullOrEmpty(settings.FileNameExcludesForMove)
                ? new string[0]
                : settings.FileNameExcludesForMove.Split(',').Select(f => f.Trim().ToLower()).ToArray();

            string[] guids = AssetDatabase.FindAssets("", new[] { settings.SourceMoveFolder });
            string destinationFolder = settings.DestinationMoveFolder;
            if (settings.CreateSubfolder && !string.IsNullOrEmpty(settings.SubfolderNamePrefix))
            {
                destinationFolder = Path.Combine(destinationFolder, settings.SubfolderNamePrefix).Replace('\\', '/');
                if (!AssetDatabase.IsValidFolder(destinationFolder))
                    AssetDatabase.CreateFolder(settings.DestinationMoveFolder, settings.SubfolderNamePrefix);
            }

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
                bool matchesInclude = includeFilters.Length == 0 || includeFilters.Any(f => fileName.Contains(f));
                bool matchesExclude = excludeFilters.Any(f => fileName.Contains(f));

                if (matchesInclude && !matchesExclude && !string.IsNullOrEmpty(assetPath) && File.Exists(assetPath))
                {
                    string newPath = Path.Combine(destinationFolder, Path.GetFileName(assetPath)).Replace('\\', '/');
                    if (newPath != assetPath)
                    {
                        string error = AssetDatabase.MoveAsset(assetPath, newPath);
                        if (!string.IsNullOrEmpty(error))
                            Debug.LogError($"Failed to move {assetPath} to {newPath}: {error}");
                    }
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", "파일 이동 작업이 완료되었습니다.", "확인");
        }

        public void BatchRenameFiles()
        {
            if (string.IsNullOrEmpty(settings.RenameSourceFolder))
            {
                EditorUtility.DisplayDialog("오류", "원본 폴더를 지정해야 합니다.", "확인");
                return;
            }

            if (!AssetDatabase.IsValidFolder(settings.RenameSourceFolder))
            {
                EditorUtility.DisplayDialog("오류", "유효하지 않은 원본 폴더 경로입니다.", "확인");
                return;
            }

            if (string.IsNullOrEmpty(settings.RenameOldString) || string.IsNullOrEmpty(settings.RenameNewString))
            {
                EditorUtility.DisplayDialog("오류", "변경 전 문자열과 변경 후 문자열을 모두 입력해야 합니다.", "확인");
                return;
            }

            string[] includeFilters = string.IsNullOrEmpty(settings.RenameFileNameContains)
                ? new string[0]
                : settings.RenameFileNameContains.Split(',').Select(f => f.Trim().ToLower()).ToArray();
            string[] excludeFilters = string.IsNullOrEmpty(settings.RenameFileNameExcludes)
                ? new string[0]
                : settings.RenameFileNameExcludes.Split(',').Select(f => f.Trim().ToLower()).ToArray();

            string[] guids = AssetDatabase.FindAssets("", new[] { settings.RenameSourceFolder });
            int renameCount = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (settings.ExcludedFolders.Any(excluded => assetPath.StartsWith(excluded + "/")))
                    continue;

                string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
                bool matchesInclude = includeFilters.Length == 0 || includeFilters.Any(f => fileName.Contains(f));
                bool matchesExclude = excludeFilters.Any(f => fileName.Contains(f));

                if (matchesInclude && !matchesExclude && !string.IsNullOrEmpty(assetPath) && File.Exists(assetPath))
                {
                    string newFileName = Path.GetFileNameWithoutExtension(assetPath).Replace(settings.RenameOldString, settings.RenameNewString);
                    string extension = Path.GetExtension(assetPath);
                    string parentFolder = Path.GetDirectoryName(assetPath);
                    string newPath = Path.Combine(parentFolder, newFileName + extension).Replace('\\', '/');

                    if (newPath != assetPath && !AssetDatabase.IsValidFolder(newPath) && !File.Exists(newPath))
                    {
                        string error = AssetDatabase.RenameAsset(assetPath, newFileName);
                        if (string.IsNullOrEmpty(error))
                            renameCount++;
                        else
                            Debug.LogError($"Failed to rename {assetPath} to {newPath}: {error}");
                    }
                    else if (newPath != assetPath)
                    {
                        Debug.LogWarning($"Skipped renaming {assetPath} to {newPath}: Target path already exists.");
                    }
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", $"파일 이름 변경 작업이 완료되었습니다. {renameCount}개의 파일이 변경되었습니다.", "확인");
        }

        public void BatchAddPrefixSuffix()
        {
            if (string.IsNullOrEmpty(settings.RenamePrefix) && string.IsNullOrEmpty(settings.RenameSuffix))
            {
                EditorUtility.DisplayDialog("오류", "접두사 또는 접미사를 입력해야 합니다.", "확인");
                return;
            }

            if (settings.SelectedFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("오류", "변경할 파일을 선택해야 합니다.", "확인");
                return;
            }

            int renameCount = 0;
            foreach (string assetPath in settings.SelectedFiles.ToList())
            {
                if (!File.Exists(assetPath))
                    continue;

                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                string extension = Path.GetExtension(assetPath);
                string parentFolder = Path.GetDirectoryName(assetPath);
                string newFileName = (settings.RenamePrefix ?? "") + fileName + (settings.RenameSuffix ?? "");
                string newPath = Path.Combine(parentFolder, newFileName + extension).Replace('\\', '/');

                if (newPath != assetPath && !AssetDatabase.IsValidFolder(newPath) && !File.Exists(newPath))
                {
                    string error = AssetDatabase.RenameAsset(assetPath, newFileName);
                    if (string.IsNullOrEmpty(error))
                    {
                        renameCount++;
                        settings.SelectedFiles.Remove(assetPath);
                        settings.SelectedFiles.Add(newPath);
                    }
                    else
                    {
                        Debug.LogError($"Failed to rename {assetPath} to {newPath}: {error}");
                    }
                }
                else if (newPath != assetPath)
                {
                    Debug.LogWarning($"Skipped renaming {assetPath} to {newPath}: Target path already exists.");
                }
            }

            settings.SaveSettings();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", $"파일 이름 변경 작업이 완료되었습니다. {renameCount}개의 파일이 변경되었습니다.", "확인");
        }
    }
}