using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IDialogueView
{
    event Action<string> MessageSubmitted;
    event Action CloseRequested;
    bool IsOpen { get; }
    void Open();
    void Close();
    void ShowNpcText(string text, NpcMood mood);
    void ShowThinking(bool on);
    void ShowStatus(string message);
    void SetInputEnabled(bool on);
}

/// <summary>Bu script HER ZAMAN AKTİF bir objede (ör. Canvas) durmalı; panelRoot ise açılıp kapanan panel olmalı.</summary>
public class DialogueView : MonoBehaviour, IDialogueView
{
    [SerializeField] GameObject panelRoot;
    [SerializeField] TMP_InputField input;
    [SerializeField] Button sendButton;
    [SerializeField] Button closeButton;
    [SerializeField] TMP_Text npcText;
    [SerializeField] TMP_Text statusText;
    [SerializeField] string thinkingLabel = "Bekçi düşünüyor";
    [SerializeField] int maxInputLength = 300;

    public event Action<string> MessageSubmitted;
    public event Action CloseRequested;

    string lastNpcText = "";
    Coroutine thinkingRoutine;

    public bool IsOpen { get { return panelRoot != null && panelRoot.activeSelf; } }

    void Awake()
    {
        input.characterLimit = maxInputLength;
        sendButton.onClick.AddListener(Submit);
        input.onSubmit.AddListener(delegate { Submit(); });
        if (closeButton != null) closeButton.onClick.AddListener(delegate { if (CloseRequested != null) CloseRequested(); });
        panelRoot.SetActive(false);
    }

    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape) && CloseRequested != null) CloseRequested();
#endif
    }

    void Submit()
    {
        string t = input.text.Trim();
        if (t.Length == 0) return;
        input.text = "";
        if (MessageSubmitted != null) MessageSubmitted(t);
    }

    public void Open()
    {
        panelRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        npcText.text = lastNpcText;
        input.ActivateInputField();
    }

    public void Close()
    {
        ShowThinking(false);
        panelRoot.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowNpcText(string text, NpcMood mood)
    {
        lastNpcText = text;
        npcText.text = text;
        switch (mood)
        {
            case NpcMood.Pleased: npcText.color = new Color(0.6f, 1f, 0.6f); break;
            case NpcMood.Suspicious: npcText.color = new Color(1f, 0.9f, 0.5f); break;
            case NpcMood.Angry: npcText.color = new Color(1f, 0.5f, 0.5f); break;
            default: npcText.color = Color.white; break;
        }
    }

    public void ShowThinking(bool on)
    {
        if (thinkingRoutine != null) { StopCoroutine(thinkingRoutine); thinkingRoutine = null; }
        if (on && gameObject.activeInHierarchy) thinkingRoutine = StartCoroutine(ThinkingAnim());
        else npcText.text = lastNpcText;
    }

    IEnumerator ThinkingAnim()
    {
        npcText.color = new Color(0.8f, 0.8f, 0.8f);
        int n = 0;
        while (true)
        {
            npcText.text = thinkingLabel + new string('.', n % 4);
            n++;
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }

    public void ShowStatus(string message) { if (statusText != null) statusText.text = message; }

    public void SetInputEnabled(bool on)
    {
        input.interactable = on;
        sendButton.interactable = on;
        if (on && IsOpen) input.ActivateInputField();
    }
}