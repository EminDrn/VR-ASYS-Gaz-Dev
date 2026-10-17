using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CalculateExamSceneScore : MonoBehaviour
{
    public static int score;
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Süre Cezası (sadece gaz açıkken, vana kapanınca durur)")]
    [Tooltip("Gaz başladıktan sonra bu kadar saniye ceza yok (vanaya yürüyüp çevirme süresi).")]
    [SerializeField] private float toleransSuresi = 20f;
    [Tooltip("Tolerans bittikten sonra kaç saniyede bir ceza kesilir.")]
    [SerializeField] private float cezaAraligi = 5f;
    [Tooltip("Her aralıkta düşülecek puan.")]
    [SerializeField] private int cezaMiktari = 5;
    [Tooltip("Süreden düşülebilecek en fazla puan.")]
    [SerializeField] private int maxSureCezasi = 65;

    [Header("Elektrikli Buton Cezası (gaz açıkken kapı butonuna basma)")]
    [SerializeField] private int butonCezasi = 10;
    [SerializeField] private int maxButonCezasi = 20;

    private Coroutine scoreCoroutine;
    private int sureCezasi = 0;
    private int toplamButonCezasi = 0;

    private void Start()
    {
        score = 100;
        sureCezasi = 0;
        toplamButonCezasi = 0;
    }

    public void StartScoreCountdown()
    {
        if (scoreCoroutine == null)
        {
            scoreCoroutine = StartCoroutine(ScoreCountdown());
        }
    }

    public void StopScoreCountdown()
    {
        if (scoreCoroutine != null)
        {
            StopCoroutine(scoreCoroutine);
            scoreCoroutine = null;
        }
    }

    private IEnumerator ScoreCountdown()
    {
        yield return new WaitForSeconds(toleransSuresi);

        while (sureCezasi < maxSureCezasi)
        {
            yield return new WaitForSeconds(cezaAraligi);
            int ceza = Mathf.Min(cezaMiktari, maxSureCezasi - sureCezasi);
            sureCezasi += ceza;
            AddScore(ceza);
            Debug.Log($"[SÜRE] Gaz hâlâ açık: -{ceza} (toplam süre cezası: {sureCezasi}/{maxSureCezasi})");
        }
        scoreCoroutine = null;
    }

    // Gaz açıkken elektrikli (kapı) butona basma cezası; 6 buton ortak sınırı paylaşır
    public void ElektrikButonuCezasi()
    {
        int ceza = Mathf.Min(butonCezasi, maxButonCezasi - toplamButonCezasi);
        if (ceza <= 0) return;
        toplamButonCezasi += ceza;
        AddScore(ceza);
        Debug.Log($"[BUTON] Gaz açıkken elektrikli butona basıldı: -{ceza} (toplam: {toplamButonCezasi}/{maxButonCezasi})");
    }

    public void AddScore(int amount)
    {
        score -= amount;
        if (score < 0) score = 0;
    }

    public void GetScore()
    {
        scoreText.text = "Puan : " + score + "\nRisk Düzeyi: " + GetRiskLabel(score);
    }

    // Puana göre renkli risk etiketi döner (sel ve yangın senaryolarıyla aynı aralıklar)
    private string GetRiskLabel(int value)
    {
        if (value >= 80) return "<color=#2ECC71>Güvenli</color>";
        if (value >= 60) return "<color=#F1C40F>Orta Riskli</color>";
        if (value >= 40) return "<color=#E67E22>Riskli</color>";
        return "<color=#E74C3C>Çok Riskli</color>";
    }
}