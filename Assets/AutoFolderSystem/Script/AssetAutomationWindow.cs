// AssetAutomationWindow.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AssetAutomationWindow : EditorWindow
{
    private AssetAutomationSettings settings;
    private AssetProcessingService assetProcessor;
    private FolderManagementService folderManager;
    private FileMovementService fileMover;
    private FileDeletionService fileDeleter;

    private string newFolderPath = "Assets/"; 
    private string customExtensionInput = "";

    private Vector2 scrollPos;
    private Vector2 allFoldersScrollPos; 
    private int selectedTabIndex = 0;

    private Texture2D bannerImage;

    [MenuItem("Tools/Asset Automation Tool")]
    public static void ShowWindow()
    {
        GetWindow<AssetAutomationWindow>("Asset Automation");
    }

    private void OnEnable()
    {
        settings = AssetAutomationSettings.Instance;
        settings.LoadSettings();

        assetProcessor = new AssetProcessingService();
        folderManager = new FolderManagementService(settings);
        fileMover = new FileMovementService(settings);
        fileDeleter = new FileDeletionService(settings);

        bannerImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor Default Resources/banner_500x100.png");
        if (bannerImage == null)
        {
            Debug.LogWarning("[AssetAutomation] 배너 이미지를 찾을 수 없습니다. 'Assets/Editor Default Resources/banner_500x100.png' 경로에 500x100 크기의 PNG 파일을 넣어주세요.");
        }
    }

    void OnGUI()
    {
        // Draw the banner image at the top
        if (bannerImage != null)
        {
            Rect bannerRect = GUILayoutUtility.GetRect(500, 100, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(bannerRect, bannerImage, ScaleMode.ScaleAndCrop);
            EditorGUILayout.Space(10); // Add some space after the banner
        }

        GUILayout.Label("유니티 애셋 자동화 도구", EditorStyles.largeLabel);
        EditorGUILayout.Space();

        string[] tabNames = { "파일 일괄 정리", "파일 일괄 이동", "확장자 관리", "파일 일괄 제거", "폴더 정리" };
        selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames);
        EditorGUILayout.Space();

        switch (selectedTabIndex)
        {
            case 0:
                DrawBatchProcessingTab();
                break;
            case 1:
                DrawBatchMovementTab();
                break;
            case 2:
                DrawExtensionManagementTab();
                break;
            case 3:
                DrawBatchDeletionTab();
                break;
            case 4: // New tab index for folder organization
                DrawFolderOrganizationTab();
                break;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("모든 설정 초기화", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("설정 초기화", "모든 저장된 설정을 초기화하시겠습니까?", "예", "아니오"))
            {
                settings.ClearAllSettings();
                Debug.Log("[AssetAutomation] 모든 설정이 초기화되었습니다.");
            }
        }
    }

    private void DrawBatchProcessingTab()
    {
        DrawExtensionSelectionSection();
        EditorGUILayout.Space();

        GUILayout.Label("2. 처리할 폴더 (관리 탭에서 수정)", EditorStyles.boldLabel);
        if (settings.TargetFolders.Count == 0)
        {
            EditorGUILayout.HelpBox("처리할 폴더가 없습니다. '폴더 정리' 탭에서 폴더를 추가하거나 선택해주세요.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox($"현재 선택된 폴더: {settings.TargetFolders[0]}\n\n폴더를 변경하거나 추가하려면 '폴더 정리' 탭을 이용해주세요.", MessageType.Info);
        }
        EditorGUILayout.Space();

        DrawAllFoldersListSection();
        EditorGUILayout.Space();

        DrawProcessButton();
    }

    private void DrawExtensionManagementTab()
    {
        GUILayout.Label("확장자 관리", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("여기서 사용자 정의 확장자를 추가하거나 제거할 수 있습니다.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        {
            customExtensionInput = EditorGUILayout.TextField("새 확장자 (예: .tga)", customExtensionInput);
            if (GUILayout.Button("확장자 추가", GUILayout.Width(100)))
            {
                settings.AddCustomExtension(customExtensionInput);
                customExtensionInput = ""; 
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUILayout.Label("사용자 정의 확장자 목록:", EditorStyles.boldLabel);

        if (settings.CustomExtensions.Count == 0)
        {
            EditorGUILayout.HelpBox("추가된 사용자 정의 확장자가 없습니다.", MessageType.Warning);
        }
        else
        {
            for (int i = 0; i < settings.CustomExtensions.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(settings.CustomExtensions[i]);
                    if (GUILayout.Button("제거", GUILayout.Width(50)))
                    {
                        settings.RemoveCustomExtension(settings.CustomExtensions[i]);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void DrawBatchMovementTab()
    {
        GUILayout.Label("파일 일괄 이동", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("특정 이름을 포함하거나 제외하는 파일을 선택된 폴더로 일괄 이동합니다.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        {
            settings.SourceMoveFolder = EditorGUILayout.TextField("원본 폴더 경로", settings.SourceMoveFolder);
            if (GUILayout.Button("폴더 선택", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("원본 폴더 선택", settings.SourceMoveFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    settings.SourceMoveFolder = ConvertToAssetsPath(path);
                    settings.SaveSettings();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            settings.DestinationMoveFolder = EditorGUILayout.TextField("대상 폴더 경로", settings.DestinationMoveFolder);
            if (GUILayout.Button("폴더 선택", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("대상 폴더 선택", settings.DestinationMoveFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    settings.DestinationMoveFolder = ConvertToAssetsPath(path);
                    settings.SaveSettings();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        settings.FileNameContainsForMove = EditorGUILayout.TextField("파일 이름 포함 (쉼표로 구분)", settings.FileNameContainsForMove);
        settings.FileNameExcludesForMove = EditorGUILayout.TextField("파일 이름 제외 (쉼표로 구분)", settings.FileNameExcludesForMove);

        settings.CreateSubfolder = EditorGUILayout.Toggle("하위 폴더 생성", settings.CreateSubfolder);
        if (settings.CreateSubfolder)
        {
            settings.SubfolderNamePrefix = EditorGUILayout.TextField("하위 폴더 이름 접두사", settings.SubfolderNamePrefix);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("파일 일괄 이동 시작", GUILayout.Height(40)))
        {
            fileMover.BatchMoveFiles();
        }
    }

    private void DrawBatchDeletionTab()
    {
        GUILayout.Label("파일 일괄 제거", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("특정 이름을 포함하거나 제외하는 파일을 선택된 폴더에서 제거합니다. 이 작업은 되돌릴 수 없습니다!", MessageType.Warning);

        EditorGUILayout.BeginHorizontal();
        {
            settings.DeleteTargetFolder = EditorGUILayout.TextField("대상 폴더 경로", settings.DeleteTargetFolder);
            if (GUILayout.Button("폴더 선택", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("파일을 제거할 폴더 선택", settings.DeleteTargetFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    settings.DeleteTargetFolder = ConvertToAssetsPath(path);
                    settings.SaveSettings();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        settings.IncludeSubfoldersForDelete = EditorGUILayout.Toggle("하위 폴더 포함", settings.IncludeSubfoldersForDelete);

        settings.FileNameContainsForDelete = EditorGUILayout.TextField("파일 이름 포함 (쉼표로 구분)", settings.FileNameContainsForDelete);
        settings.FileNameExcludesForDelete = EditorGUILayout.TextField("파일 이름 제외 (쉼표로 구분)", settings.FileNameExcludesForDelete);

        EditorGUILayout.Space();

        if (GUILayout.Button("🚨 파일 일괄 제거 시작 🚨", GUILayout.Height(40)))
        {
            fileDeleter.BatchDeleteFiles();
        }
    }

    // New Tab: Folder Organization
    private void DrawFolderOrganizationTab()
    {
        GUILayout.Label("폴더 정리 (애셋 처리 대상 폴더 관리)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("여기서는 '파일 일괄 정리' 탭에서 처리할 폴더 목록을 보고 삭제할 수 있습니다. 폴더를 추가하려면, '파일 일괄 정리' 탭 하단의 '프로젝트 내 기존 폴더 선택' 섹션을 이용해야 합니다.", MessageType.Info); // Updated help text

        GUILayout.Label("현재 선택된 폴더:", EditorStyles.boldLabel);
        if (settings.TargetFolders.Count == 0)
        {
            EditorGUILayout.HelpBox("처리할 폴더가 없습니다. '파일 일괄 정리' 탭에서 폴더를 추가해주세요.", MessageType.Warning); 
        }
        else
        {
            GUIStyle redButtonStyle = new GUIStyle(GUI.skin.button);
            redButtonStyle.normal.background = MakeTex(2, 2, new Color(0.8f, 0.2f, 0.2f));
            redButtonStyle.normal.textColor = Color.white;
            redButtonStyle.hover.background = MakeTex(2, 2, new Color(0.9f, 0.3f, 0.3f));
            redButtonStyle.active.background = MakeTex(2, 2, new Color(0.7f, 0.1f, 0.1f));
            redButtonStyle.fontStyle = FontStyle.Bold;
            redButtonStyle.padding = new RectOffset(5, 5, 3, 3);
            redButtonStyle.margin = new RectOffset(0, 0, 2, 2);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(Mathf.Min(settings.TargetFolders.Count * 25 + 5, 100)));
            for (int i = 0; i < settings.TargetFolders.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(settings.TargetFolders[i], EditorStyles.miniLabel);

                    if (AssetDatabase.IsValidFolder(settings.TargetFolders[i]))
                    {
                        GUIContent iconContent = EditorGUIUtility.IconContent("Folder Icon");
                        GUILayout.Label(iconContent, GUILayout.Width(20), GUILayout.Height(16));
                    }
                    else
                    {
                        GUIContent iconContent = EditorGUIUtility.IconContent("Error");
                        GUILayout.Label(iconContent, GUILayout.Width(20), GUILayout.Height(16));
                        EditorGUILayout.HelpBox("경로가 유효하지 않습니다!", MessageType.Error);
                    }

                    if (GUILayout.Button("삭제", redButtonStyle, GUILayout.Width(50)))
                    {
                        if (EditorUtility.DisplayDialog("폴더 삭제 확인", $"'{settings.TargetFolders[i]}' 폴더와 그 내용이 실제 프로젝트에서 삭제됩니다. 이 작업은 되돌릴 수 없습니다. 계속하시겠습니까?", "예, 삭제합니다", "아니오"))
                        {
                            folderManager.RemoveFolderFromList(settings.TargetFolders[i]); 
                            break;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space();
    }

    private string ConvertToAssetsPath(string fullPath)
    {
        if (fullPath.StartsWith(Application.dataPath))
        {
            return "Assets" + fullPath.Substring(Application.dataPath.Length);
        }
        else
        {
            EditorUtility.DisplayDialog("경로 오류", "유니티 프로젝트의 Assets 폴더 내의 경로를 선택해주세요.", "확인");
            return "";
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void DrawExtensionSelectionSection()
    {
        GUILayout.Label("1. 애셋 확장명 선택", EditorStyles.boldLabel);

        List<string> allExtensions = new List<string> { ".png", ".jpg", ".fbx", ".wav", ".mp3", ".ogg" };
        allExtensions.AddRange(settings.CustomExtensions);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.4f, 0.8f));
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.background = MakeTex(2, 2, new Color(0.3f, 0.5f, 0.9f));
        buttonStyle.active.background = MakeTex(2, 2, new Color(0.1f, 0.3f, 0.7f));
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.padding = new RectOffset(10, 10, 5, 5);

        int buttonsPerRow = 5;
        for (int i = 0; i < allExtensions.Count; i += buttonsPerRow)
        {
            EditorGUILayout.BeginHorizontal();
            {
                for (int j = i; j < i + buttonsPerRow && j < allExtensions.Count; j++)
                {
                    string ext = allExtensions[j];
                    GUI.enabled = (settings.SelectedExtension != ext);
                    if (GUILayout.Button($"{ext.ToUpper()} 파일 ({ext})", buttonStyle, GUILayout.Height(30), GUILayout.MinWidth(100)))
                    {
                        settings.SelectedExtension = ext;
                        settings.SaveSettings();
                    }
                }
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.HelpBox($"현재 선택된 확장명: {settings.SelectedExtension}", MessageType.Info);
    }

    private void DrawAllFoldersListSection()
    {
        GUILayout.Label("3. 프로젝트 내 기존 폴더 선택", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("프로젝트의 Assets 폴더 내 모든 하위 폴더 목록입니다. 버튼을 클릭하여 선택된 폴더로 설정할 수 있습니다. (하나만 선택 가능)", MessageType.Info);

        List<string> allAssetsFolders = folderManager.GetAllProjectFolders();

        GUIStyle defaultButtonStyle = new GUIStyle(GUI.skin.button);
        defaultButtonStyle.fontStyle = FontStyle.Bold;
        defaultButtonStyle.padding = new RectOffset(5, 5, 3, 3);
        defaultButtonStyle.margin = new RectOffset(0, 0, 2, 2);

        if (allAssetsFolders.Count == 0)
        {
            EditorGUILayout.HelpBox("프로젝트 내에 폴더가 없습니다.", MessageType.Info);
        }
        else
        {
            allFoldersScrollPos = EditorGUILayout.BeginScrollView(allFoldersScrollPos, GUILayout.Height(200));
            foreach (string folderPath in allAssetsFolders)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(folderPath);
                    GUI.enabled = !(settings.TargetFolders.Count > 0 && settings.TargetFolders[0] == folderPath);
                    if (GUILayout.Button("선택", defaultButtonStyle, GUILayout.Width(50)))
                    {
                        folderManager.AddFolderToList(folderPath);
                    }
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawProcessButton()
    {
        GUI.enabled = settings.TargetFolders.Count > 0;
        if (GUILayout.Button($"선택된 폴더의 {settings.SelectedExtension} 애셋 일괄 처리 시작", GUILayout.Height(50)))
        {
            assetProcessor.ProcessAssetsInSelectedFolders(settings.TargetFolders, settings.SelectedExtension);
        }
        GUI.enabled = true;
    }
}