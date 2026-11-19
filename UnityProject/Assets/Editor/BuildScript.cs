using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ExpeditionCamp.EditorTools
{
    public static class BuildScript
    {
        private const string ProductName = "ExpeditionCamp";
        private const string TestOutputDirectory = "Builds/Test";
        private const string ReleaseOutputDirectory = "Builds/Release";
        private const string QaBridgeDefine = "QA_BRIDGE";

        public static void BuildForTest()
        {
            Run(TestOutputDirectory, true);
        }

        public static void BuildForRelease()
        {
            Run(ReleaseOutputDirectory, false);
        }

        private static void Run(string outputDirectory, bool withQaBridge)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).Parent.FullName;
            string absoluteOutputDirectory = Path.Combine(projectRoot, outputDirectory);
            if (!Directory.Exists(absoluteOutputDirectory))
            {
                Directory.CreateDirectory(absoluteOutputDirectory);
            }

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            string executablePath = Path.Combine(absoluteOutputDirectory, ExecutableName(target));

            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = EnabledScenePaths();
            options.locationPathName = executablePath;
            options.target = target;
            options.targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            options.options = BuildOptions.None;
            if (withQaBridge)
            {
                options.extraScriptingDefines = new[] { QaBridgeDefine };
            }
            else
            {
                options.extraScriptingDefines = new string[0];
            }

            Debug.Log("[BuildScript] " + target + " 빌드를 시작합니다 (QA_BRIDGE " +
                (withQaBridge ? "포함" : "제외") + ")");

            string projectDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            if (!withQaBridge)
            {
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, RemoveDefine(projectDefines, QaBridgeDefine));
            }

            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(options);
            }
            finally
            {
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, projectDefines);
            }
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("[BuildScript] 빌드에 실패했습니다: " + summary.result +
                    " (오류 " + summary.totalErrors + "건)");
                EditorApplication.Exit(1);
                return;
            }

            WriteBuildInfo(absoluteOutputDirectory, withQaBridge);
            Debug.Log("[BuildScript] 빌드를 마쳤습니다: " + executablePath +
                " (" + summary.totalSize + " byte)");
        }

        private static string RemoveDefine(string defines, string target)
        {
            List<string> kept = new List<string>();
            foreach (string define in defines.Split(';'))
            {
                string trimmed = define.Trim();
                if (trimmed.Length == 0 || trimmed == target)
                {
                    continue;
                }
                kept.Add(trimmed);
            }
            return string.Join(";", kept.ToArray());
        }

        private static string[] EnabledScenePaths()
        {
            List<string> scenePaths = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenePaths.Add(scene.path);
                }
            }
            return scenePaths.ToArray();
        }

        private static string ExecutableName(BuildTarget target)
        {
            if (target == BuildTarget.StandaloneOSX)
            {
                return ProductName + ".app";
            }
            if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows)
            {
                return ProductName + ".exe";
            }
            return ProductName;
        }

        private static int ReadBuildNumber()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (arguments[index] != "-buildNumber")
                {
                    continue;
                }
                int value;
                if (int.TryParse(arguments[index + 1], out value))
                {
                    return value;
                }
            }
            return 0;
        }

        private static void WriteBuildInfo(string outputDirectory, bool withQaBridge)
        {
            string version = PlayerSettings.bundleVersion;
            int buildNumber = ReadBuildNumber();
            string builtAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            string json = "{\n" +
                "  \"version\": \"" + version + "\",\n" +
                "  \"build_number\": " + buildNumber + ",\n" +
                "  \"built_at\": \"" + builtAt + "\",\n" +
                "  \"qa_bridge\": " + (withQaBridge ? "true" : "false") + "\n" +
                "}\n";

            string path = Path.Combine(outputDirectory, "build_info.json");
            File.WriteAllText(path, json);
            Debug.Log("[BuildScript] build_info.json 을 만들었습니다: " + path);
        }
    }
}
