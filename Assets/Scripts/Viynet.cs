using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class Vinyet : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float siddet = 0.85f;
    [SerializeField, Range(0.2f, 1f)] private float baslangic = 0.45f;

    private void Awake()
    {
        int n = 256;
        Texture2D tex = new Texture2D(n, n);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x / (float)(n - 1) - 0.5f) * 2f;
                float dy = (y / (float)(n - 1) - 0.5f) * 2f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / 1.414f;
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(baslangic, 1f, d)) * siddet;
                tex.SetPixel(x, y, new Color(0, 0, 0, a));
            }
        tex.Apply();
        GetComponent<RawImage>().texture = tex;
    }
}