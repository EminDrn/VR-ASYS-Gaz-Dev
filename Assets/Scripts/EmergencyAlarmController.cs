using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmergencyAlarmController : MonoBehaviour
{
   public AudioSource alarmVoiceSource;
   

   private void OnTriggerEnter(Collider other)
   {
      
      if (other.CompareTag("LeftHand") || other.CompareTag("LeftHand"))
      {
         if (SceneManager.isGasStart == true || SceneManager.isGasEnd == true)
         {
            alarmVoiceSource.Play();

         }
      }
   }
}
