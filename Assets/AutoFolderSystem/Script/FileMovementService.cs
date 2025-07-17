using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FileMovementService
{
    private readonly AssetAutomationSettings settings;

    public FileMovementService(AssetAutomationSettings settings)
    {
        this.settings = settings;
    }

    public void BatchMoveFiles()
    {
        if (string.IsNullOrEmpty(settings.SourceMoveFolder) || !AssetDatabase.IsValidFolder(settings.SourceMoveFolder))
        {
            EditorUtility.DisplayDialog("오류", "유효한 원본 폴더를 선택해주세요.", "확인");
            return;
        }

        if (string.IsNullOrEmpty(settings.DestinationMoveFolder) || !AssetDatabase.IsValidFolder(settings.DestinationMoveFolder))
        {
            string parentDir = Path.GetDirectoryName(settings.DestinationMoveFolder);
            string folderName = Path.GetFileName(settings.DestinationMoveFolder);
            if (!string.IsNullOrEmpty(parentDir) && AssetDatabase.IsValidFolder(parentDir) && EditorUtility.DisplayDialog("대상 폴더 없음", "대상 폴더가 존재하지 않습니다. 생성하시겠습니까?", "예", "아니오"))
            {
                AssetDatabase.CreateFolder(parentDir, folderName);
                AssetDatabase.Refresh();
            }
            else
            {
                EditorUtility.DisplayDialog("오류", "유효한 대상 폴더를 선택하거나 생성할 수 없습니다.", "확인");
                return;
            }
        }

        List<string> filesToMove = new List<string>();
        string[] guidsInSource = AssetDatabase.FindAssets("t:Object", new[] { settings.SourceMoveFolder });
        List<string> includeKeywords = settings.FileNameContainsForMove.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower()).ToList();
        List<string> excludeKeywords = settings.FileNameExcludesForMove.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower()).ToList();

        foreach (string guid in guidsInSource)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(assetPath))
                continue;

            string fileName = Path.GetFileName(assetPath).ToLower();
            bool include = true;
            if (includeKeywords.Any() && !includeKeywords.Any(keyword => fileName.Contains(keyword)))
                include = false;
            if (excludeKeywords.Any(keyword => fileName.Contains(keyword)))
                include = false;
            if (include)
                filesToMove.Add(assetPath);
        }

        if (filesToMove.Count == 0)
        {
            EditorUtility.DisplayDialog("정보", "이동할 파일을 찾을 수 없습니다.", "확인");
            return;
        }

        if (!EditorUtility.DisplayDialog("파일 이동 확인", $"{filesToMove.Count}개의 파일을 '{settings.DestinationMoveFolder}'로 이동하시겠습니까?", "예", "아니오"))
            return;

        int movedCount = 0;
        EditorUtility.DisplayProgressBar("파일 이동 중...", "파일을 이동하고 있습니다...", 0.1f);

        for (int i = 0; i < filesToMove.Count; i++)
        {
            string originalPath = filesToMove[i];
            string fileName = Path.GetFileName(originalPath);
            string targetSubfolder = settings.DestinationMoveFolder;

            if (settings.CreateSubfolder)
            {
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                string subfolder = string.IsNullOrEmpty(settings.SubfolderNamePrefix) ? baseName : $"{settings.SubfolderNamePrefix}_{baseName}";
                targetSubfolder = Path.Combine(settings.DestinationMoveFolder, subfolder).Replace('\\', '/');
                string parentDir = Path.GetDirectoryName(targetSubfolder);
                string newFolderName = Path.GetFileName(targetSubfolder);
                if (!AssetDatabase.IsValidFolder(targetSubfolder) && !string.IsNullOrEmpty(parentDir) && AssetDatabase.IsValidFolder(parentDir))
                    AssetDatabase.CreateFolder(parentDir, newFolderName);
            }

            string newPath = Path.Combine(targetSubfolder, fileName).Replace('\\', '/');
            if (File.Exists(Application.dataPath + newPath.Substring("Assets".Length)))
                continue;

            if (string.IsNullOrEmpty(AssetDatabase.MoveAsset(originalPath, newPath)))
                movedCount++;
            EditorUtility.DisplayProgressBar("파일 이동 중...", $"이동 중: {fileName} ({i + 1}/{filesToMove.Count})", (float)i / filesToMove.Count);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("이동 완료", $"{movedCount}개의 파일이 성공적으로 이동되었습니다.", "확인");
    }
}