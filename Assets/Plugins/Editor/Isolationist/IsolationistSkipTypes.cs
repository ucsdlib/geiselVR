using UnityEditor;
using UnityEngine;

namespace Plugins.Isolationist.Editor
{
	[InitializeOnLoad]
	public static class IsolationistSkipTypes
	{
		static IsolationistSkipTypes()
		{
			// Add Cameras
			EditorIsolateCommand.AddCameraType(typeof(Camera));

			// Add Lights
			EditorIsolateCommand.AddLightType(typeof(Light));
			EditorIsolateCommand.AddLightType(typeof(ReflectionProbe));
		}
	}
}