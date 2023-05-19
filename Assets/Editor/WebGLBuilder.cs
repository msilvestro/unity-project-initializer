using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ParallelBuild
{
    public class WebGLBuilder
    {
        private static string[] GetAllScenes()
        {
            return EditorBuildSettings.scenes
                 .Where(scene => scene.enabled)
                 .Select(scene => scene.path)
                 .ToArray();
        }

        private static string GetArg(string name, string defaultValue = null)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return defaultValue;
        }

        public static bool Build()
        {
            return Build(GetArg("-buildpath", "Build/WebGL"));
        }

        public static bool Build(string buildPath)
        {
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                scenes = GetAllScenes()
            };
            var buildReport = BuildPipeline.BuildPlayer(options);
            return buildReport.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
        }
    }
}
