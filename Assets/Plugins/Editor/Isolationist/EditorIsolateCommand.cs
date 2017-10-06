using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace Plugins.Isolationist.Editor
{
	[InitializeOnLoad]
	public static class EditorIsolateCommand
	{
		private const string ISOLATE_KEY_PREF = "IsolationistKey";
		private const string ISOLATE_ALT_PREF = "IsolationistAlt";
		private const string ISOLATE_CTRL_PREF = "IsolationistCtrl";
		private const string ISOLATE_SHIFT_PREF = "IsolationistShift";
		private const string ISOLATE_HIDE_LIGHTS_PREF = "IsolationistHideLights";
		private const string ISOLATE_HIDE_CAMERAS_PREF = "IsolationistHideCameras";
		private static bool _alt;
		private static bool _ctrl;
		private static bool _shift;
		private static bool _ctrlOrShiftPressed;
		private static bool _hideLights;
		private static bool _hideCameras;
		private static KeyCode _hotkey;
		private static GameObject _lastSelection;
		private static int _lastSelectionCount;
		private static string _shortcutDisplay;
		private static List<GameObject> _lastSelectionList;
		private static readonly List<Type> _lightTypeList = new List<Type>();
		private static readonly List<Type> _cameraTypeList = new List<Type>();

		public static void AddLightType(Type type) {
			_lightTypeList.Add(type);
		}

		public static void AddCameraType(Type type) {
			_cameraTypeList.Add(type);
		}

		static EditorIsolateCommand()
		{
			_alt = EditorPrefs.GetBool(ISOLATE_ALT_PREF, false);
			_ctrl = EditorPrefs.GetBool(ISOLATE_CTRL_PREF, false);
			_shift = EditorPrefs.GetBool(ISOLATE_SHIFT_PREF, false);
			_hotkey = (KeyCode) EditorPrefs.GetInt(ISOLATE_KEY_PREF, (int) KeyCode.I);
			_hideLights = EditorPrefs.GetBool(ISOLATE_HIDE_LIGHTS_PREF, true);
			_hideCameras = EditorPrefs.GetBool(ISOLATE_HIDE_CAMERAS_PREF, true);
			EditorApplication.update += Update;
			EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
			SceneView.onSceneGUIDelegate += OnSceneGUI;

			AddCameraType(typeof(Camera));
			AddLightType(typeof(Light));
			AddLightType(typeof(ReflectionProbe));
		}

		private static bool IsolateKeyPressed
		{
			get
			{
				if (Event.current == null) return false;
				if (Event.current.type != EventType.keyUp) return false;
				return Event.current.keyCode == _hotkey && Event.current.alt == _alt && Event.current.control == _ctrl && Event.current.shift == _shift;
			}
		}

		public static string ShortcutDisplay { get { return _shortcutDisplay.IsNullOrEmpty() ? _shortcutDisplay = GetShortcutDisplay() : _shortcutDisplay; } }

		private static void Update()
		{
			if (!IsolateInfo.IsIsolated || _lastSelection == Selection.activeGameObject && _lastSelectionCount == Selection.gameObjects.Length) return;
			var selectionList = Selection.gameObjects.ToList();
			var newItems = _lastSelectionList == null ? selectionList : selectionList.Except(_lastSelectionList).ToList();
			_lastSelection = Selection.activeGameObject;
			_lastSelectionCount = Selection.gameObjects.Length;
			_lastSelectionList = selectionList;
			SelectionChanged(newItems);
		}

		private static void OnSceneGUI(SceneView sceneView) { OnGUI(); }

		private static void HierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
		{
			if (!GUI.GetNameOfFocusedControl().IsNullOrEmpty()) return;
			OnGUI();
		}

		private static void OnGUI()
		{
			_ctrlOrShiftPressed = Event.current.control || Event.current.shift;
			if (!IsolateKeyPressed) return;
			ToggleIsolate();
			Event.current.Use();
		}

		private static void PlaymodeStateChanged()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode) IsolateInfo.Show();
			else IsolateInfo.Hide();
		}

		private static void SelectionChanged(List<GameObject> newItems)
		{
			if (WasHidden(Selection.activeTransform) && !_ctrlOrShiftPressed)
			{
				EndIsolation();
				return;
			}

			if (!_ctrlOrShiftPressed) return;

			UpdateIsolation(newItems);
		}

		private static List<GameObject> GetAllGameObjectsToHide() { return IsolateInfo.Instance.FocusObjects.SelectMany<GameObject, GameObject>(GetGameObjectsToHide).Distinct().ToList(); }

		[MenuItem("Tools/Toggle Isolate", true)]
		public static bool CanToggleIsolate() { return Selection.activeGameObject || IsolateInfo.IsIsolated; }

		[MenuItem("Tools/Toggle Isolate")]
		public static void ToggleIsolate()
		{
			if (IsolateInfo.IsIsolated) EndIsolation();
			else StartIsolation();
		}

		private static void StartIsolation()
		{
			if (IsolateInfo.Instance)
			{
				Debug.LogWarning("Isolationist: Found previous isolation info. This shouldn't happen. Ending the previous isolation anyway.");
				EndIsolation();
			}

			if (EditorApplication.isPlayingOrWillChangePlaymode) {
				Debug.LogWarning("Isolationist: Can't isolate while playing. It'll break stuff!");
				return;
			}

			// Create new IsolateInfo object.
			var container = new GameObject("IsolationInfo") {hideFlags = HideFlags.HideInHierarchy};
			Undo.RegisterCreatedObjectUndo(container, "Isolate");
			IsolateInfo.Instance = container.AddComponent<IsolateInfo>();
			var focusList = IsolateInfo.Instance.FocusObjects = Selection.gameObjects.ToList();

			if (!_hideLights) _lightTypeList.ForEach(t => focusList.AddRange(Object.FindObjectsOfType(t).Select<Object, GameObject>(ObjectToGO)));

			if (!_hideCameras) _cameraTypeList.ForEach(t => focusList.AddRange(Object.FindObjectsOfType(t).Select<Object, GameObject>(ObjectToGO)));

			IsolateInfo.Instance.HiddenObjects = GetAllGameObjectsToHide();

			if (!IsolateInfo.Instance.HiddenObjects.Any())
			{
				Object.DestroyImmediate(container);
				Debug.LogWarning("Isolationist: Nothing to isolate.");
				return;
			}

			Undo.RecordObjects(IsolateInfo.Instance.HiddenObjects.Cast<Object>().ToArray(), "Isolate");
			IsolateInfo.Hide();
		}

		private static GameObject ObjectToGO(Object obj)
		{
			Component component = obj as Component;
			return component ? component.gameObject : null;
		}

		private static void UpdateIsolation(List<GameObject> newItems)
		{
			if (!newItems.Any()) return;
			Undo.RecordObject(IsolateInfo.Instance, "Isolate");
			Undo.RecordObjects(IsolateInfo.Instance.HiddenObjects.Cast<Object>().ToArray(), "Isolate");
			IsolateInfo.Show();
			IsolateInfo.Instance.FocusObjects = IsolateInfo.Instance.FocusObjects.Concat(newItems).Distinct().ToList();
			var newHiddenObjects = GetAllGameObjectsToHide();
			Undo.RecordObjects(newHiddenObjects.Except(IsolateInfo.Instance.HiddenObjects).Cast<Object>().ToArray(), "Isolate");
			IsolateInfo.Instance.HiddenObjects = newHiddenObjects;
			IsolateInfo.Hide();
		}

		private static bool WasHidden(Transform t) { return t && !t.gameObject.activeInHierarchy && !t.GetComponent<IsolateInfo>() && !IsolateInfo.Instance.FocusObjects.Any(t.gameObject.IsRelative); }

		private static bool CanHide(Transform t)
		{
			return t && t.gameObject.activeSelf && !t.GetComponent<IsolateInfo>() && !IsolateInfo.Instance.FocusObjects.Any(t.gameObject.IsRelative);
		}

		private static IEnumerable<GameObject> GetGameObjectsToHide(GameObject keeperGo)
		{
			if (!keeperGo) return new List<GameObject>();

			var keeper = keeperGo.transform;
			var transformsToHide = new List<Transform>();

			while (keeper.parent)
			{
				transformsToHide.AddRange(keeper.parent.GetChildren().Where(CanHide));
				keeper = keeper.parent;
			}

			transformsToHide.AddRange(GetRootTransforms().Where(CanHide));
			return transformsToHide.Select(t => t.gameObject);
		}

		[PreferenceItem("Isolationist")]
		public static void PreferencesGUI()
		{
			GUILayout.Label("Shortcut: " + ShortcutDisplay);
			_ctrl = EditorGUILayout.Toggle("Ctrl", _ctrl);
			_alt = EditorGUILayout.Toggle("Alt", _alt);
			_shift = EditorGUILayout.Toggle("Shift", _shift);
			_hotkey = (KeyCode) EditorGUILayout.EnumPopup("Shortcut Key", _hotkey);

			GUILayout.Label("");

			_hideLights = EditorGUILayout.Toggle("Hide Lights", _hideLights);
			_hideCameras = EditorGUILayout.Toggle("Hide Cameras", _hideCameras);


			if (!GUI.changed) return;

			EditorPrefs.SetBool(ISOLATE_CTRL_PREF, _ctrl);
			EditorPrefs.SetBool(ISOLATE_ALT_PREF, _alt);
			EditorPrefs.SetBool(ISOLATE_SHIFT_PREF, _shift);
			EditorPrefs.SetBool(ISOLATE_HIDE_LIGHTS_PREF, _hideLights);
			EditorPrefs.SetBool(ISOLATE_HIDE_CAMERAS_PREF, _hideCameras);
			EditorPrefs.SetInt(ISOLATE_KEY_PREF, (int) _hotkey);
			_shortcutDisplay = GetShortcutDisplay();
		}

		private static void EndIsolation()
		{
			if (!IsolateInfo.Instance) return;

			if (IsolateInfo.Instance.HiddenObjects != null)
			{
				Undo.RecordObjects(IsolateInfo.Instance.HiddenObjects.Cast<Object>().ToArray(), "DeIsolate");
				IsolateInfo.Show();
			}

			Undo.DestroyObjectImmediate(IsolateInfo.Instance.gameObject);
		}

		private static string GetShortcutDisplay()
		{
			var display = "";
			if (_ctrl) display += "Ctrl+";
			if (_alt) display += "Alt+";
			if (_shift) display += "Shift+";
			display += _hotkey;
			return display;
		}


		#region Utils

		private static bool IsParent(this Transform parent, Transform transform)
		{
			while (parent)
			{
				if (parent == transform) return true;
				parent = parent.parent;
			}

			return false;
		}

		private static bool IsParent(this GameObject parent, GameObject go) { return parent && go && IsParent(parent.transform, go.transform); }

		private static bool IsRelative(this GameObject go1, GameObject go2) { return go2.IsParent(go1) || go1.IsParent(go2); }

		private static IEnumerable<Transform> GetChildren(this Transform t)
		{
			var children = new List<Transform>();
			for (var i = 0; i < t.childCount; i++) children.Add(t.GetChild(i));
			return children;
		}

		private static IEnumerable<GameObject> GetRootSceneObjects()
		{
#if UNITY_5_3_OR_NEWER
			return SceneManager.GetActiveScene().GetRootGameObjects();
#else
			var prop = new HierarchyProperty(HierarchyType.GameObjects);
			var expanded = new int[0];
			while (prop.Next(expanded)) yield return prop.pptrValue as GameObject;
#endif
		}

		private static IEnumerable<Transform> GetRootTransforms() { return GetRootSceneObjects().Where(go => go).Select(go => go.transform); }

		private static bool IsNullOrEmpty(this string str) { return string.IsNullOrEmpty(str); }

		#endregion
	}
}