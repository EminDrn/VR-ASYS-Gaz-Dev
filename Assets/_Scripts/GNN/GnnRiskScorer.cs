using UnityEngine;

/// <summary>
/// Sınav sahnesinde gaz bölgesi cezasını GNN risk haritasından verir. Etkinken GasWrongBehaviour
/// kutusu ceza vermez (çift ceza olmasın); kutu yalnızca eğitim etiketi olarak kalır.
///
/// Gaz tehlikesinin bir puan bütçesi vardır (Fine-Kinney şiddet ölçeği: zehirlenme / patlama öldürücü).
/// Toplam gaz cezası:
///
///   ceza = bütçe × [ girişPayı × w_giriş + (1 − girişPayı) × (1 − e^(−E / τ)) ]
///
///   w_giriş : ilk girişteki GNN tehlike olasılığı
///   E       : gaz bölgesinde geçen, GNN tehlike olasılığıyla ağırlıklandırılmış süre (s)
///   τ       : maruziyet zaman sabiti
///
/// Ceza bütçeyi aşamaz; bölgeden çıkıp girmek giriş payını tekrar kesmez, süre birikmeye devam eder.
/// Puanlama yalnızca gaz kaçağı sürerken (vana kapatılana kadar) yapılır.
/// </summary>
public class GnnRiskScorer : MonoBehaviour
{
    private static GnnRiskScorer active;

    /// <summary>
    /// true iken GasWrongBehaviour puan düşmez. Model / graf yüklenemediyse false kalır,
    /// böylece eski kutu cezası devreye girer.
    /// </summary>
    public static bool IsScoring => active != null && active.riskPredictor != null && active.riskPredictor.IsReady;

    [Header("Referanslar (boşsa otomatik bulunur)")]
    [SerializeField] private GnnRiskPredictor riskPredictor;
    [SerializeField] private CalculateExamSceneScore examScore;
    [Tooltip("Konumu puanlanan obje. Boşsa ana kamera (oyuncunun başı)")]
    [SerializeField] private Transform trackedTarget;

    [Header("Gaz Bütçesi (en fazla ceza)")]
    [SerializeField] private float gasBudget = 40f;

    [Header("Ceza Eğrisi")]
    [Tooltip("Bütçenin girişte hemen kesilen payı")]
    [SerializeField, Range(0f, 1f)] private float entryShare = 0.4f;
    [Tooltip("Maruziyet zaman sabiti (s): bu kadar kalınca kalan payın %63'ü kesilmiş olur")]
    [SerializeField] private float exposureTimeConstant = 5f;
    [Tooltip("Dikkat bölgesine giriş cezası (0 = yalnızca uyarı)")]
    [SerializeField] private int cautionPenalty = 0;

    [Header("Zamanlama")]
    [SerializeField] private float updateInterval = 0.5f;
    [Tooltip("Dikkat bölgesinden çıkıp bu süreden (s) önce tekrar girilirse yeniden uyarı / ceza verilmez")]
    [SerializeField] private float reentryCooldown = 3f;

    [Header("Uyarı Sesi (isteğe bağlı)")]
    [SerializeField] private AudioSource warningSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool entered;
    private float entryWeight;   // ilk girişteki tehlike olasılığı
    private float exposure;      // olasılıkla ağırlıklı süre (s)
    private int deducted;        // gaz için şimdiye kadar düşülen puan
    private float nextUpdate;
    private float lastTick = float.NegativeInfinity;
    private GnnRiskClass currentClass = GnnRiskClass.Safe;
    private float cautionLeftAt = float.NegativeInfinity;

    private void Start()
    {
        if (riskPredictor == null) riskPredictor = GnnRiskPredictor.Instance;
        if (riskPredictor == null) riskPredictor = FindObjectOfType<GnnRiskPredictor>();
        if (riskPredictor == null) riskPredictor = gameObject.AddComponent<GnnRiskPredictor>();
        if (examScore == null) examScore = FindObjectOfType<CalculateExamSceneScore>();
        if (trackedTarget == null && Camera.main != null) trackedTarget = Camera.main.transform;
    }

    private void OnEnable() => active = this;

    private void OnDisable()
    {
        if (active == this) active = null;
    }

    private void Update()
    {
        if (Time.time < nextUpdate) return;
        nextUpdate = Time.time + updateInterval;

        float dt = Mathf.Min(Time.time - lastTick, updateInterval * 2f);
        lastTick = Time.time;

        if (!IsGasLeaking() || trackedTarget == null || !riskPredictor.IsReady) return;

        GnnPrediction p = riskPredictor.GetPrediction(trackedTarget.position);
        if (!p.IsValid) return;

        GnnRiskClass previous = currentClass;
        currentClass = p.RiskClass;

        if (p.RiskClass == GnnRiskClass.Danger)
        {
            UpdateGas(p, previous != GnnRiskClass.Danger, dt);
        }
        else if (p.RiskClass == GnnRiskClass.Caution && previous == GnnRiskClass.Safe)
        {
            if (Time.time - cautionLeftAt >= reentryCooldown) EnterCaution(p);
        }

        if (previous == GnnRiskClass.Caution && p.RiskClass == GnnRiskClass.Safe) cautionLeftAt = Time.time;
    }

    private void UpdateGas(GnnPrediction p, bool entering, float dt)
    {
        float w = p.DangerProbability;

        if (!entered)
        {
            entered = true;
            entryWeight = w;
        }
        else if (!entering)
        {
            exposure += w * dt;
        }
        if (entering && warningSound != null) warningSound.Play();

        float target = gasBudget * (entryShare * entryWeight + (1f - entryShare) * (1f - Mathf.Exp(-exposure / exposureTimeConstant)));
        int penalty = Mathf.RoundToInt(target) - deducted;
        if (penalty <= 0) return;

        deducted += penalty;
        Deduct(penalty);
        if (showDebugLogs)
        {
            Debug.Log($"<color=red>[GNN] GAZ BÖLGESİ (p={w:F2}, maruziyet={exposure:F1} s): -{penalty} " +
                      $"(gaz toplam {deducted}/{Mathf.RoundToInt(gasBudget)}) → Puan: {CalculateExamSceneScore.score}</color>");
        }
    }

    private void EnterCaution(GnnPrediction p)
    {
        Deduct(cautionPenalty);
        if (warningSound != null) warningSound.Play();
        if (showDebugLogs)
            Debug.Log($"<color=orange>[GNN] DİKKAT ALANI (risk={p.RiskScore:F2}): -{cautionPenalty} → Puan: {CalculateExamSceneScore.score}</color>");
    }

    private void Deduct(int penalty)
    {
        if (penalty > 0 && examScore != null) examScore.AddScore(penalty); // AddScore puanı düşürür
    }

    /// <summary>Gaz kaçağı sürüyor mu (GasWrongBehaviour ile aynı koşul).</summary>
    private static bool IsGasLeaking() => SceneManager.isGasStart && !SceneManager.isGasEnd;
}
