using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    Board board;
    Score score;
    public GameObject pauseMenu;
    public static bool gamePaused = true;
    private bool boardLoaded = false;

    public Toggle toggleFullscreen;
    public Toggle toggleNotificationBar;
    public Toggle toggleGhost;
    public GameObject ghost;

    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.FindObjectOfType<Board>();
        score = GameObject.FindObjectOfType<Score>();
        Time.timeScale = 0f;

        if (PlayerPrefs.GetInt("enableGhost") == 1)
        {
            toggleGhost.isOn = true;
            ghost.SetActive(true);
        }

        if (PlayerPrefs.GetInt("fullscreen") == 1)
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

        Exit();
    }

    private void EnableNotificationBar()
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (var window = activity.Call<AndroidJavaObject>("getWindow"))
                {
                    window.Call("setStatusBarColor", unchecked((int)0x00005700)); //for transparent status bar
                    using (var Decor = window.Call<AndroidJavaObject>("getDecorView"))
                    {
                        using (var controller = Decor.Call<AndroidJavaObject>("getWindowInsetsController"))
                        {
                            using (var type = new AndroidJavaClass("android.view.WindowInsets$Type"))
                            {
                                controller.Call("show", type.CallStatic<int>("statusBars"));
                            }
                        }
                    }
                }
            }
        }
    }
}
