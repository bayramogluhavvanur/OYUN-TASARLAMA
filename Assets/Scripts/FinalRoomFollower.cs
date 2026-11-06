using UnityEngine;
using UnityEngine.AI;

public class FinalRoomFollower : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private GameObject[] gates;
    [SerializeField] private GameObject[] finalGatesToOpen;
    [SerializeField] private int finalButtonIndex = 4;   // WP_Son'un sırası (senin listende 4)

    private NavMeshAgent nav;
    private int index = 0;
    private bool finalDone = false;
    [SerializeField] private Transform exitSwitch;   // Exit_Switch / Switch_Trigger

    private void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!finalDone && exitSwitch != null &&
    Vector3.Distance(transform.position, exitSwitch.position) < 2.5f)
        {
            finalDone = true;
            foreach (var g in finalGatesToOpen)
                if (g != null) g.SetActive(false);
            if (index <= finalButtonIndex) index = finalButtonIndex + 1;   // çıkışa geç
            Debug.Log("Ajan son butona bastı (mesafe)");
        }

        if (index >= waypoints.Length) return;

        // 1) Bu waypoint'in kapısı hâlâ kapalıysa bekle
        if (index < gates.Length && gates[index] != null && gates[index].activeSelf)
        {
            Debug.Log("BEKLIYOR: index=" + index + " kapi=" + gates[index].name);
            nav.isStopped = true;
            return;
        }

        // 2) Son butona basılmadan çıkışa gitme
        if (index > finalButtonIndex && !finalDone)
        {
            nav.isStopped = true;
            return;
        }

        nav.isStopped = false;
        nav.SetDestination(waypoints[index].position);

        // 3) Waypoint'e vardı mı?
        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance + 0.5f)
        {
            if (index == finalButtonIndex && !finalDone)
            {
                finalDone = true;
                foreach (var g in finalGatesToOpen)
                    if (g != null) g.SetActive(false);
                Debug.Log("Ajan son butona bastı");
            }
            index++;
        }
    }
}