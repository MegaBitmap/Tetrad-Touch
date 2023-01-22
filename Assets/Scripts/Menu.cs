using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    Board board;
    Score score;
    public GameObject pauseMenu;
    public static bool gamePaused = true;
    private bool boardLoaded = false;


    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.FindObjectOfType<Board>();
        score = GameObject.FindObjectOfType<Score>();
        Time.timeScale = 0f;
        //fixme add code for notification bar
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
        gamePaused = false;
        
        board.GameOver();
        board.Start();

    }

    public void NewGame(string setLevel)
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        gamePaused = false;
        
        board.GameOver();
        board.Start();

        score.SetLevel(int.Parse(setLevel));

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

}
