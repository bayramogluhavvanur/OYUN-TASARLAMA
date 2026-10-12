using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class YoldasZeka : Agent // MonoBehaviour yerine Agent kullanıyoruz
{
    [SerializeField] private Transform hedefButon; // Hedefimiz (Pembe Buton)
    [SerializeField] private float hareketHizi = 5f;

    private Vector3 baslangicPozisyonu;

    // Oyun başladığında yoldaşın ilk konumunu hafızaya al
    public override void Initialize()
    {
        baslangicPozisyonu = transform.localPosition;
    }

    // Her eğitim turu (Episode) baştan başladığında yoldaşı yerine koy
    public override void OnEpisodeBegin()
    {
        transform.localPosition = baslangicPozisyonu;
    }

    // Yapay zekanın (veya senin) yoldaşı hareket ettirme kararları
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Yapay zekadan gelen X ve Z yönündeki hareket verileri
        float hareketX = actions.ContinuousActions[0];
        float hareketZ = actions.ContinuousActions[1];

        Vector3 hareket = new Vector3(hareketX, 0, hareketZ);
        transform.localPosition += hareket * hareketHizi * Time.deltaTime;

        // CEZA DURUMU: Eğer yoldaş dar beyaz yoldan düşerse
        if (transform.localPosition.y < 0)
        {
            SetReward(-1f); // Ceza ver
            EndEpisode();   // Turu başa sar
        }
    }

    // Fiziksel çarpışma (Hedefi bulma)
    private void OnTriggerEnter(Collider other)
    {
        // Eğer çarptığı şey "Buton" etiketine sahipse
        if (other.gameObject.CompareTag("Buton"))
        {
            SetReward(1f); // ÖDÜL VER!
            Debug.Log("Butona basıldı! Yeşil kapı açılacak.");
            EndEpisode();  // Başarılı olduğu için turu bitir
        }
    }

    // Eğitim öncesi senin klavyeyle test edebilmen için manuel kontrol
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal"); // A ve D tuşları
        continuousActions[1] = Input.GetAxisRaw("Vertical");   // W ve S tuşları
    }
}
