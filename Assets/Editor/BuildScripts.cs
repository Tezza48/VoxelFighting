using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.IO;

public class BuildScripts
{
    [MenuItem("Build/Server")]
    public static void BuildServer()
    {
        var scenes = EditorBuildSettings.scenes;
        var scenePaths = new string[scenes.Length];

        for (var i = 0; i < scenes.Length; i++)
        {
            scenePaths[i] = scenes[i].path;
        }

        var settings = new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = "./Build/Server/Server.exe",
            options = BuildOptions.EnableHeadlessMode,
            target = BuildTarget.StandaloneWindows,
            targetGroup = BuildTargetGroup.Standalone,
        };

        var report = BuildPipeline.BuildPlayer(settings);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            var path = Path.GetFullPath(settings.locationPathName);

            Process proc = new Process();
            proc.StartInfo.FileName = path;
            proc.Start();
        }
    }

    [MenuItem("Build/Client")]
    public static void BuildClient()
    {
        var scenes = EditorBuildSettings.scenes;
        var scenePaths = new string[scenes.Length];

        for (var i = 0; i < scenes.Length; i++)
        {
            scenePaths[i] = scenes[i].path;
        }

        var settings = new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = "./Build/Client/Client.exe",
            options = BuildOptions.None,
            target = BuildTarget.StandaloneWindows,
            targetGroup = BuildTargetGroup.Standalone,
        };

        var report = BuildPipeline.BuildPlayer(settings);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            var path = Path.GetFullPath(settings.locationPathName);

            Process proc = new Process();
            proc.StartInfo.FileName = path;
            proc.Start();
        }
    }
}