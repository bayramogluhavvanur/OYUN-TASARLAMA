using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject hikayePanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Hikaye")]
    [SerializeField] private TextMeshProUGUI hikayeText;
    [SerializeField] private float harfHizi = 0.04f;
    [TextArea(2, 5)]
    [SerializeField] private string[] cumleler;

    [Header("İlerleme Göstergesi")]
    [SerializeField] private Transform noktaKapsayici;   // IlerlemeNoktalari
    [SerializeField] private GameObject noktaPrefab;
    [SerializeField] private Color aktifRenk = new Color(0.13f, 0.83f, 0.93f, 1f);
    [SerializeField] private Color pasifRenk = new Color(1f, 1f, 1f, 0.25f);

    private Image[] noktalar;
    private int index = 0;
    private bool yaziyor = false;
    private bool hikayeAktif = false;
    private bool bitiyor = false;
    private Coroutine aktif;

    private void Start()
    {
        Time.timeScale = 0f;
        menuPanel.SetActive(true);
        hikayePanel.SetActive(false);
        canvasGroup.alpha = 1f;
        NoktalariOlustur();
    }

    private void NoktalariOlustur()
    {
        noktalar = new Image[cumleler.Length];
        for (int i = 0; i < cumleler.Length; i++)
        {
            var go = Instantiate(noktaPrefab, noktaKapsayici);
            noktalar[i] = go.GetComponent<Image>();
        }
    }

    private void NoktalariGuncelle()
    {
        for (int i = 0; i < noktalar.Length; i++)
        {
            noktalar[i].color = (i == index) ? aktifRenk : pasifRenk;
            noktalar[i].transform.localScale = (i == index) ? Vector3.one * 1.3f : Vector3.one;
        }
    }

    public void OyunaBasla()
    {
        menuPanel.SetActive(false);
        hikayePanel.SetActive(true);
        hikayeAktif = true;
        index = 0;
        Goster(0);
    }

    public void Cikis()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void Update()
    {
        if (!hikayeAktif || bitiyor) return;

        // ESC veya TAB: hikayeyi atla
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            StartCoroutine(Bitir());
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (yaziyor)
            {
                StopCoroutine(aktif);
                hikayeText.text = cumleler[index];
                yaziyor = false;
            }
            else
            {
                index++;
                if (index >= cumleler.Length) StartCoroutine(Bitir());
                else Goster(index);
            }
        }
    }

    private void Goster(int i)
    {
        NoktalariGuncelle();
        aktif = StartCoroutine(Yaz(cumleler[i]));
    }

    private IEnumerator Yaz(string metin)
    {
        yaziyor = true;
        hikayeText.text = "";
        foreach (char c in metin)
        {
            hikayeText.text += c;
            yield return new WaitForSecondsRealtime(harfHizi);
        }
        yaziyor = false;
    }

    private IEnumerator Bitir()
    {
        bitiyor = true;
        hikayeAktif = false;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 1.5f;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}