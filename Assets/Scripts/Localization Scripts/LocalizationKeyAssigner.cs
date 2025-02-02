using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// Custom Editor which displays all existing LocalizedText objects and their keys
/// Allows quick assigning/reassinging of keys
/// </summary>
public class LocalizationKeyAssigner : EditorWindow
{
    private Vector2 scrollPosition;
    private List<LocalizedText> localizedTextComponents;
    private List<string> allKeys;
    private Dictionary<LocalizedText, int> keySelections;
    private LanguageScriptableObj defaultLanguage;

    [MenuItem("Window/Localization/Key Assigner")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationKeyAssigner>("Localization Key Assigner");
    }

    private void OnEnable()
    {
        RefreshData();
    }

    private void RefreshData()
    {

        allKeys = LocalizationUtilities.GetAllKeys();

        // Find all LocalizedText components in the scene
        localizedTextComponents = FindObjectsOfType<LocalizedText>().ToList();

        // Initialize key selections dictionary
        keySelections = new Dictionary<LocalizedText, int>();

        foreach (var localizedText in localizedTextComponents)
        {
            // Find the index of the current key in the allKeys list
            int selectedIndex = allKeys.IndexOf(localizedText.Key);
            if (selectedIndex < 0)
                selectedIndex = 0;

            keySelections[localizedText] = selectedIndex;
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            RefreshData();
        }

        if (allKeys == null || allKeys.Count == 0)
        {
            EditorGUILayout.HelpBox("No localization keys found. Please add keys to your localization data.", MessageType.Warning);
            return;
        }

        // Remove destroyed components from the list
        localizedTextComponents.RemoveAll(item => item == null);

        // Remove destroyed components from the keySelections dictionary
        var keysToRemove = keySelections.Keys.Where(key => key == null).ToList();
        foreach (var key in keysToRemove)
        {
            keySelections.Remove(key);
        }

        if (localizedTextComponents.Count == 0)
        {
            EditorGUILayout.HelpBox("No LocalizedText components found in the scene.", MessageType.Info);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var localizedText in localizedTextComponents)
        {
            if (localizedText == null)
            {
                continue;
            }

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.ObjectField("GameObject", localizedText.gameObject, typeof(GameObject), true);

            // Get the current selection index for this LocalizedText component
            int selectedIndex = keySelections.ContainsKey(localizedText) ? keySelections[localizedText] : 0;

            // Display a popup to select a key
            int newSelectedIndex = EditorGUILayout.Popup("Key", selectedIndex, allKeys.ToArray());

            if (newSelectedIndex != selectedIndex)
            {
                // Update the selection
                keySelections[localizedText] = newSelectedIndex;

                // Update the LocalizedText component
                Undo.RecordObject(localizedText, "Change Localization Key");
                localizedText.Key = allKeys[newSelectedIndex];

                // Update the text immediately in the editor
                localizedText.UpdateTextInEditor();

                EditorUtility.SetDirty(localizedText);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }
}
