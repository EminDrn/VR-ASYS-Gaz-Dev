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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (alarmVoiceSource.isPlaying == false)
            {
                calculateExamSceneScore.EvacuatedWithoutAlarm();
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
