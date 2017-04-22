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
using System.Collections;
using System.Collections.Generic;


/* This is used in combination with SamplesSceneSettings. When configuring a custom build these settings are checked
 * to determine which tag filters to apply to the scenes to include.
 */


public class SamplesBuildConfig : ScriptableObject {

    
    // A scene will be included if it contains...

    public List<string> tagsToInclude;  // ... at least one of these
    public List<string> tagsToRequire;  // ... AND all of theses
    public List<string> tagsToExclude;  // ... AND none of these


    [MenuItem("Assets/Create/Build Config")]
    public static void CreateAsset()
    {
        CustomAssetUtility.CreateAsset<SamplesBuildConfig>();
    }
}
