using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasWrongBehaviour : MonoBehaviour
{
    public AudioSource GaswrongAudioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&& SceneManager.isGasStart == true)
        {
            GaswrongAudioSource.Play();
        }
    }
}
