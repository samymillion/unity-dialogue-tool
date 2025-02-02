using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Was going to be a class for shared functionality between classes but mostly is just used by the custom language editor :(
/// </summary>
public static class LocalizationUtilities
{
    private static List<LanguageScriptableObj> cachedLanguages;

    public static List<LanguageScriptableObj> LoadLanguages()
    {
        if (cachedLanguages == null)
        {
            cachedLanguages = new List<LanguageScriptableObj>();
            var allObjectGuids = AssetDatabase.FindAssets("t:LanguageScriptableObj");

            foreach (var guid in allObjectGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var lang = AssetDatabase.LoadAssetAtPath<LanguageScriptableObj>(path);
                if (lang != null)
                    cachedLanguages.Add(lang);
            }
        }

        return cachedLanguages;
    }
    public static List<string> GetAllKeys()
    {
        var allLanguages = LoadLanguages();
        var allKeys = new List<string>();

        foreach (var language in allLanguages)
        {
            foreach (var entry in language.entries)
            {
                allKeys.Add(entry.key);
            }
        }

        return allKeys;
    }

    public static void RefreshLanguages()
    {
        cachedLanguages = null; // Clear the cache
        LoadLanguages();        // Reload the languages
    }

    public static void SyncLanguages(LanguageScriptableObj selectedLanguage)
    {
        var allKeys = new HashSet<string>();

        // Collect all keys and GameObjects from the selected language
        foreach (var entry in selectedLanguage.entries)
        {
            allKeys.Add(entry.key);
        }
        var allLanguages = LoadLanguages();

        foreach (var language in allLanguages)
        {
            if (language != selectedLanguage)
            {
                // Ensure that all keys are present in every language
                foreach (var key in allKeys)
                {
                    if (!language.HasKey(key))
                    {
                        language.AddEntry(key, ""); // Add missing key
                    }
                }

                // Remove keys that no longer exist in the selected language
                foreach (var entry in language.entries.ToList())
                {
                    if (!allKeys.Contains(entry.key))
                    {
                        language.RemoveEntry(entry.key); // Remove extra key
                    }
                }
            }
        }
    }
}
   

