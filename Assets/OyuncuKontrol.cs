using UnityEngine;

public class OyuncuKontrol : MonoBehaviour
{
    public float hiz = 5f;

    void Update()
    {
        // Klavyeden (WASD veya Yön Tuşları) veri al
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Karakteri hareket ettir
        Vector3 hareket = new Vector3(x, 0, z);
        transform.Translate(hareket * hiz * Time.deltaTime, Space.World);
    }
}
