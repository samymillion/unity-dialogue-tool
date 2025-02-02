using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Custom language editor which allows adding/removing languages, adding/removing/renaming keys and setting translation values
/// </summary>
public class LanguageEditorWindow : EditorWindow
{
    private List<LanguageScriptableObj> allLanguages;
    private LanguageScriptableObj selectedLanguage;
    private ListView languageListView;
    private ScrollView keyScrollView;
    private VisualElement leftPane;
    private string selectedKey;

    [MenuItem("Window/Localization/Language Editor")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<LanguageEditorWindow>();
        wnd.titleContent = new GUIContent("Language Editor");
    }

    private void OnEnable()
    {
        LocalizationUtilities.RefreshLanguages();
    }

    public void CreateGUI()
    {
        allLanguages = LocalizationUtilities.LoadLanguages();

        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        rootVisualElement.Add(splitView);

        // Left Pane: Language List
        leftPane = new VisualElement();
        splitView.Add(leftPane);
        leftPane.style.flexGrow = 1;

        // Add language management toolbar
        var languageToolbar = new Toolbar();
        languageToolbar.Add(new Button(AddLanguage) { text = "Add Language" });
        languageToolbar.Add(new Button(RemoveLanguage) { text = "Remove Language" });
        leftPane.Add(languageToolbar);

        // Set item source for language list
        languageListView = new ListView
        {
            itemsSource = allLanguages,
            makeItem = () => new Label(),
            bindItem = (element, index) =>
            {
                (element as Label).text = allLanguages[index].name;
            },
            selectionType = SelectionType.Single
        };
        languageListView.style.flexGrow = 1;
        languageListView.selectionChanged += OnLanguageSelected;
        leftPane.Add(languageListView);

        // Right Pane: Key Management
        var rightPane = new VisualElement();
        splitView.Add(rightPane);
        rightPane.style.flexGrow = 2;

        // Add key management toolbar
        var keyToolbar = new Toolbar();
        keyToolbar.Add(new Button(AddKey) { text = "Add Key" });
        rightPane.Add(keyToolbar);

        // Key Management ScrollView
        keyScrollView = new ScrollView(ScrollViewMode.Vertical);
        keyScrollView.style.flexGrow = 1;
        rightPane.Add(keyScrollView);
    }
    
    // Update editor view when new language is selected
    private void OnLanguageSelected(IEnumerable<object> selectedItems)
    {
        keyScrollView.Clear();

        foreach (var selected in selectedItems)
        {
            selectedLanguage = selected as LanguageScriptableObj;
            if (selectedLanguage != null)
            {
                DrawKeyEditor(selectedLanguage);
                break;
            }
        }
    }

    // Redraw list of languages
    private void DrawLanguageListView()
    {
        languageListView.Clear();
        languageListView = new ListView
        {
            itemsSource = allLanguages,
            makeItem = () => new Label(),
            bindItem = (element, index) =>
            {
                (element as Label).text = allLanguages[index].name;
            },
            selectionType = SelectionType.Single
        };
        languageListView.style.flexGrow = 1;
        languageListView.selectionChanged += OnLanguageSelected;
    }

    // Draw list of keys and values for the selected language
    private void DrawKeyEditor(LanguageScriptableObj language)
    {
        keyScrollView.Clear();

        // Sort the entries by the key to display in order
        var sortedEntries = language.entries.OrderBy(entry => entry.key).ToList();


        foreach (var entry in sortedEntries)
        {
            var foldout = new Foldout { text = entry.key };
            foldout.style.marginTop = 5;
            foldout.style.marginBottom = 5;
            foldout.value = false; 

            // Key Name (Editable)
            var keyField = new TextField("Key")
            {
                value = entry.key
            };

            var setKeyButton = new Button(() =>
            {
                Undo.RecordObject(language, "Edit Key");
                language.RenameKey(entry.key, keyField.value); // Update the key name
                language.OnBeforeSerialize();
                EditorUtility.SetDirty(language);
                LocalizationUtilities.SyncLanguages(selectedLanguage);
                DrawKeyEditor(language);
            })
            {
                text = "Set Key"
            };

            // Translation Value Field
            var valueField = new TextField("Value") { value = entry.value };
            valueField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(language, "Edit Value");
                language.SetTranslation(entry.key, evt.newValue);
                language.OnBeforeSerialize();
                EditorUtility.SetDirty(language);
            });


            foldout.style.borderLeftWidth = 1;
            foldout.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f);  // Light grey left border
            foldout.style.borderTopWidth = 1;
            foldout.style.borderTopColor = new Color(0.1f, 0.1f, 0.1f);  // Grey top border
            foldout.style.borderRightWidth = 1;
            foldout.style.borderRightColor = new Color(0.1f, 0.1f, 0.1f);
            foldout.style.borderBottomWidth = 1;
            foldout.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f);
            foldout.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            
            keyField.Add(setKeyButton);
            keyField.Add(new Button(RemoveKey) { text = "Remove Key" });

            var keyRow = new VisualElement();
            keyRow.Add(keyField);
            keyRow.Add(valueField);

            // Register foldout selection
            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    selectedKey = entry.key;  // Set the key to delete if foldout is expanded
            });

            foldout.Add(keyRow);
            keyScrollView.Add(foldout);
        }
    }

    private void AddKey()
    {
        if (selectedLanguage == null) return;

        selectedLanguage.AddEntry("New Key", "New Value");

        selectedLanguage.OnBeforeSerialize();
        EditorUtility.SetDirty(selectedLanguage);

        keyScrollView.Clear();
        DrawKeyEditor(selectedLanguage);

        SyncKeys();
    }

    private void RemoveKey()
    {
        if (selectedLanguage == null) return;

        selectedLanguage.RemoveEntry(selectedKey);
        selectedKey = null;

        selectedLanguage.OnBeforeSerialize();
        EditorUtility.SetDirty(selectedLanguage);

        keyScrollView.Clear();
        DrawKeyEditor(selectedLanguage);

        SyncKeys();
    }

    private void AddLanguage()
    {
        var newLanguage = CreateInstance<LanguageScriptableObj>();
        var path = EditorUtility.SaveFilePanelInProject("Save New Language", "NewLanguage", "asset", "Select location to save the new language");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newLanguage, path);
            LocalizationUtilities.RefreshLanguages();

            allLanguages = LocalizationUtilities.LoadLanguages();
            LocalizationUtilities.SyncLanguages(selectedLanguage);
            selectedLanguage = newLanguage;

            languageListView.itemsSource = LocalizationUtilities.LoadLanguages();
            DrawLanguageListView();
        }
    }

    private void RemoveLanguage()
    {
        if (selectedLanguage == null)
            return;

        var path = AssetDatabase.GetAssetPath(selectedLanguage);
        AssetDatabase.DeleteAsset(path);
        LocalizationUtilities.RefreshLanguages() ;

        allLanguages = LocalizationUtilities.LoadLanguages();
        selectedLanguage = null;

        languageListView.itemsSource = LocalizationUtilities.LoadLanguages();  // Update the ListView
        keyScrollView.Clear();

        DrawLanguageListView();
    }

    private void SyncKeys()
    {
        var allKeys = new HashSet<string>();

        // Collect all keys from the selected language
        foreach (var entry in selectedLanguage.entries)
        {
            allKeys.Add(entry.key);
        }

        // Update all languages with the keys
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
                        language.OnBeforeSerialize();
                        EditorUtility.SetDirty(language);
                    }
                }

                // Remove keys that no longer exist in the selected language
                foreach (var entry in language.entries.ToList())
                {
                    if (!allKeys.Contains(entry.key))
                    {
                        language.RemoveEntry(entry.key); // Remove extra key
                        language.OnBeforeSerialize();
                        EditorUtility.SetDirty(language);
                    }
                }
            }
        }
    }
}
