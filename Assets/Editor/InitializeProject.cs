using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


[CreateAssetMenu(fileName = "Initialize", menuName = "Tools/Project Initializer", order = 1)]
public class InitializeProject : ScriptableObject
{
    [SerializeField] private bool addGitignore = true;

    [Header("Publish Tools")] [SerializeField]
    private bool addParallelBuild = true;

    [SerializeField] private bool publishOnItch = true;
    [SerializeField] private string itchUser = "msilvestro";
    [SerializeField] private string itchGame;

    [Header("Project Structure")] [SerializeField]
    private bool createProjectFolders = true;

    [SerializeField] private string[] projectFolders = { "Art", "Audio", "Prefabs", "Scripts" };

    [Header("Namespace")] [SerializeField] private bool setRootNamespace = true;
    [SerializeField] private string rootNamespace;

    [Header("Change settings")] [SerializeField]
    private bool selectWebGLBetterMinimalTemplate = true;

    [SerializeField, Tooltip("This has always been essential to make WebGL builds work")]
    private bool setDecompressionFallback = true;

    [SerializeField, Tooltip("Enable only WebGL 2, removing the fallback of the deprecated Web GL1")]
    private bool enableOnlyWebGL2 = true;

    [SerializeField] private bool switchActiveBuildTargetToWebGL = true;

    [SerializeField] private bool setUnderscoreGameObjectNamingScheme = true;

    private void OnEnable()
    {
        if (itchGame == "")
        {
            itchGame = Application.productName.Replace(" ", "-").ToLower();
        }

        if (rootNamespace == "")
        {
            rootNamespace = Application.productName.ToPascalCase();
        }
    }

    private void AddGitignore()
    {
        string unityGitignoreUrl = "https://raw.githubusercontent.com/github/gitignore/main/Unity.gitignore";
        using var client = new WebClient();
        client.DownloadFile(unityGitignoreUrl, ".gitignore");
    }

    private void AddParallelBuild()
    {
        Uri parallelGistBaseUrl =
            new Uri(
                "https://gist.githubusercontent.com/msilvestro/0cb78bf4aeb0fd8d0697db0f38b0c4ae/raw/d3df7d2a6fc5268bb75927d5667300c1b414c6e6");
        using var client = new WebClient();
        client.DownloadFile(new Uri(parallelGistBaseUrl, "ParallelBuild.ps1"), "ParallelBuild.ps1");
        Directory.CreateDirectory("Assets/Editor");
        client.DownloadFile(new Uri(parallelGistBaseUrl, "WebGLBuilder.cs"), "Assets/Editor/WebGLBuilder.cs");

        string publishOnItchValue = publishOnItch ? "true" : "false";
        string buildSettingsContents = "{\n" +
                                       "   \"buildPath\": \"Builds/WebGL\",\n" +
                                       $"   \"itchUser\": \"{itchUser}\",\n" +
                                       $"   \"itchGame\": \"{itchGame}\",\n" +
                                       "   \"parallel\": true,\n" +
                                       $"   \"publishToItch\": {publishOnItchValue}\n" +
                                       "}\n";
        File.WriteAllText("buildsettings.json", buildSettingsContents);
    }

    private void CreateProjectFolders()
    {
        foreach (string folder in projectFolders)
        {
            Directory.CreateDirectory(Path.Join("Assets", folder));
        }
    }

    private void SetRootNamespace()
    {
        EditorSettings.projectGenerationRootNamespace = rootNamespace;
    }
    
    private void EditAssetField(string assetPath, string field, string newValue)
    {
        var lines = File.ReadLines(assetPath);
        List<string> outputContent = new List<string>();
        foreach (var line in lines)
        {
            if (line.StartsWith($"  {field}:"))
            {
                if (newValue.StartsWith("\n"))
                {
                    outputContent.Add($"  {field}:{newValue}");
                }
                else
                {
                    outputContent.Add($"  {field}: {newValue}");
                }
            }
            else
            {
                outputContent.Add(line);
            }
        }
        File.WriteAllLines(assetPath, outputContent);
    }

    public void Begin()
    {
        if (addGitignore)
        {
            Debug.Log("Adding `.gitignore`...");
            AddGitignore();
        }

        if (addParallelBuild)
        {
            Debug.Log("Adding Parallel Build...");
            AddParallelBuild();
        }

        if (createProjectFolders)
        {
            Debug.Log("Creating project folders...");
            CreateProjectFolders();
        }

        if (setRootNamespace)
        {
            Debug.Log("Setting root namespace...");
            SetRootNamespace();
        }

        if (selectWebGLBetterMinimalTemplate)
        {
            Debug.Log("Selecting the WebGL better minimal template...");
            PlayerSettings.WebGL.template = "PROJECT:BetterMinimal";
        }

        if (setDecompressionFallback)
        {
            Debug.Log("Setting decompression fallback...");
            PlayerSettings.WebGL.decompressionFallback = true;
        }

        if (enableOnlyWebGL2)
        {
            Debug.Log("Enabling only WebGL 2...");
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[]
            {
                GraphicsDeviceType.OpenGLES3
            });
        }

        if (setUnderscoreGameObjectNamingScheme)
        {
            Debug.Log("Setting underscore game object naming scheme...");
            EditorSettings.gameObjectNamingScheme = EditorSettings.NamingScheme.Underscore;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (selectWebGLBetterMinimalTemplate)
        {
            EditAssetField(
                "ProjectSettings/ProjectSettings.asset",
                "m_TemplateCustomTags",
                @"
    BACKGROUND: 
    OPTIMIZE_FOR_PIXEL_ART: 
    SCALE_TO_FIT: "
            );
        }

        if (switchActiveBuildTargetToWebGL)
        {
            Debug.Log("Switching to WebGL as active build target...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        }
    }
}

[CustomEditor(typeof(InitializeProject))]
public class InitializeProjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        InitializeProject initializeProject = (InitializeProject)target;
        if (GUILayout.Button("Initialize"))
        {
            initializeProject.Begin();
        }
    }
}