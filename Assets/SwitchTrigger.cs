using UnityEngine;

public class SwitchTrigger : MonoBehaviour
{
    [SerializeField] private GameObject[] doorsToOpen;   // bu butonun açacağı kapılar
    [SerializeField] private string activatorTag = "Player";
    private bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (used || !other.CompareTag(activatorTag)) return;
        used = true;
        foreach (var d in doorsToOpen)
            if (d != null) d.SetActive(false);
        Debug.Log("Ana karakter butona bastı: " + name);
    }
}