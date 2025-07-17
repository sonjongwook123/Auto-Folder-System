using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SJW
{
    public class AssetAutomationWindow : EditorWindow
    {
        private AssetAutomationSettings settings;
        private AssetProcessingService assetProcessor;
        private FolderManagementService folderManager;
        private FileMovementService fileMover;
        private FileDeletionService fileDeleter;
        private string newFolderPath = "Assets/";
        private string customExtensionInput = "";
        private string excludedFolderInput = "";
        private Vector2 scrollPos;
        private Vector2 allFoldersScrollPos;
        private Vector2 renameFilesScrollPos;
        private int selectedTabIndex;
        private Texture2D bannerImage;
        private Texture2D blueButtonTex;
        private Texture2D blueButtonHoverTex;
        private Texture2D blueButtonActiveTex;
        private string renamingFolderPath;
        private string newFolderName;
        private string pendingRenameOldPath;
        private string pendingRenameNewPath;

        [MenuItem("Tools/Auto Folder&File System")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetAutomationWindow>("Auto Folder&File System");
            window.minSize = new Vector2(660, 950);
            window.maxSize = new Vector2(800, 2000);
            window.Show();
        }

        private void OnEnable()
        {
            settings = AssetAutomationSettings.Instance;
            assetProcessor = new AssetProcessingService();
            folderManager = new FolderManagementService(settings);
            fileMover = new FileMovementService(settings);
            fileDeleter = new FileDeletionService(settings);

            string[] guids = AssetDatabase.FindAssets("t:Folder Editor Default Resources", new[] { "Assets" });
            string editorDefaultResourcesPath = null;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(path) == "Editor Default Resources")
                {
                    editorDefaultResourcesPath = path;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(editorDefaultResourcesPath))
            {
                string bannerPath = Path.Combine(editorDefaultResourcesPath, "banner_500x100.png").Replace('\\', '/');
                bannerImage = AssetDatabase.LoadAssetAtPath<Texture2D>(bannerPath);
            }

            blueButtonTex = MakeTex(2, 2, new Color(0.2f, 0.4f, 0.8f));
            blueButtonHoverTex = MakeTex(2, 2, new Color(0.3f, 0.5f, 0.9f));
            blueButtonActiveTex = MakeTex(2, 2, new Color(0.1f, 0.3f, 0.7f));
        }

        private void OnGUI()
        {
            if (bannerImage != null)
            {
                Rect bannerRect = GUILayoutUtility.GetRect(500, 100, GUILayout.ExpandWidth(true));
                GUI.DrawTexture(bannerRect, bannerImage, ScaleMode.ScaleAndCrop);
                EditorGUILayout.Space(10);
            }

            GUILayout.Label("유니티 애셋 자동화 도구", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, new[] { "파일 일괄 정리", "특정 파일 이동", "확장자 관리", "파일 일괄 제거", "폴더 관리", "제외 폴더", "파일명 검색/변경" });
            EditorGUILayout.Space();

            switch (selectedTabIndex)
            {
                case 0: DrawBatchProcessingTab(); break;
                case 1: DrawBatchMovementTab(); break;
                case 2: DrawExtensionManagementTab(); break;
                case 3: DrawBatchDeletionTab(); break;
                case 4: DrawFolderOrganizationTab(); break;
                case 5: DrawExcludedFoldersTab(); break;
                case 6: DrawFileSearchRenameTab(); break;
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("모든 설정 초기화", GUILayout.Height(30)) && EditorUtility.DisplayDialog("설정 초기화", "모든 저장된 설정을 초기화하시겠습니까?", "예", "아니오"))
                settings.ClearAllSettings();

            if (!string.IsNullOrEmpty(pendingRenameOldPath) && !string.IsNullOrEmpty(pendingRenameNewPath))
            {
                settings.TargetFolders.Remove(pendingRenameOldPath);
                settings.TargetFolders.Add(pendingRenameNewPath);
                settings.SaveSettings();
                AssetDatabase.Refresh();
                pendingRenameOldPath = null;
                pendingRenameNewPath = null;
            }
        }

        private void DrawBatchProcessingTab()
        {
            DrawExtensionSelectionSection();
            EditorGUILayout.Space();

            GUILayout.Label("2. 처리할 폴더 (관리 탭에서 수정)", EditorStyles.boldLabel);
            if (settings.TargetFolders.Count == 0)
                EditorGUILayout.HelpBox("처리할 폴더가 없습니다. '폴더 관리' 탭에서 폴더를 추가하거나 선택해주세요.", MessageType.Warning);
            else
                EditorGUILayout.HelpBox($"현재 선택된 폴더: {settings.TargetFolders[0]}\n\n폴더를 변경하거나 추가하려면 '폴더 관리' 탭을 이용해주세요.", MessageType.Info);
            EditorGUILayout.Space();

            DrawAllFoldersListSection();
            EditorGUILayout.Space();

            GUI.enabled = settings.TargetFolders.Count > 0;
            if (GUILayout.Button($"선택된 폴더의 {settings.SelectedExtension} 애셋 일괄 처리 시작", GUILayout.Height(50)))
                assetProcessor.ProcessAssetsInSelectedFolders(settings.TargetFolders, settings.SelectedExtension);
            GUI.enabled = true;
        }

        private void DrawExtensionSelectionSection()
        {
            GUILayout.Label("1. 애셋 확장명 선택", EditorStyles.boldLabel);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { background = blueButtonTex, textColor = Color.white },
                hover = { background = blueButtonHoverTex },
                active = { background = blueButtonActiveTex },
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(10, 10, 5, 5)
            };

            List<string> allExtensions = new List<string> { ".png", ".jpg", ".fbx", ".wav", ".mp3", ".ogg" };
            allExtensions.AddRange(settings.CustomExtensions);

            int buttonsPerRow = 5;
            for (int i = 0; i < allExtensions.Count; i += buttonsPerRow)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = i; j < i + buttonsPerRow && j < allExtensions.Count; j++)
                {
                    string ext = allExtensions[j];
                    GUI.enabled = settings.SelectedExtension != ext;
                    if (GUILayout.Button($"{ext.ToUpper()} 파일 ({ext})", buttonStyle, GUILayout.Height(30), GUILayout.MinWidth(100)))
                    {
                        settings.SelectedExtension = ext;
                        settings.SaveSettings();
                    }
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.HelpBox($"현재 선택된 확장명: {settings.SelectedExtension}", MessageType.Info);
        }

        private void DrawAllFoldersListSection()
        {
            GUILayout.Label("3. 프로젝트 내 기존 폴더 선택", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("프로젝트의 Assets 폴더 내 모든 하위 폴더 목록입니다. 버튼을 클릭하여 선택된 폴더로 설정할 수 있습니다. (하나만 선택 가능)", MessageType.Info);

            List<string> allAssetsFolders = folderManager.GetAllProjectFolders();
            if (allAssetsFolders.Count == 0)
                EditorGUILayout.HelpBox("프로젝트 내에 폴더가 없습니다.", MessageType.Info);
            else
            {
                allFoldersScrollPos = EditorGUILayout.BeginScrollView(allFoldersScrollPos, GUILayout.Height(200));
                foreach (string folderPath in allAssetsFolders)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(folderPath);
                    GUI.enabled = !(settings.TargetFolders.Count > 0 && settings.TargetFolders[0] == folderPath);
                    if (GUILayout.Button("선택", GUILayout.Width(50)))
                        folderManager.AddFolderToList(folderPath);
                    GUI.enabled = true;
                    if (GUILayout.Button("삭제", GUILayout.Width(50)))
                    {
                        if (EditorUtility.DisplayDialog("폴더 삭제", $"폴더 '{folderPath}'를 영구적으로 삭제하시겠습니까? 이 작업은 되돌릴 수 없습니다!", "예", "아니오"))
                        {
                            if (AssetDatabase.IsValidFolder(folderPath))
                                AssetDatabase.DeleteAsset(folderPath);
                            folderManager.RemoveFolderFromList(folderPath);
                            AssetDatabase.Refresh();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawBatchMovementTab()
        {
            GUILayout.Label("특정 파일 이동", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("특정 이름을 포함하거나 제외하는 파일을 선택된 폴더로 일괄 이동합니다.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            settings.SourceMoveFolder = EditorGUILayout.TextField("원본 폴더 경로", settings.SourceMoveFolder);
            if (GUILayout.Button("폴더 선택", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("원본 폴더 선택", settings.SourceMoveFolder, "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    settings.SourceMoveFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    settings.SaveSettings();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            settings.DestinationMoveFolder = EditorGUILayout.TextField("대상 폴더 경로", settings.DestinationMoveFolder);
            if (GUILayout.Button("폴더 선택", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("대상 폴더 선택", settings.DestinationMoveFolder, "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    settings.DestinationMoveFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    settings.SaveSettings();
                }
            }
            EditorGUILayout.EndHorizontal();

            settings.FileNameContainsForMove = EditorGUILayout.TextField("파일 이름 포함 (쉼표로 구분)", settings.FileNameContainsForMove);
            settings.FileNameExcludesForMove = EditorGUILayout.TextField("파일 이름 제외 (쉼표로 구분)", settings.FileNameExcludesForMove);
            settings.CreateSubfolder = EditorGUILayout.Toggle("하위 폴더 생성", settings.CreateSubfolder);
            if (settings.CreateSubfolder)
                settings.SubfolderNamePrefix = EditorGUILayout.TextField("하위 폴더 이름 접두사", settings.SubfolderNamePrefix);
            EditorGUILayout.Space();

            if (GUILayout.Button("파일 일괄 이동 시작", GUILayout.Height(40)))
                fileMover.BatchMoveFiles();
        }

        private void DrawBatchDeletionTab()
        {
            GUILayout.Label("파일 일괄 제거", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("특정 이름을 포함하거나 제외하는 파일을 선택된 폴더에서 제거합니다. 이 작업은 되돌릴 수 없습니다!", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            settings.DeleteTargetFolder = EditorGUILayout.TextField("대상 폴더 경로", settings.DeleteTargetFolder);
            if (GUILayout.Button("폴더 선택", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("파일을 제거할 폴더 선택", settings.DeleteTargetFolder, "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    settings.DeleteTargetFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    settings.SaveSettings();
                }
            }
            EditorGUILayout.EndHorizontal();

            settings.IncludeSubfoldersForDelete = EditorGUILayout.Toggle("하위 폴더 포함", settings.IncludeSubfoldersForDelete);
            settings.FileNameContainsForDelete = EditorGUILayout.TextField("파일 이름 포함 (쉼표로 구분)", settings.FileNameContainsForDelete);
            settings.FileNameExcludesForDelete = EditorGUILayout.TextField("파일 이름 제외 (쉼표로 구분)", settings.FileNameExcludesForDelete);
            EditorGUILayout.Space();

            if (GUILayout.Button("🚨 파일 일괄 제거 시작 🚨", GUILayout.Height(40)))
                fileDeleter.BatchDeleteFiles();
        }

        private void DrawFolderOrganizationTab()
        {
            GUILayout.Label("폴더 관리 (애셋 처리 대상 폴더 관리)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("여기서 '파일 일괄 정리' 탭에서 처리할 폴더를 추가하거나 이름을 변경할 수 있습니다.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            newFolderPath = EditorGUILayout.TextField("새 폴더 경로", newFolderPath);
            if (GUILayout.Button("폴더 선택", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("폴더 선택", newFolderPath, "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                    newFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
            if (GUILayout.Button("추가", GUILayout.Width(60)))
            {
                folderManager.AddFolderToList(newFolderPath);
                newFolderPath = "Assets/";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.Label("현재 선택된 폴더:", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(Mathf.Min(settings.TargetFolders.Count * 25 + 5, 100)));
            if (settings.TargetFolders.Count == 0)
                EditorGUILayout.HelpBox("처리할 폴더가 없습니다. 위에서 폴더를 추가하거나 하단에서 선택해주세요.", MessageType.Warning);
            else
            {
                foreach (string folderPath in settings.TargetFolders.ToList())
                {
                    EditorGUILayout.BeginHorizontal();
                    if (renamingFolderPath == folderPath)
                    {
                        newFolderName = EditorGUILayout.TextField(newFolderName);
                        if (GUILayout.Button("확인", GUILayout.Width(50)))
                        {
                            if (!string.IsNullOrEmpty(newFolderName))
                            {
                                string parentFolder = Path.GetDirectoryName(folderPath);
                                string newPath = Path.Combine(parentFolder, newFolderName).Replace('\\', '/');
                                if (AssetDatabase.IsValidFolder(newPath))
                                {
                                    EditorUtility.DisplayDialog("오류", $"폴더 '{newPath}'는 이미 존재합니다.", "확인");
                                }
                                else if (string.IsNullOrEmpty(newFolderName) || newFolderName.Contains("/") || newFolderName.Contains("\\"))
                                {
                                    EditorUtility.DisplayDialog("오류", "유효하지 않은 폴더 이름입니다.", "확인");
                                }
                                else
                                {
                                    string error = AssetDatabase.RenameAsset(folderPath, newFolderName);
                                    if (string.IsNullOrEmpty(error))
                                    {
                                        pendingRenameOldPath = folderPath;
                                        pendingRenameNewPath = newPath;
                                        renamingFolderPath = null;
                                        newFolderName = "";
                                    }
                                    else
                                    {
                                        EditorUtility.DisplayDialog("오류", $"폴더 이름 변경 실패: {error}", "확인");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(folderPath, EditorStyles.miniLabel);
                        if (AssetDatabase.IsValidFolder(folderPath))
                            GUILayout.Label(EditorGUIUtility.IconContent("Folder Icon"), GUILayout.Width(20), GUILayout.Height(16));
                        else
                            GUILayout.Label(EditorGUIUtility.IconContent("Error"), GUILayout.Width(20), GUILayout.Height(16));
                    }
                    if (GUILayout.Button(renamingFolderPath == folderPath ? "취소" : "이름 변경", GUILayout.Width(80)))
                    {
                        if (renamingFolderPath == folderPath)
                        {
                            renamingFolderPath = null;
                            newFolderName = "";
                        }
                        else
                        {
                            renamingFolderPath = folderPath;
                            newFolderName = Path.GetFileName(folderPath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            GUILayout.Label("프로젝트 내 기존 폴더:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("프로젝트의 Assets 폴더 내 모든 하위 폴더 목록입니다. 버튼을 클릭하여 선택된 폴더로 설정하거나 삭제할 수 있습니다. (하나만 선택 가능)", MessageType.Info);

            List<string> allAssetsFolders = folderManager.GetAllProjectFolders();
            allFoldersScrollPos = EditorGUILayout.BeginScrollView(allFoldersScrollPos, GUILayout.Height(200));
            if (allAssetsFolders.Count == 0)
                EditorGUILayout.HelpBox("프로젝트 내에 폴더가 없습니다.", MessageType.Info);
            else
            {
                foreach (string folderPath in allAssetsFolders)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(folderPath);
                    GUI.enabled = !(settings.TargetFolders.Count > 0 && settings.TargetFolders[0] == folderPath);
                    if (GUILayout.Button("선택", GUILayout.Width(50)))
                        folderManager.AddFolderToList(folderPath);
                    GUI.enabled = true;
                    if (GUILayout.Button("삭제", GUILayout.Width(50)))
                    {
                        if (EditorUtility.DisplayDialog("폴더 삭제", $"폴더 '{folderPath}'를 영구적으로 삭제하시겠습니까? 이 작업은 되돌릴 수 없습니다!", "예", "아니오"))
                        {
                            if (AssetDatabase.IsValidFolder(folderPath))
                                AssetDatabase.DeleteAsset(folderPath);
                            folderManager.RemoveFolderFromList(folderPath);
                            AssetDatabase.Refresh();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawExtensionManagementTab()
        {
            GUILayout.Label("확장자 관리", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("여기서 사용자 정의 확장자를 추가하거나 제거할 수 있습니다.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            customExtensionInput = EditorGUILayout.TextField("새 확장자 (예: .tga)", customExtensionInput);
            if (GUILayout.Button("확장자 추가", GUILayout.Width(100)))
            {
                settings.AddCustomExtension(customExtensionInput);
                customExtensionInput = "";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.Label("사용자 정의 확장자 목록:", EditorStyles.boldLabel);

            if (settings.CustomExtensions.Count == 0)
                EditorGUILayout.HelpBox("추가된 사용자 정의 확장자가 없습니다.", MessageType.Warning);
            else
            {
                foreach (string ext in settings.CustomExtensions.ToList()) // Use ToList() to avoid modifying collection during iteration
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(ext);
                    if (GUILayout.Button("제거", GUILayout.Width(50)))
                    {
                        settings.RemoveCustomExtension(ext);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawExcludedFoldersTab()
        {
            GUILayout.Label("제외 폴더", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("여기서 프로젝트 폴더 목록에서 제외할 폴더를 추가하거나 제거할 수 있습니다.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            excludedFolderInput = EditorGUILayout.TextField("제외할 폴더 경로", excludedFolderInput);
            if (GUILayout.Button("폴더 선택", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("제외할 폴더 선택", excludedFolderInput, "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                    excludedFolderInput = "Assets" + path.Substring(Application.dataPath.Length);
            }
            if (GUILayout.Button("추가", GUILayout.Width(60)))
            {
                settings.AddExcludedFolder(excludedFolderInput);
                excludedFolderInput = "";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.Label("현재 제외된 폴더:", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(Mathf.Min(settings.ExcludedFolders.Count * 25 + 5, 100)));
            if (settings.ExcludedFolders.Count == 0)
                EditorGUILayout.HelpBox("제외된 폴더가 없습니다.", MessageType.Info);
            else
            {
                foreach (string folderPath in settings.ExcludedFolders.ToList())
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(folderPath, EditorStyles.miniLabel);
                    if (AssetDatabase.IsValidFolder(folderPath))
                        GUILayout.Label(EditorGUIUtility.IconContent("Folder Icon"), GUILayout.Width(20), GUILayout.Height(16));
                    else
                        GUILayout.Label(EditorGUIUtility.IconContent("Error"), GUILayout.Width(20), GUILayout.Height(16));
                    if (GUILayout.Button("제거", GUILayout.Width(50)))
                    {
                        settings.RemoveExcludedFolder(folderPath);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawFileSearchRenameTab()
        {
            GUILayout.Label("파일명 검색/변경", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("파일을 검색하고, 선택된 파일의 이름을 일괄 변경하거나 접두사/접미사를 추가할 수 있습니다. 제외 폴더는 검색에서 제외됩니다.", MessageType.Info);

            // 파일 검색 섹션
            GUILayout.Label("1. 파일 검색", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            settings.RenameSourceFolder = EditorGUILayout.TextField("원본 폴더 경로", settings.RenameSourceFolder);
            if (GUILayout.Button("폴더 선택", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("원본 폴더 선택", settings.RenameSourceFolder, "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    settings.RenameSourceFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    settings.SaveSettings();
                }
            }
            EditorGUILayout.EndHorizontal();

            settings.RenameFileNameContains = EditorGUILayout.TextField("파일 이름 포함 (쉼표로 구분)", settings.RenameFileNameContains);
            settings.RenameFileNameExcludes = EditorGUILayout.TextField("파일 이름 제외 (쉼표로 구분)", settings.RenameFileNameExcludes);
            EditorGUILayout.Space();

            GUILayout.Label("검색된 파일 목록:", EditorStyles.boldLabel);
            if (string.IsNullOrEmpty(settings.RenameSourceFolder) || !AssetDatabase.IsValidFolder(settings.RenameSourceFolder))
            {
                EditorGUILayout.HelpBox("유효한 원본 폴더를 선택해주세요.", MessageType.Warning);
            }
            else
            {
                string[] includeFilters = string.IsNullOrEmpty(settings.RenameFileNameContains)
                    ? new string[0]
                    : settings.RenameFileNameContains.Split(',').Select(f => f.Trim().ToLower()).ToArray();
                string[] excludeFilters = string.IsNullOrEmpty(settings.RenameFileNameExcludes)
                    ? new string[0]
                    : settings.RenameFileNameExcludes.Split(',').Select(f => f.Trim().ToLower()).ToArray();

                string[] guids = AssetDatabase.FindAssets("", new[] { settings.RenameSourceFolder });
                List<string> matchedFiles = new List<string>();

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (settings.ExcludedFolders.Any(excluded => assetPath.StartsWith(excluded + "/")))
                        continue;

                    string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
                    bool matchesInclude = includeFilters.Length == 0 || includeFilters.Any(f => fileName.Contains(f));
                    bool matchesExclude = excludeFilters.Any(f => fileName.Contains(f));

                    if (matchesInclude && !matchesExclude && !string.IsNullOrEmpty(assetPath) && File.Exists(assetPath))
                        matchedFiles.Add(assetPath);
                }

                if (matchedFiles.Count == 0)
                    EditorGUILayout.HelpBox("조건에 맞는 파일이 없습니다.", MessageType.Info);
                else
                {
                    renameFilesScrollPos = EditorGUILayout.BeginScrollView(renameFilesScrollPos, GUILayout.Height(150));
                    foreach (string assetPath in matchedFiles)
                    {
                        EditorGUILayout.BeginHorizontal();
                        bool isSelected = settings.SelectedFiles.Contains(assetPath);
                        bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                        if (newSelected != isSelected)
                        {
                            if (newSelected)
                                settings.SelectedFiles.Add(assetPath);
                            else
                                settings.SelectedFiles.Remove(assetPath);
                            settings.SaveSettings();
                        }
                        EditorGUILayout.LabelField(assetPath, EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                }
            }

            EditorGUILayout.Space(20);

            // 파일명 일괄 변경 섹션
            GUILayout.Label("2. 파일명 일괄 변경", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            settings.RenameSourceFolder = EditorGUILayout.TextField("원본 폴더 경로", settings.RenameSourceFolder);
            if (GUILayout.Button("폴더 선택", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("원본 폴더 선택", settings.RenameSourceFolder, "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    settings.RenameSourceFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    settings.SaveSettings();
                }
            }
            EditorGUILayout.EndHorizontal();

            settings.RenameOldString = EditorGUILayout.TextField("변경 전 문자열", settings.RenameOldString);
            settings.RenameNewString = EditorGUILayout.TextField("변경 후 문자열", settings.RenameNewString);
            settings.RenameFileNameContains = EditorGUILayout.TextField("파일 이름 포함 (쉼표로 구분)", settings.RenameFileNameContains);
            settings.RenameFileNameExcludes = EditorGUILayout.TextField("파일 이름 제외 (쉼표로 구분)", settings.RenameFileNameExcludes);
            EditorGUILayout.Space();

            if (GUILayout.Button("파일 이름 일괄 변경 시작", GUILayout.Height(40)))
                fileMover.BatchRenameFiles();

            EditorGUILayout.Space(20);

            // 접두사/접미사 추가 섹션
            GUILayout.Label("3. 접두사/접미사 추가", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("선택된 파일에 접두사 또는 접미사를 추가합니다. 파일은 위의 '파일 검색' 섹션에서 선택해주세요.", MessageType.Info);

            settings.RenamePrefix = EditorGUILayout.TextField("접두사 추가", settings.RenamePrefix);
            settings.RenameSuffix = EditorGUILayout.TextField("접미사 추가", settings.RenameSuffix);
            EditorGUILayout.Space();

            GUILayout.Label("선택된 파일 목록:", EditorStyles.boldLabel);
            if (settings.SelectedFiles.Count == 0)
            {
                EditorGUILayout.HelpBox("선택된 파일이 없습니다. 위의 '파일 검색' 섹션에서 파일을 선택해주세요.", MessageType.Warning);
            }
            else
            {
                renameFilesScrollPos = EditorGUILayout.BeginScrollView(renameFilesScrollPos, GUILayout.Height(150));
                foreach (string assetPath in settings.SelectedFiles)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(assetPath, EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("선택한 파일에 접두사/접미사 추가 시작", GUILayout.Height(40)))
                fileMover.BatchAddPrefixSuffix();
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}