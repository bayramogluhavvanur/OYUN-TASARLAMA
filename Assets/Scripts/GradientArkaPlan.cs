using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class GradientArkaPlan : MonoBehaviour
{
    [SerializeField] private Color ust = new Color32(0x05, 0x07, 0x1A, 255);
    [SerializeField] private Color alt = new Color32(0x0B, 0x12, 0x33, 255);

    private void Awake()
    {
        Texture2D tex = new Texture2D(1, 64);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 64; y++)
            tex.SetPixel(0, y, Color.Lerp(alt, ust, y / 63f));
        tex.Apply();
        GetComponent<RawImage>().texture = tex;
    }
}