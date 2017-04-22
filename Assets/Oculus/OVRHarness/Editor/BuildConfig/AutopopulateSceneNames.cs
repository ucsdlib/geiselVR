/************************************************************************************

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.2 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.2

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Linq;
using SystemDiags = System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

/* This class provides several editor commands which can be used to prepare building settings.
 * The functions configure the scenes included in the build settings, and also write out the
 * included scenes to a text file which is loaded at runtime to configure the items in the 
 * scene selection menu
 *
 * The menu options provided are :
 * 
 * OVR/Configure Rift Build                     Add all scenes compatible with Rift (most require a gamepad)
 * OVR/Configure Gear VR Build                  Add all scenes compatible with Gear VR
 * OVR/Configure GearVR no Gamepad              Add all scenes compatible with Gear VR that don't require a gamepad
 * OVR/Configure Just This Scene                Add just the current scene
 * OVR/Configure Loader plus this Scene         Add just the loading screen scene and the current scene
 * OVR/Sample list from build settings          Keeps the build settings the same but configures scene.txt so that these scenes can be selected in the in-game menu
 * OVR/Configure Custom Build                   Configures a build based on the tags specified in Assets/OVRHarness/BuildConfig.asset
 */
public class AutopopulateSceneNames : AssetPostprocessor
{
	private static readonly string SCENE_FILE_PATH = "/OVRHarness/Resources/scenes.txt";
    private static readonly string STARTUP_SCENE_PATH = "Assets/OVRHarness/Scenes/StartUp.unity";
    private static readonly string LOADING_SCENE_PATH = "Assets/OVRHarness/Scenes/Loading.unity";
	
	
	
	// based on https://gist.github.com/benblo/10732800
	// Find all scene paths in project
    public static string[] FindAllScenePaths()
	{
		string[] guids = AssetDatabase.FindAssets("t:Scene");
		string[] paths = Array.ConvertAll<string, string>(guids, AssetDatabase.GUIDToAssetPath);
		paths = Array.FindAll(paths, File.Exists); // Unity erroneously considers folders named something.unity as scenes, remove them
		return paths;
	}

    //Find all scenes in our SampleScene directory that are not in a DONOTSHIP directory
    static string[] FindAllSampleScenePaths()
    {
        string[] sampleScenePath = { "Assets", "SampleScenes" };
        string[] existingSceneList = FindAllScenePaths();
        var scenes = existingSceneList.Where(c => IsInSubDir(c, sampleScenePath) && !c.Contains("DONOTSHIP")).ToList();

        scenes.Add(STARTUP_SCENE_PATH);
        scenes.Add(LOADING_SCENE_PATH);

        return scenes.ToArray();

    }

    static bool IsInSubDir(string path, string [] subdirpath)
    {
        string[] directories = path.Split(new char[] { '/','.' });;
        int i = 0;
        foreach (string component in subdirpath)
        {
            if (i > directories.Length || !string.Equals(component, directories[i]))
                return false;
            i++;
        }
        return true;
    }

    static string[] GetPathInSceneDirectory(string path)
    {
        int prefixesToRemove = 2;
        string[] directories = path.Split(new char[] { '/','.' });;
        if (!string.Equals(directories[0],"Assets") || !string.Equals(directories[1],"SampleScenes") )
        {
            Debug.LogWarning(string.Format("Scene found in unexpected path:{0} {1} ", path, directories[0]));
            // Path isn't as we expected so just return last element
            prefixesToRemove = 0;
        }
        string[] newPath = new string[directories.Length-prefixesToRemove-1];
        Array.Copy(directories,prefixesToRemove , newPath, 0, newPath.Length);
        return newPath;
    }
	private static void RefreshSceneList(List<string> actualSceneList)
	{
        // actualSceneList is list of scenes in assets
		// add all of the scenes in the list to the build settings unless they are already
		// there.
		
		List<string> buildScenes = new List<string>(); 
		List<string> disabledScenes = new List<string>(); // scenes that are configured in build settings but also disabled
		
        // add scenes that are configured in build settings
		foreach (UnityEditor.EditorBuildSettingsScene scene in UnityEditor.EditorBuildSettings.scenes)
		{
			if (actualSceneList.IndexOf(scene.path) != -1)
			{
				if (buildScenes.IndexOf(scene.path) != -1)
                {
                    Debug.LogError(string.Format("Scene {0} is in Editor Settings more than once",scene.path));
                }
                buildScenes.Add(scene.path);
				if (!scene.enabled)
				{
					disabledScenes.Add(scene.path);
				}
			}
		}
		
		// Add scenes we have assets for but aren't configured in build settings
		foreach (string scene in actualSceneList)
		{
			if (buildScenes.IndexOf(scene) == -1)
			{
				buildScenes.Add(scene);
			}
		}
		
       
        EnsureStartupSceneFirst(buildScenes);	
        WriteBuildSettings(buildScenes, disabledScenes);
    }


    // Make sure the loading scene is the first scene in the build settings
    static void EnsureStartupSceneFirst(List<string> scenes)
    {
        int startupIndex = scenes.IndexOf(LOADING_SCENE_PATH);
        if (startupIndex >= 0)
        {
            scenes.Remove(LOADING_SCENE_PATH);
            scenes.Insert(0, LOADING_SCENE_PATH);
        }
    }

    static void WriteBuildSettings(List<string> scenes, List<string> disabledScenes = null, bool writeBuildSettings = true)
    {
		StringBuilder sb = new StringBuilder();
        UnityEditor.EditorBuildSettingsScene[] finalScenes = new UnityEditor.EditorBuildSettingsScene[scenes.Count];
        for (int x = 0; x < finalScenes.Length; x++)
        {
            string scenePath = scenes[x];
            bool enabled = disabledScenes == null ||  disabledScenes.IndexOf(scenePath) == -1;
            finalScenes[x] = new UnityEditor.EditorBuildSettingsScene(scenePath, enabled);
            //Debug.Log(scenePath);
            // TODO: throw an error if two scenes have the same base name.
            if (!scenePath.Equals(STARTUP_SCENE_PATH) && 
                !scenePath.Equals(LOADING_SCENE_PATH) && 
                !scenePath.Contains("DONOTSHIP") &&
                enabled)
            {
                string[] path = GetPathInSceneDirectory(scenePath);
                string separator = "";
                foreach (var element in path)
                {
                    sb.Append(separator);
                    sb.Append(element);
                    separator = "/";
                }
                sb.Append("\n");
            }
        }
        if (writeBuildSettings)
        {
            UnityEditor.EditorBuildSettings.scenes = finalScenes;
        }

        string sceneListString = sb.ToString().Trim();
        string sceneListPath = Application.dataPath + SCENE_FILE_PATH;
        string sceneListDir = Path.GetDirectoryName(sceneListPath);
        Directory.CreateDirectory(sceneListDir);


        System.IO.File.WriteAllText(sceneListPath, sceneListString);
        AssetDatabase.Refresh();
    }

    static SamplesSceneSettings GetSceneSettings(string path)
    {
        SamplesSceneSettings settings = null;
        int li = path.LastIndexOf(".");
        if (li >= 0)
        {
            string settingsPath = path.Substring(0, li) + ".asset";
            settings = (SamplesSceneSettings)AssetDatabase.LoadAssetAtPath(settingsPath, typeof (SamplesSceneSettings));
            if (settings == null)
            {
                Debug.LogError(string.Format("No settings file for scene {0}. Creating settings file.",path));


                CustomAssetUtility.CreateAsset<SamplesSceneSettings>(settingsPath);
            }
        }
        else
        {
            Debug.LogError(string.Format("Unexpected scene path : {0}",path));
        }
        return settings;
    }

    static List<string> filterScenes(string[] scenes, List<string> includeTags, List<string> requireTags, List<string> excludeTags)
    {
        var filtered = new List<string>();

        foreach (var scene in scenes)
        {
            SamplesSceneSettings settings = GetSceneSettings(scene);
            if (settings)
            {
                bool include = false;
                foreach (var tag in includeTags)
                {
                    if (settings.HasTag(tag))
                        include = true;
                }
                if (include)
                {
                    foreach (var tag in requireTags)
                    {
                        if (!settings.HasTag(tag))
                            include = false;
                    }
                }
                if (include)
                {
                    foreach (var tag in excludeTags)
                    {
                        if (settings.HasTag(tag))
                            include = false;
                    }
                }
                if (include)
                {
                    filtered.Add(scene);
                }
            }
        }
        return filtered;

    }

   

    // Configure build for rift
    [MenuItem("OVR/Samples Build Config/Configure Rift Build")]
    public static void ConfigureRiftBuild()
    {
        Debug.Log("Rebuilding sample list - Rift");
        string[] existingSceneList = FindAllSampleScenePaths();

        List<string> tagsToInclude = new List<string>();
        List<string> tagsToRequire = new List<string>();
        List<string> tagsToExclude = new List<string>();
        tagsToInclude.Add("rift");

        List<string> finalStringList = filterScenes(existingSceneList, tagsToInclude, tagsToRequire, tagsToExclude);

        EnsureStartupSceneFirst(finalStringList);
        WriteBuildSettings(finalStringList);
    }

    // Configure build for Gear VR
    [MenuItem("OVR/Samples Build Config/Configure Gear VR Build")]
    public static void ConfigureGearVRBuild()
    {
        Debug.Log("Rebuilding sample list - Gearvr");
        string[] existingSceneList = FindAllSampleScenePaths();

        List<string> tagsToInclude = new List<string>();
        List<string> tagsToRequire = new List<string>();
        List<string> tagsToExclude = new List<string>();
        tagsToInclude.Add("gearvr");

        List<string> finalStringList = filterScenes(existingSceneList, tagsToInclude, tagsToRequire, tagsToExclude);


        EnsureStartupSceneFirst(finalStringList);
        WriteBuildSettings(finalStringList);
    }

    // Configure a build for Gear VR without a Gamepad
    [MenuItem("OVR/Samples Build Config/Configure GearVR no Gamepad")]
    static void ConfigureGearVRNoGamepad()
    {
        Debug.Log("Rebuilding sample list - Gearvr");
        string[] existingSceneList = FindAllSampleScenePaths();

        List<string> tagsToInclude = new List<string>();
        List<string> tagsToRequire = new List<string>();
        List<string> tagsToExclude = new List<string>();
        tagsToInclude.Add("gearvr");
        tagsToRequire.Add("gazeonly");

        List<string> finalStringList = filterScenes(existingSceneList, tagsToInclude, tagsToRequire, tagsToExclude);


        EnsureStartupSceneFirst(finalStringList);
        WriteBuildSettings(finalStringList);
    }

    // Configure settings to build just this scene
    [MenuItem("OVR/Samples Build Config/Configure Just This Scene")]
    static void ConfigureJustThisScene()
    {
        Debug.Log("Rebuilding sample list - This scene only");

        List<string> finalStringList = new List<string>();
        finalStringList.Add(EditorSceneManager.GetActiveScene().path);

        WriteBuildSettings(finalStringList);
    }

    // Configure the loading screen and the current scene
    [MenuItem("OVR/Samples Build Config/Configure Loader plus this Scene")]
    static void ConfigureLoaderPlusThisScene()
    {
        Debug.Log("Rebuilding sample list - This scene only");

        List<string> finalStringList = new List<string>();
        finalStringList.Add(LOADING_SCENE_PATH);
        finalStringList.Add(STARTUP_SCENE_PATH);
        finalStringList.Add(EditorSceneManager.GetActiveScene().path);

        WriteBuildSettings(finalStringList);
    }


    // Rebuild sample list based off current build menu settings
    [MenuItem("OVR/Samples Build Config/Sample list from build settings")]
    static void SampleListFromBuildSettings()
    {
        List<string> finalStringList = new List<string>();
        foreach (UnityEditor.EditorBuildSettingsScene scene in UnityEditor.EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                finalStringList.Add(scene.path);
        }

        EnsureStartupSceneFirst(finalStringList);
        WriteBuildSettings(finalStringList, null, false);
    }

    // Rebuild sample list based on BuildConfig.asset
    [MenuItem("OVR/Samples Build Config/Configure Custom Build")]
    static void ConfigureCustomBuild()
    {
        Debug.Log("Rebuilding sample list");
        string[] existingSceneList = FindAllSampleScenePaths();

        var config = (SamplesBuildConfig)AssetDatabase.LoadAssetAtPath("Assets/OVRHarness/BuildConfig.asset", typeof (SamplesBuildConfig));

        List<string> finalStringList = filterScenes(existingSceneList, config.tagsToInclude, config.tagsToInclude, config.tagsToExclude);

        EnsureStartupSceneFirst(finalStringList);
        WriteBuildSettings(finalStringList);
    }
   
}



