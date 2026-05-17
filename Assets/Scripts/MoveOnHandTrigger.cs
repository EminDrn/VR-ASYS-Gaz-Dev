using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveOnHandTrigger : MonoBehaviour
{
    [SerializeField] private float moveAmount = 2f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private AudioSource wrongDoorBehaviourSound;
    [SerializeField] public GameObject door;

    private bool isMoving = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (isMoving) return; 
        if (other.CompareTag("LeftHand") || other.CompareTag("RightHand"))
         {
             isMoving = true;
             Debug.Log("ismoving");
             Vector3 targetPos = door.transform.position + new Vector3(0f, moveAmount, 0f);

             door.transform.DOMove(targetPos, duration)
                 .SetEase(Ease.OutQuad)
                 .OnComplete(() => isMoving = false);
         }

      /* if (other.CompareTag("LeftHand") || other.CompareTag("RightHand"))
       {
           if (SceneManager.isGasEnd == true)
           {
               isMoving = true;

               Vector3 targetPos = door.transform.position + new Vector3(0f, moveAmount, 0f);

               door.transform.DOMove(targetPos, duration)
                   .SetEase(Ease.OutQuad)
                   .OnComplete(() => isMoving = false);
           }
           else if (SceneManager.isGasStart == true)
           {
               wrongDoorBehaviourSound.Play();
               return;
           }

       }**/
       
    }
}