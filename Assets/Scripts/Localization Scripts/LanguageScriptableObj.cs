using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom language object
/// Stores kvps as entry structures in a list for serialization
/// Copies to and from dictionary for runtime lookup
/// </summary>
[CreateAssetMenu(fileName = "Language", menuName = "Localization/Language")]
public class LanguageScriptableObj : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string value;
    }

    public List<LocalizationEntry> entries = new List<LocalizationEntry>();

    // For quick access during runtime
    private Dictionary<string, LocalizationEntry> translationDict = new Dictionary<string, LocalizationEntry>();

    public void OnBeforeSerialize()
    {
        // Serialize the list to the dictionary
        translationDict.Clear();
        foreach (var entry in entries)
        {
            if (!translationDict.ContainsKey(entry.key))
                translationDict[entry.key] = entry;
        }
    }

    public void OnAfterDeserialize()
    {
        // Deserialize the dictionary to the list
        foreach (var entry in entries)
        {
            if (!translationDict.ContainsKey(entry.key))
                translationDict[entry.key] = entry;
        }
    }

    public bool HasKey(string key)
    {
        foreach (var entry in entries)
        {
            if (entry.key == key) return true;
        }
        return false;

    }

    public string GetTranslation(string key)
    {
        Debug.Log($"Trying to get GameObject for key: {key}");
        return translationDict.TryGetValue(key, out var entry) ? entry.value : key;
    }

    public void SetTranslation(string key, string value)
    {
        translationDict[key].value = value;
    }

    private string CheckKeyDuplicates(string key)
    {
        string originalKey = key;
        int counter = 1;

        // Check if the key already exists and append suffix until key is original
        while (translationDict.ContainsKey(key))
        {
            key = originalKey + "_" + counter;
            counter++;
        }

        return key;
    }

    public void RenameKey(string oldKey, string newKey)
    {
        translationDict[oldKey].key = CheckKeyDuplicates(newKey);
    }

    public void AddEntry(string key, string value)
    {
        
        string uniqueKey = CheckKeyDuplicates(key);

        var newEntry = new LocalizationEntry
        {
            key = uniqueKey,
            value = value
        };

        entries.Add(newEntry);
        translationDict[uniqueKey] = newEntry;
    }    

    public void RemoveEntry(string key)
    {
        if (translationDict.ContainsKey(key))
        {
            var entry = translationDict[key];
            entries.Remove(entry);
            translationDict.Remove(key);
        }
    }
}
