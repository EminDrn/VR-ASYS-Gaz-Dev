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
            
            calculateExamSceneScore.AddScore(35);
            if (isExamScene == false)
            {
                GaswrongAudioSource.Play();
            }
        }
    }
}
