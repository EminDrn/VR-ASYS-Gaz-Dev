using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishEducation : MonoBehaviour
{
    
    public AudioSource audioSource;
    public AudioSource alarmVoiceSource;
    public CalculateExamSceneScore calculateExamSceneScore;
    [SerializeField] private bool isExamScene = false;

    private bool isFinished = false; // Çıkışa ilk girişte puan kesinleşir, tekrar girişler puanı değiştirmez

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isFinished) return;
            isFinished = true;

            if (alarmVoiceSource.isPlaying == false)
            {
                calculateExamSceneScore.AddScore(25);
            }

            // KNN gaz cezası (en fazla maxPenalty) senaryo sonunda bir kere düşülür
            KnnDangerZoneSystem knn = KnnDangerZoneSystem.Instance;
            if (knn != null)
            {
                knn.enabled = false; // Senaryo bitti, artık ceza işlemesin
                int gazCezasi = Mathf.RoundToInt(knn.TotalPenalty);
                Debug.Log($"[KNN] Gaz cezası: -{gazCezasi} (sınır: {knn.maxPenalty})");
                if (gazCezasi > 0)
                    calculateExamSceneScore.AddScore(gazCezasi);
            }
            alarmVoiceSource.Stop();
            if (!isExamScene)
            {
                audioSource.Play();
            }
         
            
            calculateExamSceneScore.StopScoreCountdown();
            calculateExamSceneScore.GetScore();
        }
    }

  
}
