using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>LLM'in JSON çıktısındaki eylemi oyun durumuna bağlar: kapıyı açar, senaryoyu bitirir.</summary>
public class DoorUnlockHandler : MonoBehaviour, INpcActionHandler
{
    [SerializeField] GameObject door;
    [SerializeField] float endDelaySeconds = 3f;
    [SerializeField] GameObject endPanel;
    [SerializeField] bool freezeTimeOnEnd = true;
    [SerializeField] UnityEvent onScenarioCompleted;

    bool done;

    public void Execute(NpcAction action)
    {
        if (action != NpcAction.OpenDoor || done) return;
        done = true;
        if (door != null) door.SetActive(false);
        StartCoroutine(FinishRoutine());
    }

    IEnumerator FinishRoutine()
    {
        yield return new WaitForSeconds(endDelaySeconds);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (endPanel != null) endPanel.SetActive(true);
        if (onScenarioCompleted != null) onScenarioCompleted.Invoke();
        if (freezeTimeOnEnd) Time.timeScale = 0f;
    }
}