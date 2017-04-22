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
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Parse;
using UnityEngine.SceneManagement;

/* OVRHarness
 * This class is concerned with managing the scene list menu, navigating between scenes
 * and the top level intro text.
 * */


public class OVRHarness : MonoBehaviour, IOVRInspectorContext
{
    public string introFile;
    public string helpFile;
    public bool LoadingLevel {
        get { return loadingLevel; }
    }
    
    private static readonly string SCENE_FILE_NAME = "scenes"; // File which contains the list of scenes
    private string[] sceneNames;
    private bool loadingLevel = false;
    private OVRInspector Inspector;
    private enum InspectorContext { None, SceneList, Help }

    // Represents a node in the directory system heirarchy which we traverse and convert
    // to a heirarchy of menus
    class HierarchyNode
    {
        public List<HierarchyNode> children;
        public HierarchyNode parent;
        public string name;
        
        public void addNode(string[] path, int pathDepth = 0 /*which element of the path we are currently processing, start at 0 meaning the root of the path*/)
        {
            if (pathDepth >= path.Length)
            {
                // We've reached the end of the path
                return;
            }

            bool foundSubNode = false;
            // Check if we've already added a menu to represent this directory
            foreach (var node in children)
            {
                if (string.Equals(node.name, path[pathDepth]))
                {
                    node.addNode(path, pathDepth + 1);
                    foundSubNode = true; 
                    break;
                }
            }

            // If not found add a node now to represent this directory
            if (!foundSubNode)
            {
                HierarchyNode newNode = new HierarchyNode(path[pathDepth], this);
                children.Add(newNode);
                newNode.addNode(path, pathDepth + 1);

            }
        }

        public HierarchyNode(string name, HierarchyNode parent)
        {
            this.name = name;
            this.parent = parent;
            children = new List<HierarchyNode>();
        }
    };

    private HierarchyNode rootNode, currentNode;


    void Awake()
    {
        //Keep this object alive as we move between scenes
        DontDestroyOnLoad(gameObject);
    }

	void Start () 
    {
        // Load scenes list from file
        ReadSceneList();

        // Start at the root level of the scene menu system
        currentNode = rootNode;

        
        // Configure top level menu items in the inspector.
        Inspector = OVRInspector.instance;
        Inspector.RegisterContext(this, (int)InspectorContext.Help,true);
        Inspector.RegisterContext(this, (int)InspectorContext.SceneList,true);
        // Keep the inspector around as we load scenes
        DontDestroyOnLoad(Inspector.gameObject);


        // pop the inspector up now and show the intro text
        Inspector.Show();
        Inspector.SetDocTextFromFile(introFile);

        // It's easy to accidentally scroll the docs as you put the headset on so
        // disable the docs scrolling briefly
        StartCoroutine(DisableDocsScrollingForStartup());

        InitReporting();


#if !UNITY_ANDROID || UNITY_EDITOR
        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
	}

    IEnumerator DisableDocsScrollingForStartup()
    {
        Inspector.SetDocsScrollEnabled(false);
        yield return new WaitForSeconds(3);
        Inspector.SetDocsScrollEnabled(true);
    }

    void ReadSceneList()
    {
        rootNode = new HierarchyNode("root", null);
        TextAsset text = Resources.Load(SCENE_FILE_NAME) as TextAsset;
        if (text != null)
        {
            string scenes = text.text;
            sceneNames = Regex.Split(scenes, "\r\n|\r|\n");
        }

        foreach (string sceneName in sceneNames)
        {
            string [] scenePath = sceneName.Split(new char[]{'/'});
            rootNode.addNode(scenePath);
        }
    }



    // ********************  IOVRInspectorContext ********************
    public void SetContextActive(OVRInspector Inspector, int subContextID, bool active)
    {
    }

    // Called by the inspector when it wants us to build one of the UIs we registered with RegisterContext
    public void BuildUI(OVRInspector Inspector, int subContextID)
    {
        switch ((InspectorContext)subContextID)
        {
            case InspectorContext.Help:
                BuildHelpUI(Inspector);
                break;
            case InspectorContext.SceneList:
                BuildSceneUI(Inspector);
                break;

        }
    }

    public string GetName(int subContextID)
    {
        switch ((InspectorContext)subContextID)
        {
            case InspectorContext.Help:
                return "Help";
            case InspectorContext.SceneList:
                return "Scenes";
        }
        return "?";
    }
    //******************** END   IOVRInspectorContext ********************
      

    // Build menu for scene selection
    public void BuildSceneUI(OVRInspector Inspector)
    {
        if (currentNode.parent != null)
        {
            Inspector.AddFolder("..", delegate { NodeSelected(currentNode.parent); });
        }
        foreach (var node in currentNode.children)
        {
            HierarchyNode nodeCopy = node;
            if (node.children.Count > 0)
            {
                if (node.children.Count == 1 && node.children[0].children.Count == 0 && string.Equals(node.children[0].name, node.name, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    // Special case where a folder only contains one scene with the same name as the folder. Just show the scene
                    Inspector.AddButton(node.children[0].name, delegate { NodeSelected(nodeCopy.children[0]); });
                }
                else
                {
                    Inspector.AddFolder(node.name, delegate { NodeSelected(nodeCopy); });
                }
            }
            else
                Inspector.AddButton(node.name, delegate { NodeSelected(nodeCopy); });
        }
    }

    // Show help screen
    public void BuildHelpUI(OVRInspector Inspector)
    {
        Inspector.SetDocTextFromFile(helpFile);
        Inspector.AddButton("Show Intro Docs", delegate {ShowIntroDocs();});
        if (Inspector.ScenePanelPresent())
        {
            Inspector.AddButton("Show Scene Docs", delegate { GotoSceneControls(); });
        }
    }

    private void ShowIntroDocs()
    {
        Inspector.SetDocTextFromFile(introFile);
    }

    private void GotoSceneControls()
    {
        Inspector.ActivateSceneContext();
    }

    // Handle a scene or directory being selected to load
    private void NodeSelected(HierarchyNode node)
    {
        if (node.children.Count > 0)
        {
            // It's a directory
            Debug.Log(string.Format("Opening {0}", node.name));
            currentNode = node;
            OVRInspector.instance.UpdateContextMenu();
        }
        else
        {
            // It's a level
            Debug.Log(string.Format("Loading {0}", node.name));
            Inspector.ResetDocText();
            Inspector.Hide();
            LoadLevel(node.name);
        }
    }
   
    void LoadLevel(string levelName)
    {
        if (loadingLevel)
            return;
        loadingLevel = true;
        StartCoroutine(LoadLevelCoroutine(levelName));
    }

    IEnumerator LoadLevelCoroutine(string levelName)
    {
        // Fade to black
        yield return StartCoroutine(OVRInspector.instance.FadeOutCameras());
        
        // Get rid of any elements of the inspector menu which were specific to this scene
        Inspector.ClearSceneSpecificContexts();


        OVRSceneLoader.LoadSceneViaLoadingScene(levelName);
        loadingLevel = false;

    }



    // Parse logging

    // This is called when Unity loads a level but not when loading the first level
    void OnLevelWasLoaded(int level)
    {
        ReportEvent("LevelStart", SceneManager.GetActiveScene().name);
    }


    IEnumerator PerformanceLogCoroutine()
    {
        yield return new WaitForSeconds(10);
        while (true)
        {
            ReportPerformance();
            yield return new WaitForSeconds(60);
        }

    }
    void ReportPerformance()
    {
        ParseObject obj = new ParseObject("Performance");
        obj["deviceUniqueIdentifier"] = SystemInfo.deviceUniqueIdentifier;
        obj["fps"] = OVRInspector.instance.frameRate;
        obj.SaveAsync();
    }

    void LogParseException(System.Exception e)
    {
        Debug.LogError("Parse Exception");
        Debug.LogException(e);
    }

    void ReportEvent(string name, string data = "")
    {
        try
        {
            ParseObject obj = new ParseObject("Event");
            obj["deviceUniqueIdentifier"] = SystemInfo.deviceUniqueIdentifier; ;
            obj["elapsed"] = Time.unscaledTime;
            obj["name"] = name;
            obj["data"] = data;
            obj.SaveAsync();
        }
        catch (System.Exception e)
        {
            LogParseException(e);
        }
    }

    void ReportStat(string name, float stat)
    {
        try
        {
            ParseObject obj = new ParseObject("Stat");
            obj["deviceUniqueIdentifier"] = SystemInfo.deviceUniqueIdentifier; ;
            obj["elapsed"] = Time.unscaledTime;
            obj["name"] = name;
            obj["value"] = stat;
            obj.SaveAsync();
        }
        catch (System.Exception e)
        {
            LogParseException(e);
        }

    }



    void ReportSystemData()
    {
        try
        {
            ParseObject obj = new ParseObject("SystemReport");

            obj["deviceModel"] = SystemInfo.deviceModel;
            obj["deviceName"] = SystemInfo.deviceName;
            obj["deviceType"] = SystemInfo.deviceType.ToString();
            obj["deviceUniqueIdentifier"] = SystemInfo.deviceUniqueIdentifier;
            obj["graphicsDeviceID"] = SystemInfo.graphicsDeviceID;
            obj["graphicsDeviceName"] = SystemInfo.graphicsDeviceName;
            obj["graphicsDeviceVendor"] = SystemInfo.graphicsDeviceVendor;
            obj["graphicsDeviceVendorID"] = SystemInfo.graphicsDeviceVendorID;
            obj["graphicsDeviceVersion"] = SystemInfo.graphicsDeviceVersion;
            obj["graphicsMemorySize"] = SystemInfo.graphicsMemorySize;
            obj["graphicsShaderLevel"] = SystemInfo.graphicsShaderLevel;
            obj["operatingSystem"] = SystemInfo.operatingSystem;
            obj["processorCount"] = SystemInfo.processorCount;
            obj["processorType"] = SystemInfo.processorType;
            obj["systemMemorySize"] = SystemInfo.systemMemorySize;
            obj["unityVersion"] = Application.unityVersion;

            obj.SaveAsync();
        }
        catch (System.Exception e)
        {
            LogParseException(e);
        }

    }


    void InitReporting()
    {

        ReportEvent("AppStart");
        ReportEvent("LevelStart", SceneManager.GetActiveScene().name);
        ReportSystemData();

        StartCoroutine(PerformanceLogCoroutine());

    }

    
}
