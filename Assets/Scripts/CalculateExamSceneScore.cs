using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CalculateExamSceneScore : MonoBehaviour
{
    public static int score;
    [SerializeField] TextMeshProUGUI scoreText;

    private Coroutine scoreCoroutine;

    private void Start()
    {
        score = 100;
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
        while (true)
        {
            yield return new WaitForSeconds(10f);
            AddScore(10);
        }
    }

    public void AddScore(int amount)
    {
        score -= amount;
        if (score < 0) score = 0;
    }

    public void GetScore()
    {
        scoreText.text = "Puan : " + score;
    }
}