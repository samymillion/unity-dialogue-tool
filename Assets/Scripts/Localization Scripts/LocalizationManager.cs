using System;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Icons;

/// <summary>
/// Handles runtime translation, broadcasts language changes to LocalizedText objects
/// You should have a LocalizationManager object in your scene
/// You can set the default language in the inspector to whatever you'd like it to be
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    public LanguageScriptableObj defaultLanguage;
    private LanguageScriptableObj currentLanguage;

    public event Action OnLanguageChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentLanguage = defaultLanguage;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLanguage(LanguageScriptableObj language)
    {
        if (language == null || currentLanguage == language) return;

        currentLanguage = language;
        Debug.Log($"Language changed to: {language.name}");
        OnLanguageChanged?.Invoke(); // Notify subscribers
    }

    public string GetTranslation(string key)
    {
        if (currentLanguage == null) return key;
        Debug.Log($"Getting translation for: {currentLanguage.name}");
        var entry = currentLanguage.entries.Find(e => e.key == key);
        return entry != null ? entry.value : key; // Return the key as a fallback
    }
}
