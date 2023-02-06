using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Score : MonoBehaviour
{
    public TMP_Text score;
    public TMP_Text level;
    public TMP_Text highscore;
    static public int scoreInt { get; private set; }
    private int levelInt;
    static public int highscoreInt { get; private set; }
    private int scoreMultiplier = 1;

    // Start is called before the first frame update
    void Start()
    {
        highscoreInt = PlayerPrefs.GetInt("highscore");
        highscore.text = "High " + highscoreInt.ToString();
    }

    public void ResetScore()
    {
        scoreInt = 0;
        levelInt = 1;
        score.text = scoreInt.ToString();
        level.text = "Level " + levelInt.ToString();
    }

    public void AddScore(int addScore)
    {
        scoreInt += addScore * scoreMultiplier;
        score.text = scoreInt.ToString();
        if (scoreInt > highscoreInt)
        {
            PlayerPrefs.SetInt("highscore", scoreInt);
            highscoreInt = scoreInt;
            highscore.text = "High " + highscoreInt.ToString();
        }
    }

    public void SetLevel(int addLevel)
    {
        levelInt = addLevel;
        level.text = "Level " + levelInt.ToString();

        Piece.stepDelay = (float)(1 / ((levelInt - 1) * .5 + 1));
    }

    public void AddLevel(int addLevel)
    {
        levelInt += addLevel;
        level.text = "Level " + levelInt.ToString();
    }
 
    public void SaveScore()
    {
        PlayerPrefs.SetInt("gameScore", scoreInt);
        PlayerPrefs.SetInt("gameLevel", levelInt);
    }

    public void LoadScore()
    {
        ResetScore();
        AddScore(PlayerPrefs.GetInt("gameScore"));
        SetLevel(PlayerPrefs.GetInt("gameLevel", 1));
    }

    public void AddMultiplier(int addMultiplier)
    {
        scoreMultiplier += addMultiplier;
    }

    public void ResetMultiplier()
    {
        scoreMultiplier = 1;
    }
}
