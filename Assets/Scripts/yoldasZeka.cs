using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class YoldasZeka : Agent
{
    [Header("Hedef ve Hareket Ayarları")]
    [SerializeField] private Transform hedefButon; // Pembe Buton
    [SerializeField] private float hareketHizi = 5f;
    [SerializeField] private GameObject yesilKapi;

    private Vector3 baslangicPozisyonu;
    private bool otomatikyuru = false; // LLM'den komut gelince aktifleşecek bayrak

    public override void Initialize()
    {
        baslangicPozisyonu = transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = baslangicPozisyonu;
    }

    // CompanionBridge'den tetiklenen metod
    public void ButonaGitKomutuAl()
    {
        Debug.Log("[YoldasZeka] LLM komutu alındı! Pembe butona doğru yürüyüş başlatılıyor.");
        otomatikyuru = true;
    }

    private void Update()
    {
        // LLM komutu geldiyse pembe butona doğru otonom yürü
        if (otomatikyuru && hedefButon != null)
        {
            Vector3 hedefYon = (hedefButon.localPosition - transform.localPosition).normalized;
            hedefYon.y = 0; // Dikey eksende kaymayı önle
            transform.localPosition += hedefYon * hareketHizi * Time.deltaTime;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        if (hedefButon != null)
        {
            sensor.AddObservation(hedefButon.localPosition);
            sensor.AddObservation(hedefButon.localPosition - transform.localPosition);
            sensor.AddObservation(Vector3.Distance(transform.localPosition, hedefButon.localPosition));
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
        sensor.AddObservation(transform.localPosition.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Klavye (Heuristic) hareketleri
        float hareketX = actions.ContinuousActions[0];
        float hareketZ = actions.ContinuousActions[1];

        Vector3 hareket = new Vector3(hareketX, 0, hareketZ);
        transform.localPosition += hareket * hareketHizi * Time.deltaTime;

        if (transform.localPosition.y < 0)
        {
            SetReward(-1f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Buton"))
        {
            SetReward(1f); // ÖDÜL VER!
            Debug.Log("Butona basıldı! Yeşil kapı açılacak.");
            EndEpisode();  // Başarılı olduğu için turu bitir
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
}