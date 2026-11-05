using UnityEngine;

public class OdaTetikleyici : MonoBehaviour
{
    [SerializeField] private CompanionAgent ajan;   // bu odadaki ajan

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            ajan.OdaTetikle();
    }
}