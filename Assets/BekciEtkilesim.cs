using UnityEngine;

public class BekciEtkilesim : MonoBehaviour
{
    public OllamaManager ollamaManager;

    private void OnTriggerEnter(Collider other)
    {
        // Kendi karakterinin Hierarchy'deki adı neyse "oyuncu" veya "player" kelimesini ona göre yazabilirsin
        if (other.gameObject.name.ToLower().Contains("player") || other.gameObject.name.ToLower().Contains("oyuncu"))
        {
            ollamaManager.PaneliAc();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.ToLower().Contains("player") || other.gameObject.name.ToLower().Contains("oyuncu"))
        {
            ollamaManager.PaneliKapat();
        }
    }
}