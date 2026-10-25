using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NpcInteractionTrigger : MonoBehaviour
{
    [SerializeField] NpcDialogueController dialogue;
    [SerializeField] string playerTag = "Player";

    bool inside;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger'a girildi: " + other.name + " tag=" + other.tag + " dialogue=" + (dialogue == null ? "NULL" : dialogue.name));
        if (dialogue == null || !other.CompareTag(playerTag)) return;
        inside = true;
        dialogue.BeginConversation();
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger'dan çıkıldı: " + other.name);
        if (dialogue == null || !other.CompareTag(playerTag)) return;
        inside = false;
        dialogue.EndConversation();
    }

    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        // Paneli kapattıktan sonra trigger içindeyken E ile tekrar konuşulabilir
        if (inside && dialogue != null && !dialogue.IsOpen && Input.GetKeyDown(KeyCode.E))
            dialogue.BeginConversation();
#endif
    }
}