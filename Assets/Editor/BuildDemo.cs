using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public static class BuildDemo
{
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/MountainTea.unity");
        PlayerSettings.companyName="MountainWindWorkshop";
        PlayerSettings.productName="山風茶屋";
        PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=false;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11});
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        PlayerSettings.apiCompatibilityLevel=ApiCompatibilityLevel.NET_Standard;
        QualitySettings.SetQualityLevel(3);QualitySettings.vSyncCount=1;QualitySettings.antiAliasing=4;
        Directory.CreateDirectory("Builds/WindowsV18");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/MountainTea.unity"},locationPathName="Builds/WindowsV18/MountainTea.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        Debug.Log("DEMO BUILD: "+report.summary.result+" | "+report.summary.totalSize+" bytes");
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Demo build failed");
    }
}
