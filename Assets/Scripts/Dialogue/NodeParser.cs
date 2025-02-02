using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using TMPro;

public class NodeParser : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueGraph[] dialogueGraphs;
    public DialogueGraph currentDialogueGraph;

    [Header("UI Elements")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI promptText;
    public GameObject dialogueBox;
    public GameObject buttonPrefab;
    public Transform buttonParent;

    private DialogueNode activeNode;
    private Coroutine parserCoroutine;

    private void Start()
    {
        if (dialogueGraphs == null || dialogueGraphs.Length == 0)
        {
            Debug.LogError("DialogueGraph array is empty or null.");
            return;
        }

        InitializeGraph();
    }
    private void OnEnable()
    {
        StartCoroutine(SubscribeToLocalizationManager());
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= RefreshUI;
        }
    }

    private IEnumerator SubscribeToLocalizationManager()
    {
        // Wait until LocalizationManager.Instance is available
        while (LocalizationManager.Instance == null)
        {
            yield return null;
        }

        LocalizationManager.Instance.OnLanguageChanged += RefreshUI;
        RefreshUI(); // Update text after subscribing
    }

    private void InitializeGraph()
    {
        try
        {
            foreach (DialogueNode node in currentDialogueGraph.nodes)
            {
                if (node is DialogueNode && node.GetPort("entry") != null)
                {
                    currentDialogueGraph.current = node;
                    break;
                }
            }
        }
        catch
        {
            Debug.LogError("Error initializing graph. Ensure the graph has nodes and a start node is connected.");
        }

        if (currentDialogueGraph.current != null)
        {
            parserCoroutine = StartCoroutine(ParseNode());
        }
        else
        {
            Debug.LogError("No starting node found in the current graph.");
        }
    }

    public void OnResponseClicked(int responseIndex)
    {
        if (parserCoroutine != null)
        {
            StopCoroutine(parserCoroutine);
            parserCoroutine = null;
        }

        var port = activeNode.GetPort("responseKeys " + responseIndex);
        if (port != null && port.IsConnected)
        {
            currentDialogueGraph.current = port.Connection.node as DialogueNode;
            parserCoroutine = StartCoroutine(ParseNode());
        }
        else
        {
            Debug.LogError($"Response port at index {responseIndex} is not connected.");
            dialogueBox.SetActive(false);
        }
    }

    private IEnumerator ParseNode()
    {
        BaseNode current = currentDialogueGraph.current;
        if (current == null || !(current is DialogueNode))
        {
            Debug.LogError("Current node is invalid or not a DialogueNode.");
            yield break;
        }

        activeNode = (DialogueNode)current;

        // Update UI
        UpdateDialogueBox();

        yield return new WaitUntil(() => dialogueBox.activeSelf);
    }

    private void UpdateDialogueBox()
    {
        // Update speaker text
        speakerText.text = activeNode.speakerName;

        // Update prompt text using localization
        promptText.SetText(LocalizationManager.Instance.GetTranslation(activeNode.promptKey));

        // Clear previous buttons
        foreach (Transform child in buttonParent)
        {
            var button = child.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners(); // Remove any lingering listeners
            }
            Destroy(child.gameObject); // Destroy the button
        }

        // Wait for previous buttons to be fully cleaned up
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

        // Create buttons for responses
        for (int i = 0; i < activeNode.responseKeys.Count; i++)
        {
            var response = activeNode.responseKeys[i];

            // Instantiate button
            var button = Instantiate(buttonPrefab, buttonParent);

            // Force enable all components on the button
            button.SetActive(true); // Ensure the root GameObject is enabled
            var image = button.GetComponent<Image>();
            if (image != null) image.enabled = true;

            var buttonComponent = button.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.enabled = true;

                // Add listener
                var responseIndex = i;
                buttonComponent.onClick.AddListener(() => OnResponseClicked(responseIndex));
            }

            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.enabled = true;
                buttonText.SetText(LocalizationManager.Instance.GetTranslation(response));
            }
        }

        // Show dialogue box
        dialogueBox.SetActive(true);
    }

    public void EndDialogue()
    {
        Debug.Log("Dialogue ended.");

        // Hide the dialogue box
        dialogueBox.SetActive(false);

        // Clear the buttons
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        activeNode = null;
    }

    public void RefreshUI()
    {
        UpdateDialogueBox();
    }
}
