using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public TMP_Text scoreText;
    public float scoreAmount;
    public float pointIncreasedPerSecond;

    //public event Action<float> ScoreInfo;
    

    void Start()
    {
        scoreAmount = 0f;
        pointIncreasedPerSecond = 1f;
    }

    // Update is called once per frame
    public void Update()
    {
        scoreText.text = (int)scoreAmount + " Score";
        scoreAmount += pointIncreasedPerSecond * Time.deltaTime;
        //ScoreInfo(scoreAmount);
    }
}
