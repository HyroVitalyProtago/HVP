using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Connector))]
public class ConnectorEditor : Editor {

	static readonly GUIContent m_IconToolbarPlus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Plus"));
	static readonly GUIContent m_IconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
	static readonly Texture2D gray = HVPEditorUtility.GetTexture2DFromColor(new Color(.89f, .89f, .89f));

	SerializedProperty p_connexions;

	GUIStyle _boxStyle;
	GUIStyle boxStyle {
		get {
			if (_boxStyle == null) {
				_boxStyle = new GUIStyle(GUI.skin.box);
				_boxStyle.normal.background = gray;
			}
			return _boxStyle;
		}
	}

    GenericMenu eventMenu, callbackMenu;

    void OnEnable() {
		p_connexions = serializedObject.FindProperty("connexions");

        eventMenu = new GenericMenu();
        eventMenu.AddDisabledItem(new GUIContent("No Event"));

        callbackMenu = new GenericMenu();
        callbackMenu.AddDisabledItem(new GUIContent("No Callback"));
    }

	public override void OnInspectorGUI() {
		serializedObject.Update();

//		EditorGUILayout.HelpBox("Connexions do not have to relies on extern components.", MessageType.Info);
//		DrawDefaultInspector();
		DrawConnexionSystem(); // When <sender> send <event>, <receiver> call <callback>

		serializedObject.ApplyModifiedProperties();
	}

	void DrawConnexionSystem() {
		EditorGUILayout.Space();

		Rect lastRect;
		int toBeRemovedEntry = -1;
		Vector2 removeButtonSize = GUIStyle.none.CalcSize(m_IconToolbarMinus);
		Vector2 plusButtonSize = GUIStyle.none.CalcSize(m_IconToolbarPlus);

		for (int i = 0; i < p_connexions.arraySize; ++i) {
			SerializedProperty delegateProperty = p_connexions.GetArrayElementAtIndex(i);
			SerializedProperty sender = delegateProperty.FindPropertyRelative("sender");
			SerializedProperty receiver = delegateProperty.FindPropertyRelative("receiver");
			SerializedProperty eventName = delegateProperty.FindPropertyRelative("senderEventName");
			SerializedProperty callbackName = delegateProperty.FindPropertyRelative("receiverCallbackName");

			EditorGUILayout.BeginVertical(boxStyle);

            /*
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ToggleLeft("custom", false);
            EditorGUILayout.Popup(0, new[] { "awake", "enable" });
            EditorGUILayout.EndHorizontal();
            */

            EditorGUILayout.Space();
            EditorGUILayout.Space(); // just because it's more comfortable
			lastRect = GUILayoutUtility.GetLastRect();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(sender, GUIContent.none);
			// EditorGUILayout.PropertyField(eventName, GUIContent.none);
		    string[] eventOptions = EventConductor.GetEventsOf(sender.objectReferenceValue as Component);
            EditorGUI.BeginDisabledGroup(eventOptions.Length == 1); // or sender == null
            HVPEditorUtility.Popup(eventName, ref eventOptions);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
            // EditorGUILayout.ToggleLeft("static", true);
            EditorGUILayout.PropertyField(receiver, GUIContent.none);
			// EditorGUILayout.PropertyField(callbackName, GUIContent.none);
		    string[] callbackOptions = EventConductor.GetCallbacksOf(receiver.objectReferenceValue as Component);
            EditorGUI.BeginDisabledGroup(callbackOptions.Length == 1); // or receiver == null
            HVPEditorUtility.Popup(callbackName, ref callbackOptions);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

			Rect removeButtonPos = new Rect(
				lastRect.xMax - removeButtonSize.x - 4,
				lastRect.y - 8,
				removeButtonSize.x,
				removeButtonSize.y
			);
			if (GUI.Button(removeButtonPos, m_IconToolbarMinus, GUIStyle.none)) {
				toBeRemovedEntry = i;
			}

			EditorGUILayout.EndVertical();
		}

		if (toBeRemovedEntry > -1) {
			p_connexions.DeleteArrayElementAtIndex(toBeRemovedEntry);
		}

		EditorGUILayout.BeginHorizontal();
		Rect rect = EditorGUILayout.GetControlRect();
		Rect plusButtonPos = new Rect(
			rect.xMax - plusButtonSize.x - 8,
			rect.y - 1,
			plusButtonSize.x,
			plusButtonSize.y
		);
		if (GUI.Button(plusButtonPos, m_IconToolbarPlus, GUIStyle.none)) {
			p_connexions.arraySize++;
			SerializedProperty delegateProperty = p_connexions.GetArrayElementAtIndex (p_connexions.arraySize - 1);
			delegateProperty.FindPropertyRelative("sender").objectReferenceValue = null;
			delegateProperty.FindPropertyRelative("receiver").objectReferenceValue = null;
			delegateProperty.FindPropertyRelative("senderEventName").stringValue = string.Empty;
			delegateProperty.FindPropertyRelative("receiverCallbackName").stringValue = string.Empty;
			serializedObject.ApplyModifiedProperties();
		}
		EditorGUILayout.EndHorizontal();
	}
}
