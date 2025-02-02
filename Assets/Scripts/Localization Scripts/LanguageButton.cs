using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Custom button class, creates functionality for changing language
/// Button prefab exists with this script attached, use those if you want to make another language button
/// </summary>
public class LanguageButton : MonoBehaviour
{
    public LanguageScriptableObj language; // Assign this in the Inspector
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnLanguageButtonClick);
    }

    private void OnLanguageButtonClick()
    {
        LocalizationManager.Instance.SetLanguage(language);
    }
}