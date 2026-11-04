using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float hiz = 6f;
    public float donmeHizi = 10f;

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 yon = new Vector3(x, 0, z);

        if (yon.sqrMagnitude > 0.01f)
        {
            transform.position += yon.normalized * hiz * Time.deltaTime;

            Quaternion hedef = Quaternion.LookRotation(yon);
            transform.rotation = Quaternion.Slerp(transform.rotation, hedef, donmeHizi * Time.deltaTime);
        }
    }
}