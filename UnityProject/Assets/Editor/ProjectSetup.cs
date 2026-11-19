using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ExpeditionCamp.EditorTools
{
    public static class ProjectSetup
    {
        public const string SettingsFolder = "Assets/Settings";
        public const string GameConfigPath = SettingsFolder + "/GameConfig.asset";
        public const string QAConfigPath = SettingsFolder + "/QAConfig.asset";
        public const string PipelinePath = SettingsFolder + "/URP/ExpeditionCamp_URP.asset";
        public const string RendererPath = SettingsFolder + "/URP/ExpeditionCamp_Renderer.asset";
        public const string QaBridgeDefine = "QA_BRIDGE";

        public static readonly string[] RequiredTags = { "Monster", "DropItem" };
        public static readonly string[] RequiredLayers = { "Monster", "Player", "DropItem", "Ground", "Obstacle", "Hidden" };

        [MenuItem("ExpeditionCamp/Setup Project")]
        public static void Run()
        {
            ConfigurePlayer();
            ConfigureEditor();
            CheckTagsAndLayers();
            EnsureRenderPipeline();
            EnsureConfigAssets();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] 프로젝트 설정을 마쳤습니다");
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "302lab";
            PlayerSettings.productName = "ExpeditionCamp";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.runInBackground = true;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Unity_4_8);

            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            if (!defines.Contains(QaBridgeDefine))
            {
                defines = string.IsNullOrEmpty(defines) ? QaBridgeDefine : defines + ";" + QaBridgeDefine;
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, defines);
            }
        }

        private static void ConfigureEditor()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";
        }

        private static void CheckTagsAndLayers()
        {
            foreach (string tag in RequiredTags)
            {
                bool found = false;
                foreach (string existing in UnityEditorInternal.InternalEditorUtility.tags)
                {
                    if (existing == tag)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    Debug.LogError("[Setup] Tag 가 없습니다: " + tag);
                }
            }
            foreach (string layer in RequiredLayers)
            {
                if (LayerMask.NameToLayer(layer) < 0)
                {
                    Debug.LogError("[Setup] Layer 가 없습니다: " + layer);
                }
            }
        }

        private static void EnsureRenderPipeline()
        {
            EditorUtil.EnsureFolder(SettingsFolder + "/URP");
            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                UniversalRendererData rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RendererPath);
                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
                AssetDatabase.SaveAssets();
            }
            pipeline.shadowDistance = 60f;
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            int current = QualitySettings.GetQualityLevel();
            for (int level = 0; level < QualitySettings.names.Length; level++)
            {
                QualitySettings.SetQualityLevel(level, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(current, false);
        }

        private static void EnsureConfigAssets()
        {
            GameConfig gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigPath);
            if (gameConfig == null)
            {
                gameConfig = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(gameConfig, GameConfigPath);
            }
            gameConfig.timeScale = 1f;
            gameConfig.cameraDistance = 20f;
            gameConfig.cameraPitch = 50f;
            gameConfig.cameraYaw = 45f;
            gameConfig.cameraLookHeight = 1f;
            gameConfig.dropItemLifetime = 30f;
            gameConfig.waveInterval = 1.5f;
            gameConfig.dropMagnetSpeed = 12f;
            EditorUtility.SetDirty(gameConfig);

            QAConfig qaConfig = AssetDatabase.LoadAssetAtPath<QAConfig>(QAConfigPath);
            if (qaConfig == null)
            {
                qaConfig = ScriptableObject.CreateInstance<QAConfig>();
                AssetDatabase.CreateAsset(qaConfig, QAConfigPath);
            }
            qaConfig.randomSeed = 0;
            qaConfig.showDebugOnStart = false;
            EditorUtility.SetDirty(qaConfig);
        }

        public static GameConfig GameConfigAsset
        {
            get { return AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigPath); }
        }

        public static QAConfig QAConfigAsset
        {
            get { return AssetDatabase.LoadAssetAtPath<QAConfig>(QAConfigPath); }
        }
    }
}
