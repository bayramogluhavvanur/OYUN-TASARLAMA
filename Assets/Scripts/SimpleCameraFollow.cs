using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;   // ana karakter
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -8);
    [SerializeField] private float pitch = 50f;  // yukarıdan bakış açısı

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}