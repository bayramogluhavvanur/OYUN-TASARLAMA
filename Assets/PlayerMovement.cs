using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float hiz = 6f;
    public float donmeHizi = 150f;

    void Update()
    {
        float yuru = Input.GetAxis("Vertical") * hiz * Time.deltaTime;
        float don = Input.GetAxis("Horizontal") * donmeHizi * Time.deltaTime;

        // İleri-geri yürüme ve sağa-sola dönme işlemi
        transform.Translate(0, 0, yuru);
        transform.Rotate(0, don, 0);
    }
}