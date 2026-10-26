using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class YoldasZeka : Agent
{
    [Header("Ajan Hedefleri")]
    public Transform anaKarakter;
    public Transform pembeButon;


    [SerializeField] private Transform aktifHedef; // Ajanın o an gitmeye çalıştığı nokta
    [SerializeField] private float hareketHizi = 5f;
    [SerializeField] private GameObject yesilKapi; // Arkadaşınız burayı kullanabilir

    public override void Initialize()
    {
        aktifHedef = anaKarakter;
    }
    private void Update()
    {
    }

    public override void OnEpisodeBegin()
    {
        // Ajanı eski yerine ışınlama (Resetleme) kapalı
    }

    // CompanionBridge'den tetiklenen metod
    public void ButonaGitKomutuAl()
    {
        Debug.Log("[YoldasZeka] LLM komutu alındı! Pembe butona doğru yürüyüş başlatılıyor.");
        HedefiDegistir("BUTON"); // Eksik olan asıl hedef değiştirme komutu eklendi
    }

    // DİKKAT: Yapay zekayı bozan Update() metodu tamamen silindi!

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Ajanın kendi pozisyonu (3 değer)
        sensor.AddObservation(transform.localPosition);

        if (aktifHedef != null)
        {
            // 2. Hedefin pozisyonu - İster ana karakter olsun ister buton! (3 değer)
            sensor.AddObservation(aktifHedef.localPosition);

            // 3. Hedefe olan yön vektörü (3 değer)
            sensor.AddObservation(aktifHedef.localPosition - transform.localPosition);

            // 4. Hedefe olan mesafe (1 değer)
            sensor.AddObservation(Vector3.Distance(transform.localPosition, aktifHedef.localPosition));
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }

        // 5. Modelin beklediği 11. boyutu tamamlamak için sabit veri (1 değer)
        sensor.AddObservation(0f);

        // Toplamda tam 11 değer vererek modelin "Fewer observations" hatası vermesini engelliyoruz.
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (aktifHedef != null)
        {
            float aradakiMesafe = Vector3.Distance(transform.position, aktifHedef.position);

            // HEDEFE GÖRE DURMA MESAFESİ:
            // Eğer hedef pembe butonsa 0.3 birim yaklaşsın (üzerine bassın).
            // Eğer hedef ana karakterse 1.5 birim uzakta dursun (içimize girmesin).
            float durmaMesafesi = (aktifHedef == pembeButon) ? 0.3f : 3f;

            // Mesafe, belirlediğimiz durma mesafesinden büyükse hareket et
            if (aradakiMesafe > durmaMesafesi)
            {
                Vector3 hedefYon = (aktifHedef.position - transform.position).normalized;
                hedefYon.y = 0; // Havaya uçmayı engelle

                transform.position += hedefYon * hareketHizi * Time.deltaTime;

                if (hedefYon != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(hedefYon);
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // Eğer çarpan obje "Buton" etiketine (Tag) sahipse ve şu anki hedefimiz pembe butonsa
        if (other.gameObject.CompareTag("Buton") && aktifHedef == pembeButon)
        {
            Debug.Log("Butona basıldı! Otomatik olarak ana karaktere dönülüyor.");

            // Hedefi tekrar ana karakter yap -> Ajan otomatik olarak peşinize dönecektir!
            HedefiDegistir("TAKIP");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        //continuousActions[0] = Input.GetAxisRaw("Horizontal");
        //continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    public void HedefiDegistir(string hedefModu)
    {
        if (hedefModu == "BUTON")
        {
            aktifHedef = pembeButon;
            Debug.Log("Durum: GÖREV - Ajan butona gidiyor!");
        }
        else if (hedefModu == "TAKIP")
        {
            aktifHedef = anaKarakter;
            Debug.Log("Durum: TAKİP - Ajan karakterin yanına dönüyor.");
        }
    }
}