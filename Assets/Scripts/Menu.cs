using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Menu : MonoBehaviour
{
    Board board;
    Score score;
    public GameObject pauseMenu;
    public static bool gamePaused = true;
    private bool boardLoaded = false;

    public GameObject gameOver;
    public TMP_Text endScoreTxt;
    public TMP_Text endHighscoreTxt;
    public TMP_Text endMessage;
    public TMP_Text leftRightText;
    public TMP_Text hardDropText;
    private int endScoreInt;
    private int endHighscoreInt;

    public Toggle toggleFullscreen;
    public Toggle toggleNotificationBar;
    public Toggle toggleLeftCounterclockwise;
    public Toggle toggleGhost;
    public Toggle toggleGrid;
    public GameObject grid;
    public GameObject ghost;

    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.FindObjectOfType<Board>();
        score = GameObject.FindObjectOfType<Score>();
        Time.timeScale = 0f;


        Piece.leftRightSensitivity = PlayerPrefs.GetInt("leftRightSensitivity", 65);
        leftRightText.text = Piece.leftRightSensitivity.ToString();

        Piece.hardDropSensitivity = PlayerPrefs.GetInt("hardDropSensitivity", 150);
        hardDropText.text = Piece.hardDropSensitivity.ToString();

        if (PlayerPrefs.GetInt("enableLeftCounterclockwise", 1) == 1)
        {
            toggleLeftCounterclockwise.isOn = true;
            Piece.leftCounterclockwise = true;
        }

        if (PlayerPrefs.GetInt("enableGrid", 1) == 1)
        {
            toggleGrid.isOn = true;
            grid.SetActive(true);
        }

        if (PlayerPrefs.GetInt("enableGhost", 1) == 1)
        {
            toggleGhost.isOn = true;
            ghost.SetActive(true);
        }

        if (PlayerPrefs.GetInt("fullscreen", 1) == 1)
        {
            toggleFullscreen.isOn = true;
            Screen.fullScreen = true;
        }

        if (PlayerPrefs.GetInt("notificationBar") == 1)
        {
            toggleNotificationBar.isOn = true;
            EnableNotificationBar(); //this must be called last as it breaks Start()
        }


    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            Pause();
            board.Save();
            PlayerPrefs.Save();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        gamePaused = true;

    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        gamePaused = false;
        if (!boardLoaded)
        {
            board.Load();
            boardLoaded = true;
        }
    }

    public void NewGame()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        gameOver.SetActive(false);
        gamePaused = false;
        
        board.Start();

    }

    public void NewGame(string setLevel)
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        gamePaused = false;
        
        board.Start();

        score.SetLevel(int.Parse(setLevel));

    }

    public void SetLeftRightSensitivity(string setRange)
    {
        PlayerPrefs.SetInt("leftRightSensitivity", int.Parse(setRange));
        Piece.leftRightSensitivity = int.Parse(setRange);
    }

    public void SetHardDropSensitivity(string setRange)
    {
        PlayerPrefs.SetInt("hardDropSensitivity", int.Parse(setRange));
        Piece.hardDropSensitivity = int.Parse(setRange);
    }

    public void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        Application.Quit();
    }

    public void Exit()
    {
        board.Save();
        Application.Quit();
    }

    public void ApplySettings()
    {
        if (toggleFullscreen.isOn)
        {
            PlayerPrefs.SetInt("fullscreen", 1);
        }
        else
        {
            PlayerPrefs.SetInt("fullscreen", 0);
        }

        if (toggleNotificationBar.isOn)
        {
            PlayerPrefs.SetInt("notificationBar", 1);
        }
        else
        {
            PlayerPrefs.SetInt("notificationBar", 0);
        }

        if (toggleGhost.isOn)
        {
            PlayerPrefs.SetInt("enableGhost", 1);
        }
        else
        {
            PlayerPrefs.SetInt("enableGhost", 0);
        }

        if (toggleGrid.isOn)
        {
            PlayerPrefs.SetInt("enableGrid", 1);
        }
        else
        {
            PlayerPrefs.SetInt("enableGrid", 0);
        }

        if (toggleLeftCounterclockwise.isOn)
        {
            PlayerPrefs.SetInt("enableLeftCounterclockwise", 1);
        }
        else
        {
            PlayerPrefs.SetInt("enableLeftCounterclockwise", 0);
        }

        Exit();
    }

    public void TransferScore()
    {
        endScoreInt = Score.scoreInt;
        endHighscoreInt = Score.highscoreInt;
        endScoreTxt.text = endScoreInt.ToString();
        endHighscoreTxt.text = endHighscoreInt.ToString();

    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        gamePaused = true;
        gameOver.SetActive(true);
        if (endScoreInt == endHighscoreInt)
        {
            endMessage.text = "New Highscore!";
        }
        else
        {
            endMessage.text = "";
        }
    }

    public void ExitGameOver()
    {
        gameOver.SetActive(false);
        pauseMenu.SetActive(true);
    }

    private void EnableNotificationBar() //update if there is a cleaner way to do this
    {
        using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        using AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow");
        window.Call("setStatusBarColor", unchecked((int)0x00005700)); //for transparent status bar
        using AndroidJavaObject Decor = window.Call<AndroidJavaObject>("getDecorView");
        using AndroidJavaObject controller = Decor.Call<AndroidJavaObject>("getWindowInsetsController");
        using AndroidJavaClass type = new AndroidJavaClass("android.view.WindowInsets$Type");
        controller.Call("show", type.CallStatic<int>("statusBars"));
    }
}