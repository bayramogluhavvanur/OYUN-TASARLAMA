using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>LLM'in JSON çıktısındaki eylemi oyun durumuna bağlar: Sadece kapıyı açar.</summary>
public class DoorUnlockHandler : MonoBehaviour, INpcActionHandler
{
    [SerializeField] GameObject door;

    bool done;

    public void Execute(NpcAction action)
    {
        // Eğer eylem OpenDoor değilse veya zaten yapıldıysa işlem yapma
        if (action != NpcAction.OpenDoor || done) return;
        done = true;

        // Şifre (veya komut) doğruysa sadece kapıyı ortadan kaldır/aç
        if (door != null) door.SetActive(false);
    }
}