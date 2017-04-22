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
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Load a scene with loading screen
/// </summary>
public class OVRSceneLoader : MonoBehaviour {

    public GameObject logoCanvas;

    [Tooltip("First scene to load")]
    public string firstSceneName;
    [Tooltip("How long to spend fading in first scene")]
    public float firstSceneFadeTime;
    [Tooltip("How long to spend fading in scenes other than the first one")]
    public float defaultFadeTime;


    static bool firstScene = true;
    static private string nextScene;
    

    /// <summary>
    /// Load specified scene with the loading screen shown
    /// </summary>
    /// <param name="scene"></param>
    static public void LoadSceneViaLoadingScene(string scene)
    {
        nextScene = scene;
        firstScene = false;
        SceneManager.LoadScene("Loading");
    }

	// Use this for initialization
    IEnumerator Start()
    {
        float fadeTime;
        // First time through we load the prechosen first scene
        if (firstScene)
        {
            nextScene = firstSceneName;
            fadeTime = firstSceneFadeTime;
            OVRManager.display.RecenterPose();
        }
        else
        {
            fadeTime = defaultFadeTime;
        }

        // Set priority low and load asynchronously to help performance while loading
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        AsyncOperation async = SceneManager.LoadSceneAsync(nextScene);
        async.allowSceneActivation = false;
        // Wait until Unity has finished loading the scene.
        // Wth allowSceneActivation = false Unity won't fully load the scene and will
        // and async.progress will only go as far as 90%
        while (!async.isDone)
        {
            float loadProgress = async.progress;

            if (loadProgress >= 0.9f)
            {
                // Almost done.
                break;
            }
        
            yield return null;
        }
        
        //Now fade to black. We fade to black here as the final part of scene loading can cause judder
        float t = Time.time;
        SpriteRenderer sr = logoCanvas.GetComponentInChildren<SpriteRenderer>();
        while (Time.time < t + fadeTime)
        {
            float i = 1 - (Time.time - t) / fadeTime;
            sr.color = new Color(i, i, i);
            yield return null;
        }
        
        // Finish loading the scene
        yield return null;
        async.allowSceneActivation = true;
        
        yield return async;
        Debug.Log("Loading complete");
    }
	
}
