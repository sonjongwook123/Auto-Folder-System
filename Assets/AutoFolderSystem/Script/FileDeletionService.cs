using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FileDeletionService
{
    private readonly AssetAutomationSettings settings;

    public FileDeletionService(AssetAutomationSettings settings)
    {
        this.settings = settings;
    }

    public void BatchDeleteFiles()
    {
        if (string.IsNullOrEmpty(settings.DeleteTargetFolder) || !AssetDatabase.IsValidFolder(settings.DeleteTargetFolder))
        {
            EditorUtility.DisplayDialog("오류", "유효한 대상 폴더를 선택해주세요.", "확인");
            return;
        }

        List<string> filesToDelete = new List<string>();
        string[] guidsInTarget = AssetDatabase.FindAssets("t:Object", new[] { settings.DeleteTargetFolder });
        List<string> includeKeywords = settings.FileNameContainsForDelete.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower()).ToList();
        List<string> excludeKeywords = settings.FileNameExcludesForDelete.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower()).ToList();

        foreach (string guid in guidsInTarget)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(assetPath))
                continue;

            if (!settings.IncludeSubfoldersForDelete && Path.GetDirectoryName(assetPath).Replace('\\', '/') != settings.DeleteTargetFolder.Replace('\\', '/'))
                continue;

            string fileName = Path.GetFileName(assetPath).ToLower();
            bool include = true;
            if (includeKeywords.Any() && !includeKeywords.Any(keyword => fileName.Contains(keyword)))
                include = false;
            if (excludeKeywords.Any(keyword => fileName.Contains(keyword)))
                include = false;
            if (include)
                filesToDelete.Add(assetPath);
        }

        if (filesToDelete.Count == 0)
        {
            EditorUtility.DisplayDialog("정보", "제거할 파일을 찾을 수 없습니다.", "확인");
            return;
        }

        if (!EditorUtility.DisplayDialog("🚨 파일 제거 확인 🚨", $"'{settings.DeleteTargetFolder}' 폴더 ({(settings.IncludeSubfoldersForDelete ? "및 하위 폴더" : "만")}) 에서 {filesToDelete.Count}개의 파일을 정말로 제거하시겠습니까?\n\n이 작업은 되돌릴 수 없습니다!", "예, 제거합니다", "아니오"))
            return;

        int deletedCount = 0;
        EditorUtility.DisplayProgressBar("파일 제거 중...", "파일을 제거하고 있습니다...", 0.1f);

        for (int i = 0; i < filesToDelete.Count; i++)
        {
            string assetPath = filesToDelete[i];
            EditorUtility.DisplayProgressBar("파일 제거 중...", $"제거 중: {Path.GetFileName(assetPath)} ({i + 1}/{filesToDelete.Count})", (float)i / filesToDelete.Count);
            if (AssetDatabase.DeleteAsset(assetPath))
                deletedCount++;
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("제거 완료", $"{deletedCount}개의 파일이 성공적으로 제거되었습니다.", "확인");
    }
}