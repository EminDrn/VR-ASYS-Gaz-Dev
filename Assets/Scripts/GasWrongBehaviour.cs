using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasWrongBehaviour : MonoBehaviour
{
    public AudioSource GaswrongAudioSource;
    public CalculateExamSceneScore calculateExamSceneScore;
    [SerializeField] private bool isExamScene = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&& SceneManager.isGasStart == true && SceneManager.isGasEnd == false)
        {
            // Puan kesme artık KnnDangerZoneSystem'de ("Gaz" bölgesi, en fazla 30).
            // Burada sadece eğitim sahnesindeki uyarı sesi kaldı.
            if (isExamScene == false)
            {
                GaswrongAudioSource.Play();
            }
        }
    }
}
