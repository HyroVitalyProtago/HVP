using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// TODO
/// - add static possibility
/// - add custom possibility (text input)
/// - add disable on ... ?
/// </summary>
[CustomEditor(typeof(MonoBehaviourExtended), true)]
public class MonoBehaviourExtendedEditor : Editor {
    
    static GUIContent m_IconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
    
    SerializedProperty p_events, p_listeners;

    string[] allEvents; // All available global events.
    string[] eventOptions; // All local events.
    string[] callbackOptions; // All local callbacks.
    bool reloadEvents; // True when allEvents need to be reloaded.

    bool showCommunicationSystem;
    GUIContent[] m_EventTypes;

    /// <summary>
    /// Return all events of enabled MonoBehaviourExtended.
    /// </summary>
    static string[] AllEvents() {
        HashSet<string> allEvents = new HashSet<string>();
        allEvents.Add("No Event");
        foreach (MonoBehaviourExtended mbx in FindObjectsOfType<MonoBehaviourExtended>()) {
            if (!mbx.enabled) {
                continue;
            }

            string[] localEvents = typeof(MonoBehaviourExtended)
                .GetField("events", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .GetValue(mbx) as string[];

            if (localEvents != null) {
                foreach (string eventName in localEvents) {
                    allEvents.Add(eventName);
                }
            }
        }
        return allEvents.ToArray();
    }

    void OnEnable() {
        allEvents = AllEvents();

        eventOptions = target
            .GetType()
            .GetMembers(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(mi => mi.MemberType == MemberTypes.Event)
            .Select(mi => mi.Name)
            .ToArray();

        callbackOptions = target
            .GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Select(mi => mi.Name)
            .ToArray();
        callbackOptions = callbackOptions.Prepend("No Function");

        reloadEvents = false;

        m_EventTypes = new GUIContent[eventOptions.Length];
        for (int i = 0; i < eventOptions.Length; ++i) {
            m_EventTypes[i] = new GUIContent(eventOptions[i]);
        }

        p_events = serializedObject.FindProperty("events");
        p_listeners = serializedObject.FindProperty("listeners");

        showCommunicationSystem = p_events.arraySize > 0 || p_listeners.arraySize > 0;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        // Update available events
        if (reloadEvents) {
            allEvents = AllEvents();
            reloadEvents = false;
        }

        DrawDefaultInspector();

        // EditorGUILayout.HelpBox("Never disable a MonoBehaviourExtended component in Editor !", MessageType.Warning);
        showCommunicationSystem = EditorGUILayout.Foldout(showCommunicationSystem, "Communication System");
        if (showCommunicationSystem) {
            DrawCommunicationSystem();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawCommunicationSystem() {
        DrawEventSystem();
        DrawListenerSystem();
    }

    // TODO check if all offered events are still existing
    void DrawEventSystem() {
        if (eventOptions.Length <= 0) {
            return;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

        int toBeRemovedEntry = -1;
        Vector2 removeButtonSize = GUIStyle.none.CalcSize(m_IconToolbarMinus);

        for (int i = 0; i < p_events.arraySize; ++i) {
            SerializedProperty delegateProperty = p_events.GetArrayElementAtIndex(i);

            EditorGUILayout.LabelField(delegateProperty.stringValue);
            Rect callbackRect = GUILayoutUtility.GetLastRect();

            Rect removeButtonPos = new Rect(callbackRect.xMax - removeButtonSize.x - 8, callbackRect.y + 1, removeButtonSize.x, removeButtonSize.y);
            if (GUI.Button(removeButtonPos, m_IconToolbarMinus, GUIStyle.none)) {
                toBeRemovedEntry = i;
            }
        }

        if (toBeRemovedEntry > -1) {
            p_events.DeleteArrayElementAtIndex(toBeRemovedEntry);
            reloadEvents = true; // update allEvents
        }

        if (GUILayout.Button("Add Event")) {
            ShowAddTriggermenu();
        }
    }

    void DrawListenerSystem() {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Listeners", EditorStyles.boldLabel);

        int toBeRemovedEntry = -1;
        Vector2 removeButtonSize = GUIStyle.none.CalcSize(m_IconToolbarMinus);

        for (int i = 0; i < p_listeners.arraySize; ++i) {
            SerializedProperty delegateProperty = p_listeners.GetArrayElementAtIndex(i);
            SerializedProperty eventName = delegateProperty.FindPropertyRelative("eventName");
            SerializedProperty callbackName = delegateProperty.FindPropertyRelative("callbackName");

            EditorGUILayout.LabelField(string.Empty);
            Rect callbackRect = GUILayoutUtility.GetLastRect();

            EditorGUILayout.BeginHorizontal();
            // EditorGUI.PrefixLabel(callbackRect, new GUIContent("On"));

            HVPEditorUtility.Popup(eventName, ref allEvents);
            HVPEditorUtility.Popup(callbackName, ref callbackOptions);

            // Rect upRect = GUILayoutUtility.GetLastRect();
            // upRect.Set(upRect.x, callbackRect.y, upRect.width, upRect.height);
            // EditorGUI.PrefixLabel(upRect, new GUIContent("Call"));
            EditorGUILayout.EndHorizontal();

            Rect removeButtonPos = new Rect(callbackRect.xMax - removeButtonSize.x - 8, callbackRect.y + 1, removeButtonSize.x, removeButtonSize.y);
            if (GUI.Button(removeButtonPos, m_IconToolbarMinus, GUIStyle.none)) {
                toBeRemovedEntry = i;
            }
        }

        if (toBeRemovedEntry > -1) {
            p_listeners.DeleteArrayElementAtIndex(toBeRemovedEntry);
        }

        if (GUILayout.Button("Add Listener")) {
            p_listeners.arraySize++;
            SerializedProperty delegateProperty = p_listeners.GetArrayElementAtIndex(p_listeners.arraySize - 1);
            delegateProperty.FindPropertyRelative("eventName").stringValue = string.Empty;
            delegateProperty.FindPropertyRelative("callbackName").stringValue = string.Empty;
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    void ShowAddTriggermenu() {
        // Now create the menu, add items and show it
        GenericMenu menu = new GenericMenu();
        for (int i = 0; i < eventOptions.Length; ++i) {
            bool active = true;

            // Check if we already have a Entry for the current eventType, if so, disable it
            for (int p = 0; p < p_events.arraySize; ++p) {
                if (p_events.GetArrayElementAtIndex(p).stringValue == eventOptions[i]) {
                    active = false;
                    break;
                }
            }

            if (active) {
                menu.AddItem(m_EventTypes[i], false, OnAddNewSelected, i);
            } else {
                menu.AddDisabledItem(m_EventTypes[i]);
            }
        }
        menu.ShowAsContext();
        Event.current.Use();
    }

    void OnAddNewSelected(object index) {
        p_events.arraySize++;
        p_events.GetArrayElementAtIndex(p_events.arraySize - 1).stringValue = eventOptions[(int)index];
        serializedObject.ApplyModifiedProperties();
        reloadEvents = true; // update allEvents
    }
}
