using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CompanionAgent : Agent
{
    [SerializeField] private Transform targetSwitch;  // Şalter / Buton
    [SerializeField] private Transform obstacleWall;  // YER DEĞİŞTİREN ENGEL (YENİ EKLENDİ)
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    // HER BÖLÜM (EPISODE) BAŞINDA SIFIRLAMA
    public override void OnEpisodeBegin()
    {
        // 1. Ajanın hızını sıfırla ve başlangıç alanına koy
        rb.linearVelocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-2f, 2f), 0.5f, -7f);

        // 2. Şalterin (Hedef) yerini rastgele değiştir (Zemin üst kısmında)
        targetSwitch.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(5f, 8f));

        // 3. Ortadaki Engelin (ObstacleWall) yerini rastgele değiştir
        if (obstacleWall != null)
        {
            obstacleWall.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-1f, 2f));
        }
    }

    // AJANIN VERİ TOPLAMASI (Gözlem)
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetSwitch.localPosition);
        sensor.AddObservation(rb.linearVelocity);
    }

    // AJANIN HAREKETİ VE ZAMAN CEZASI
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        // Zamana bağlı küçük ceza (Ajan boş durmasın, hızlı gitsin)
        AddReward(-0.001f);
    }

    // TETİKLENMELER VEYA ÇARPIŞMALAR
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Switch")) // Şaltere ulaştıysa
        {
            SetReward(1.0f); // Büyük ödül!
            EndEpisode();   // Turu bitir ve yeni tura geç
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")) // Dış duvara veya engelle çarptıysa
        {
            SetReward(-1.0f); // Ceza!
            EndEpisode();    // Turu bitir
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