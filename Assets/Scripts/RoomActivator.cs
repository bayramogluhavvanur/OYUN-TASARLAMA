using UnityEngine;

public class RoomActivator : MonoBehaviour
{
    [SerializeField] private GameObject agentObject;   // bu odanın ajanı

    private void Start()
    {
        if (agentObject != null)
            agentObject.SetActive(false);   // oyun başında ajanı gizle
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && agentObject != null)
            agentObject.SetActive(true);    // ana karakter girince ajanı göster
    }
}