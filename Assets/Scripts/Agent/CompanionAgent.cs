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
        if (transform.position.y < -20f)
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }
    [SerializeField] private Transform targetSwitch;  // Şalter / Buton
    [SerializeField] private Transform obstacleWall;  // YER DEĞİŞTİREN ENGEL (YENİ EKLENDİ)
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody rb;
    public Transform spawnPoint;
    [SerializeField] private Transform[] checkpoints;
    private int sonrakiCheckpoint = 0;
    [SerializeField] private bool trainingMode = true;
    [SerializeField] private bool randomizeSwitch = false;
    [SerializeField] private GameObject Door_Agent;    // ajanın kapısı
    [SerializeField] private GameObject Door_Player;   // ana karakterin kapısı
    [SerializeField] private Transform player;              // ana karakter
    [SerializeField] private float followDistance = 8f;     // bu mesafeden uzaksa ajan bekler
    [SerializeField] private bool useFollowMode = true;     // eğitimde false
    private bool butonaBasildi = false;
    private bool gorevTamam = false;
    [SerializeField] private float hideNearDistance = 4f;    // yaklaşınca kaybol
    [SerializeField] private float hideFarDistance = 10f;    // uzaklaşınca kaybol
    [SerializeField] private float minVisibleTime = 1.5f;
    private float gizleSayaci = 0f;
    [Header("Sıralı Başlatma")]
    [SerializeField] private bool bekleyerekBasla = false;   // 2. ve 3. ajanda true yap
    [SerializeField] private CompanionAgent oncekiAjan;      // 1. ajanda boş bırak
    private bool odaTetiklendi = false;
    private bool basladi = true;

    public bool GorevTamam => gorevTamam;          // başka ajan okuyabilsin
    public void OdaTetikle() { odaTetiklendi = true; }
    [SerializeField] private bool yardimliMod = false;



    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0f;
        basladi = trainingMode || !bekleyerekBasla;
    }

    // HER BÖLÜM (EPISODE) BAŞINDA SIFIRLAMA
    public override void OnEpisodeBegin()
    {
        Debug.Log("--- BÖLÜM BAŞLADI (Ajan Doğdu) ---"); // TEŞHİS 1

        gizleSayaci = 0f;
        gorevTamam = false;
        butonaBasildi = false;
        sonrakiCheckpoint = 0;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (obstacleWall != null)
        {
            Vector3 p = obstacleWall.position;
            obstacleWall.position = new Vector3(Random.Range(-2f, 2f), p.y, p.z);
        }

        if (randomizeSwitch)
        {
            Vector3 p = targetSwitch.localPosition;
            targetSwitch.localPosition = new Vector3(Random.Range(-4f, 4f), p.y, Random.Range(4f, 7f));
        }
        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        transform.position = spawnPoint.position + offset;
        butonaBasildi = false;
    }
    private Vector3 currentMoveInput = Vector3.zero;
    private float oncekiMesafe;
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        currentMoveInput = new Vector3(moveX, 0, moveZ);

        // Mesafe ödülü: hedefe yaklaştıkça artı, uzaklaştıkça eksi
        float yeniMesafe = Vector3.Distance(transform.position, GetCurrentTarget().position);
        AddReward((oncekiMesafe - yeniMesafe) * 0.1f);
        oncekiMesafe = yeniMesafe;

        // Küçük zaman cezası
        AddReward(-0.001f);
    }

    private Transform GetCurrentTarget()
    {
        if (checkpoints != null && sonrakiCheckpoint < checkpoints.Length)
            return checkpoints[sonrakiCheckpoint];
        return targetSwitch;   // tüm checkpoint'ler bitince hedef buton
    }

    private void FixedUpdate()
    {
        if (!basladi)
        {
            bool oncekiBitti = (oncekiAjan == null) || oncekiAjan.GorevTamam;
            if (oncekiBitti && odaTetiklendi)
            {
                basladi = true;   // iki şart da sağlandı
            }
            else
            {
                currentMoveInput = Vector3.zero;
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                return;           // beklerken karar da istemez, hareket de etmez
            }
        }
        if (gorevTamam)
        {
            Debug.Log("Mesafe: " + Vector3.Distance(transform.position, player.position));
            rb.linearVelocity = Vector3.zero;
            gizleSayaci += Time.fixedDeltaTime;

            if (gizleSayaci > minVisibleTime && player != null)
            {
                float d = Vector3.Distance(transform.position, player.position);
                if (d < hideNearDistance || d > hideFarDistance)
                    gameObject.SetActive(false);
            }
            return;

        }
        if (useFollowMode && !trainingMode && player != null)
        {
            float d = Vector3.Distance(transform.position, player.position);
            if (d > followDistance)
            {
                currentMoveInput = Vector3.zero;
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                return;   // ana karakter uzak, ajan bekler
            }
        }
        RequestDecision();   // Her fizik adımında karar iste (Decision Requester yerine)

        if (rb.IsSleeping()) rb.WakeUp();
        // --- ACİL YARDIM: oyun modunda checkpoint/şalter yönüne yürüt ---
        if (!trainingMode && yardimliMod)
        {
            Vector3 yon = GetCurrentTarget().position - transform.position;
            yon.y = 0;
            if (yon.magnitude > 0.3f)
                currentMoveInput = yon.normalized;
            else
                currentMoveInput = Vector3.zero;
        }

        Vector3 move = currentMoveInput * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

    }
    // AJANIN VERİ TOPLAMASI (Gözlem)
    public override void CollectObservations(VectorSensor sensor)
    {
        // Ajanın ve hedeflerin pozisyonları
        sensor.AddObservation(transform.position - spawnPoint.position);                         // 3
        sensor.AddObservation(GetCurrentTarget().position - spawnPoint.position);                // 3

        // Engelin ajana göre MESAFESİ / YÖNÜ (Vector3 = 3 observation)
        if (obstacleWall != null)
        {
            sensor.AddObservation(obstacleWall.position - transform.position);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);
        sensor.AddObservation(GetCurrentTarget().position - transform.position);

    }

    private void AjaniGizle()
    {
        gameObject.SetActive(false);
    }

    // TETİKLENMELER VEYA ÇARPIŞMALAR
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            if (checkpoints != null &&
                sonrakiCheckpoint < checkpoints.Length &&
                other.transform == checkpoints[sonrakiCheckpoint])
            {
                sonrakiCheckpoint++;
                AddReward(1.0f);
                oncekiMesafe = Vector3.Distance(transform.position, GetCurrentTarget().position);
            }
        }
        else if (other.CompareTag("Button"))
        {
            if (trainingMode)
            {
                AddReward(5.0f);
                EndEpisode();
            }
            else if (!butonaBasildi)
            {

                butonaBasildi = true;
                gorevTamam = true;
                AddReward(5.0f);
                if (Door_Agent != null) Door_Agent.SetActive(false);
                if (Door_Player != null) Door_Player.SetActive(false);
                Debug.Log("Buton basıldı, kapılar açıldı");
            }
        }
        else if (other.CompareTag("Portal"))
        {
            AddReward(5.0f); // Bitiş noktası için büyük ödül
            Debug.Log("Ajan mor odadaki bitiş portalına ulaştı!");
            EndEpisode(); // Bölümü bitir
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("ÇARPIŞMA: " + collision.gameObject.name + " tag: " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Obstacle") ||
            collision.gameObject.CompareTag("Wall"))

        {
            AddReward(-0.5f);
            if (trainingMode) EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Gate"))
        {
            AddReward(-0.01f);
        }
    }
    // MANUEL TEST İÇİN (Klavyeyle Hareket Ettirme)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var a = actionsOut.ContinuousActions;
        float x = 0f, z = 0f;

        if (Input.GetKey(KeyCode.RightArrow)) x = 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) x = -1f;
        if (Input.GetKey(KeyCode.UpArrow)) z = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) z = -1f;

        a[0] = x;
        a[1] = z;
    }

}
