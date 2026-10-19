using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Gaz kaçağı sınav puanı (100'den düşülür). Her hatanın şiddetine göre bir puan bütçesi vardır:
///   - Müdahale süresi (gaz başlangıcı → vana kapanana kadar): tepki süresinden sonra doygun artan ceza, en fazla 30
///   - Gaz bulutuna girme: GnnRiskScorer verir, en fazla 40
///   - Gaz varken elektrikli kapı butonuna basma (kıvılcım / ateşleme kaynağı): ilk basış 30, sonrakiler +10, en fazla 50
///   - Alarm vermeden tahliye: 30
/// Sonuçta standart risk etiketi: 75+ Güvenli, 50–74 Orta Riskli, 25–49 Riskli, 25 altı Yüksek Riskli.
/// </summary>
public class CalculateExamSceneScore : MonoBehaviour
{
    public static int score;
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Müdahale Süresi (gaz başlangıcı → vana kapanması)")]
    [Tooltip("Bu süreye (s) kadar ceza yok: vanaya ulaşıp kapatmak için makul tepki süresi")]
    [SerializeField] private float responseGracePeriod = 30f;
    [Tooltip("Tepki süresi aşılınca kesilebilecek en fazla puan")]
    [SerializeField] private float responseBudget = 30f;
    [Tooltip("Gecikme zaman sabiti (s): bu kadar gecikince bütçenin %63'ü kesilir")]
    [SerializeField] private float responseTimeConstant = 30f;

    [Header("Ateşleme Kaynağı (gaz varken elektrikli buton)")]
    [SerializeField] private int ignitionFirstPenalty = 30;
    [SerializeField] private int ignitionRepeatPenalty = 10;
    [SerializeField] private int ignitionMaxPenalty = 50;

    [Header("Alarm")]
    [SerializeField] private int noAlarmPenalty = 30;

    [Header("Risk Etiketi (sınav sonunda puana göre gösterilir)")]
    [SerializeField] private int guvenliEsigi = 75;
    [SerializeField] private int ortaRiskEsigi = 50;
    [SerializeField] private int riskliEsigi = 25;

    private float gasStartTime = -1f;
    private bool responseScored;
    private int ignitionDeducted;

    private void Start()
    {
        score = 100;
    }

    /// <summary>Gaz kaçağı başladı: müdahale süresi ölçülmeye başlar.</summary>
    public void StartScoreCountdown()
    {
        if (gasStartTime < 0f) gasStartTime = Time.time;
    }

    /// <summary>Vana kapandı: müdahale süresi cezası bir kez hesaplanır.</summary>
    public void OnValveClosed()
    {
        if (responseScored || gasStartTime < 0f) return;
        responseScored = true;

        float delay = Mathf.Max(0f, Time.time - gasStartTime - responseGracePeriod);
        int penalty = Mathf.RoundToInt(responseBudget * (1f - Mathf.Exp(-delay / responseTimeConstant)));
        AddScore(penalty);
        Debug.Log($"[Puan] Vana {Time.time - gasStartTime:F0} sn'de kapandı (tepki süresi {responseGracePeriod:F0} sn): -{penalty} → Puan: {score}");
    }

    /// <summary>Tahliye tamamlandı. Vana kapanışı bir şekilde kaçırıldıysa süre cezası burada hesaplanır.</summary>
    public void StopScoreCountdown()
    {
        OnValveClosed();
    }

    /// <summary>Gaz varken elektrikli kapı butonuna basıldı (kıvılcım riski).</summary>
    public void IgnitionSourceUsed()
    {
        int target = ignitionDeducted == 0 ? ignitionFirstPenalty : ignitionDeducted + ignitionRepeatPenalty;
        int penalty = Mathf.Min(target, ignitionMaxPenalty) - ignitionDeducted;
        if (penalty <= 0) return;

        ignitionDeducted += penalty;
        AddScore(penalty);
        Debug.Log($"[Puan] Gaz varken elektrikli butona basıldı: -{penalty} (toplam {ignitionDeducted}/{ignitionMaxPenalty}) → Puan: {score}");
    }

    /// <summary>Alarm verilmeden tahliye edildi.</summary>
    public void EvacuatedWithoutAlarm()
    {
        AddScore(noAlarmPenalty);
        Debug.Log($"[Puan] Alarm verilmeden tahliye: -{noAlarmPenalty} → Puan: {score}");
    }

    /// <summary>Puan düşürür (adı eski kodla uyum için korunuyor).</summary>
    public void AddScore(int amount)
    {
        score -= amount;
        if (score < 0) score = 0;
    }

    public void GetScore()
    {
        scoreText.text = "Puan : " + score + $"\n<size=75%><color={GetRiskColor()}>{GetRiskLabel()}</color></size>";
    }

    /// <summary>Puana göre risk etiketi: yüksek puan = doğru davranış = düşük risk.</summary>
    public string GetRiskLabel()
    {
        if (score >= guvenliEsigi) return "Güvenli";
        if (score >= ortaRiskEsigi) return "Orta Riskli";
        if (score >= riskliEsigi) return "Riskli";
        return "Yüksek Riskli";
    }

    private string GetRiskColor()
    {
        if (score >= guvenliEsigi) return "#3CC864";
        if (score >= ortaRiskEsigi) return "#F0B43C";
        if (score >= riskliEsigi) return "#F07832";
        return "#E04646";
    }
}