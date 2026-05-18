using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class MoveOnHandTrigger : MonoBehaviour
{
    [SerializeField] private float moveAmount = 2f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private AudioSource wrongDoorBehaviourSound;
    [SerializeField] public GameObject door;
    public GameObject teleportingArea;
    [SerializeField] private AudioSource doorOpenSound;
    [SerializeField] private AudioSource SuccesfulDoorOpenSound;
    public CalculateExamSceneScore calculateExamSceneScore;
    [SerializeField] private bool isExamScene = false;


    private void Start()
    {
        teleportingArea.GetComponent<NavMeshObstacle>().enabled = true;
    }

    private bool isMoving = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isMoving) return;

        if (other.CompareTag("LeftHand") || other.CompareTag("RightHand"))
        {
            if (SceneManager.isGasEnd == true)
            {
                isMoving = true;

                Vector3 targetPos = door.transform.position + new Vector3(0f, moveAmount, 0f);

                StartCoroutine(PlayDoorOpenSound());

                door.transform.DOMove(targetPos, duration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        isMoving = false;
                        teleportingArea.GetComponent<NavMeshObstacle>().enabled = false;
                        StartCoroutine(PlaySuccessSound());
                    });
            }
            else if (SceneManager.isGasStart == true)
            {
                if (isExamScene == false)
                {
                    wrongDoorBehaviourSound.Play();

                }
                calculateExamSceneScore.AddScore(20);
                return;
                
            }
        }
    }

    private IEnumerator PlayDoorOpenSound()
    {
        doorOpenSound.Play();
        yield return new WaitForSeconds(duration); // 4 saniye sonra ses durur
        doorOpenSound.Stop();
    }

    private IEnumerator PlaySuccessSound()
    {
        yield return new WaitForSeconds(2f);
        if (isExamScene == false)
        {
            SuccesfulDoorOpenSound.Play();

        }
    }
}