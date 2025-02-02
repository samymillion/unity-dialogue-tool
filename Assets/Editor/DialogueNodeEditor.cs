using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(DialogueNode))]
public class DialogueNodeEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();

        var segment = serializedObject.targetObject as DialogueNode;

        NodeEditorGUILayout.PortField(segment.GetPort("entry"));

        GUILayout.Label("Speaker Name");
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("speakerName"), GUIContent.none);

        GUILayout.Label("Prompt Key");
        DrawLocalizationKeyDropdown(serializedObject.FindProperty("promptKey"));

        NodeEditorGUILayout.DynamicPortList(
            "responseKeys", // field name
            typeof(string), // field type
            serializedObject, // serializable object
            NodePort.IO.Input, // new port i/o
            Node.ConnectionType.Override, // new port connection type
            Node.TypeConstraint.None,
            OnCreateReorderableList); // onCreate override. This is where the magic

        foreach (XNode.NodePort dynamicPort in target.DynamicPorts)
        {
            if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort)) continue;
            NodeEditorGUILayout.PortField(dynamicPort);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void OnCreateReorderableList(ReorderableList list)
    {
        list.elementHeightCallback = (int index) => { return 60; };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var segment = serializedObject.targetObject as DialogueNode;

            NodePort port = segment.GetPort("responseKeys " + index);

            // Draw dropdown for responseKeys
            segment.responseKeys[index] = DrawLocalizationKeyDropdownForList(rect, segment.responseKeys[index]);

            if (port != null)
            {
                Vector2 pos = rect.position + (port.IsOutput ? new Vector2(rect.width + 6, 0) : new Vector2(-36, 0));
                NodeEditorGUILayout.PortField(pos, port);
            }
        };
    }

    private void DrawLocalizationKeyDropdown(SerializedProperty property)
    {
        var allKeys = LocalizationUtilities.GetAllKeys(); // Get all keys from LocalizationUtilities
        if (allKeys == null || allKeys.Count == 0)
        {
            allKeys = new List<string> { "No Keys Available" }; // Fallback option
        }

        int selectedIndex = allKeys.IndexOf(property.stringValue);
        if (selectedIndex < 0) selectedIndex = 0;

        selectedIndex = EditorGUILayout.Popup(selectedIndex, allKeys.ToArray());

        property.stringValue = allKeys[selectedIndex] == "No Keys Available" ? "" : allKeys[selectedIndex];
    }


    private string DrawLocalizationKeyDropdownForList(Rect rect, string currentKey)
    {
        var allKeys = LocalizationUtilities.GetAllKeys(); // Get all keys from LocalizationUtilities
        if (allKeys == null || allKeys.Count == 0)
        {
            allKeys = new List<string> { "No Keys Available" }; // Fallback option
        }

        int selectedIndex = allKeys.IndexOf(currentKey);
        if (selectedIndex < 0) selectedIndex = 0;

        selectedIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, 20), selectedIndex, allKeys.ToArray());
        return allKeys[selectedIndex] == "No Keys Available" ? "" : allKeys[selectedIndex];
    }

}
