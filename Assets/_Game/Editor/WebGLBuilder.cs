using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor build script for WebGL batch mode builds.
/// Called from command line: Unity.exe -batchmode -executeMethod WebGLBuilder.Build
/// </summary>
public static class WebGLBuilder
{
    public static void Build()
    {
        Debug.Log("[WebGLBuilder] Starting WebGL build...");

        string buildPath = "Builds/WebGL";

        // Get all scenes in build settings, or fallback to SampleScene
        string[] scenes;
        if (EditorBuildSettings.scenes.Length > 0)
        {
            scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        else
        {
            scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[WebGLBuilder] Build succeeded! Output: {buildPath}");
            Debug.Log($"[WebGLBuilder] Total size: {report.summary.totalSize / 1024 / 1024} MB");
        }
        else
        {
            Debug.LogError($"[WebGLBuilder] Build failed: {report.summary.result}");
            EditorApplication.Exit(1);
        }
    }
}
