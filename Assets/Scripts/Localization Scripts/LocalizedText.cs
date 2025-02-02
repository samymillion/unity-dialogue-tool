using TMPro;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Class that associates keys with text objects, handles updating text
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private string key;

    private TMP_Text textComponent;

    public string Key
    {
        get => key;
        set
        {
            if (key != value)
            {
                key = value;
                UpdateText();

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    // Ensure the changes are saved in the editor
                    EditorUtility.SetDirty(this);
                }
#endif
            }
        }
    }

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        UpdateText();
    }
    private void OnEnable()
    {
        StartCoroutine(SubscribeToLocalizationManager());
    }
    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }

    private IEnumerator SubscribeToLocalizationManager()
    {
        // Wait until LocalizationManager.Instance is available
        while (LocalizationManager.Instance == null)
        {
            yield return null;
        }

        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        UpdateText(); // Update text after subscribing
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            // Ensure textComponent is assigned in the editor
            if (textComponent == null)
            {
                textComponent = GetComponent<TMP_Text>();
            }
            UpdateText();
        }
    }
#endif


    public void UpdateText()
    {
        Debug.Log($"UpdateText called on {gameObject.name} with key '{key}'");
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }

        if (textComponent == null)
        {
            Debug.LogWarning($"LocalizedText on '{gameObject.name}' is missing a TMP_Text component.");
            return;
        }

        if (LocalizationManager.Instance != null)
        {
            textComponent.text = LocalizationManager.Instance.GetTranslation(key);
            Debug.Log(LocalizationManager.Instance.GetTranslation(key));
        }
        else
        {
#if UNITY_EDITOR
            // In the editor, when LocalizationManager is not available, use the default language directly
            var defaultLanguage = GetDefaultLanguage();
            if (defaultLanguage != null)
            {
                var entry = defaultLanguage.entries.Find(e => e.key == key);
                textComponent.text = entry != null ? entry.value : key;
            }
            else
            {
                textComponent.text = key;
            }
#else
            textComponent.text = key; // Fallback to key if LocalizationManager is not available
#endif
        }
    }

#if UNITY_EDITOR
    private LanguageScriptableObj GetDefaultLanguage()
    {
        // Try to get the default language from a LocalizationManager in the scene
        var manager = FindObjectOfType<LocalizationManager>();
        if (manager != null && manager.defaultLanguage != null)
        {
            return manager.defaultLanguage;
        }
        else
        {
            // Default language set to english if not set in LocalizationManager
            string path = "Assets/Languages/English.asset";
            var defaultLanguage = AssetDatabase.LoadAssetAtPath<LanguageScriptableObj>(path);

            if (defaultLanguage == null)
            {
                Debug.LogWarning("Default language asset not found at path: " + path);
            }

            return defaultLanguage;
        }
    }
#endif

    public void UpdateTextInEditor()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UpdateText();
            EditorUtility.SetDirty(textComponent);
        }
#endif
    }
}
