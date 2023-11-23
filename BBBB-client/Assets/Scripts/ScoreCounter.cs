using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreDisplay;
    private int score;
    public void SetScore(int newScore)
    {
        score = newScore;
        scoreDisplay.text = score.ToString();
    }
}
