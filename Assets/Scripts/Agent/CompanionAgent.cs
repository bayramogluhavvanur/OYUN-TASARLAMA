using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class CompanionAgent : Agent
{
    private bool ilkKontrolGecildi = false;
    private bool ikinciKontrolGecildi = false;
    private void Update()
    {
        // Ajan zeminden aşağı (örneğin Y ekseninde -1'in altına) düşerse bölümü bitir
        if (transform.localPosition.y < -20f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }
    [SerializeField] private Transform targetSwitch;  // Şalter / Buton
    [SerializeField] private Transform obstacleWall;  // YER DEĞİŞTİREN ENGEL (YENİ EKLENDİ)
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody rb;
    public Transform spawnPoint;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    // HER BÖLÜM (EPISODE) BAŞINDA SIFIRLAMA
    public override void OnEpisodeBegin()
    {
        Debug.Log("--- BÖLÜM BAŞLADI (Ajan Doğdu) ---"); // TEŞHİS 1

        ilkKontrolGecildi = false;
        ikinciKontrolGecildi = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (obstacleWall != null)
        {
            obstacleWall.localPosition = new Vector3(Random.Range(-2f, 2f), 0.5f, 0f);
        }

        targetSwitch.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(4f, 7f));
        transform.position = spawnPoint.position;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Debug.Log("Ajan Kararı -> X: " + moveX + " | Z: " + moveZ); // TEŞHİS 2

        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        AddReward(-0.001f);
    }

    // AJANIN VERİ TOPLAMASI (Gözlem)
    public override void CollectObservations(VectorSensor sensor)
    {
        // Ajanın ve hedeflerin pozisyonları
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetSwitch.localPosition);

        // Engelin ajana göre MESAFESİ / YÖNÜ (Vector3 = 3 observation)
        if (obstacleWall != null)
        {
            sensor.AddObservation(obstacleWall.localPosition - transform.localPosition);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);
    }


    // TETİKLENMELER VEYA ÇARPIŞMALAR
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint_1") && !ilkKontrolGecildi)
        {
            ilkKontrolGecildi = true;
            AddReward(1.0f); // Doğru yön için ara ödül (Bölüm bitmez)
            Debug.Log("İlk engelin sağından geçildi!");
        }
        else if (other.CompareTag("Checkpoint_2") && ilkKontrolGecildi && !ikinciKontrolGecildi)
        {
            ikinciKontrolGecildi = true;
            AddReward(1.0f); // İkinci doğru yön için ara ödül
            Debug.Log("İkinci engelin solundan geçildi!");
        }
        else if (other.CompareTag("Button"))
        {
            SetReward(5.0f); // Hedefe ulaştı, büyük ödül!
            Debug.Log("Butona basıldı, bölüm başarıyla bitti!");
            // Buraya kapıyı açma kodunuzu ekleyebilirsiniz.
            EndEpisode(); // Bölümü bitir ve başa sar
        }
        else if (other.CompareTag("Portal"))
        {
            SetReward(5.0f); // Bitiş noktası için büyük ödül
            Debug.Log("Ajan mor odadaki bitiş portalına ulaştı!");
            EndEpisode(); // Bölümü bitir
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Engellere veya duvarlara çarparsa ağır ceza ver ve bölümü başa sar
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Wall"))
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        // Kapıya çarparsa bölümü BİTİRME
        else if (collision.gameObject.CompareTag("Gate"))
        {
            // Ajanın kapıya yapışıp sürtünmesini önlemek için çok ufak bir uyarı cezası ver.
            // EndEpisode() OLMADIĞI İÇİN bölüm başa sarmaz, ajan kapıda bekler.
            AddReward(-0.01f);
        }
    }

    // MANUEL TEST İÇİN (Klavyeyle Hareket Ettirme)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}