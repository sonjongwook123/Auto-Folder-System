using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SJW
{
    public class AssetProcessingService
    {
        public void ProcessAssetsInSelectedFolders(List<string> targetFolders, string selectedExtension)
        {
            if (targetFolders.Count == 0)
            {
                EditorUtility.DisplayDialog("경고", "처리할 폴더가 지정되지 않았습니다. '폴더 정리' 탭에서 폴더를 추가해주세요.", "확인");
                return;
            }

            EditorUtility.DisplayProgressBar("애셋 처리 중...", "애셋을 검색하고 있습니다...", 0.1f);
            int processedCount = 0;
            List<string> assetsToProcess = new List<string>();

            foreach (string folderPath in targetFolders)
            {
                string[] assetGuids = AssetDatabase.FindAssets($"t:Object", new[] { folderPath });
                foreach (string guid in assetGuids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!AssetDatabase.IsValidFolder(assetPath) &&
                        Path.GetExtension(assetPath).ToLower() == selectedExtension.ToLower())
                        assetsToProcess.Add(assetPath);
                }
            }

            if (assetsToProcess.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("정보", $"선택된 폴더에서 '{selectedExtension}' 확장자를 가진 애셋을 찾을 수 없습니다.", "확인");
                return;
            }

            for (int i = 0; i < assetsToProcess.Count; i++)
            {
                string assetPath = assetsToProcess[i];
                EditorUtility.DisplayProgressBar("애셋 처리 중...",
                    $"처리 중: {Path.GetFileName(assetPath)} ({i + 1}/{assetsToProcess.Count})",
                    (float)i / assetsToProcess.Count);

                bool changed = false;
                switch (selectedExtension)
                {
                    case ".png":
                    case ".jpg":
                        changed = ProcessTexture(assetPath);
                        break;
                    case ".fbx":
                        changed = ProcessModel(assetPath);
                        break;
                    case ".wav":
                    case ".mp3":
                    case ".ogg":
                        changed = ProcessAudio(assetPath);
                        break;
                    default:
                        changed = ProcessGenericAsset(assetPath);
                        break;
                }

                if (changed)
                    processedCount++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("처리 완료", $"{processedCount}개의 {selectedExtension} 애셋을 성공적으로 처리했습니다.", "확인");
        }

        private bool ProcessTexture(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return false;

            bool wasChanged = false;
            if (importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Single ||
                !importer.alphaIsTransparency ||
                importer.mipmapEnabled)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
                wasChanged = true;
            }

            return wasChanged;
        }

        private bool ProcessModel(string assetPath)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
                return false;

            bool wasChanged = false;
            if (importer.importAnimation)
            {
                importer.importAnimation = false;
                wasChanged = true;
            }

            if (importer.meshCompression != ModelImporterMeshCompression.Medium)
            {
                importer.meshCompression = ModelImporterMeshCompression.Medium;
                wasChanged = true;
            }

            if (wasChanged)
                importer.SaveAndReimport();
            return wasChanged;
        }

        private bool ProcessAudio(string assetPath)
        {
            AudioImporter importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if (importer == null)
                return false;

            bool wasChanged = false;
            AudioImporterSampleSettings currentSettings = importer.defaultSampleSettings;

            if (currentSettings.loadType != AudioClipLoadType.Streaming)
            {
                currentSettings.loadType = AudioClipLoadType.Streaming;
                importer.defaultSampleSettings = currentSettings;
                wasChanged = true;
            }

            if (importer.forceToMono != true)
            {
                importer.forceToMono = true;
                wasChanged = true;
            }

            if (currentSettings.compressionFormat != AudioCompressionFormat.Vorbis || currentSettings.quality != 0.5f)
            {
                currentSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                currentSettings.quality = 0.5f;
                importer.defaultSampleSettings = currentSettings;
                wasChanged = true;
            }

            if (wasChanged)
                importer.SaveAndReimport();
            return wasChanged;
        }

        private bool ProcessGenericAsset(string assetPath)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                importer.SaveAndReimport();
                return true;
            }

            return false;
        }
    }
}