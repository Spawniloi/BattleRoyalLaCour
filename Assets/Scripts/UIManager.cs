//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class UIManager : MonoBehaviour
//{
//    public static UIManager Instance;

//    [Header("Menus")]
//    public GameObject mainMenu;
//    public GameObject pauseMenu;
//    public GameObject endMenu;

//    public GameObject menuCanvas;


//    private bool isPaused = false;

//    void Start()
//    {
//        ShowMainMenu();
//    }
//    void Awake()
//    {
//        Instance = this;    
//    }

//    //START//

//    public void StartGame(int playerCount)
//    {
//        GameSettings.Instance.playerCount = playerCount;
//        menuCanvas.SetActive(false);
//    }

//    //___Menu Principal___//


//    public void ShowMainMenu()
//    {
//        mainMenu.SetActive(true);
//        pauseMenu.SetActive(false);
//        endMenu.SetActive(false);

//        Time.timeScale = 0f;
//        //SceneManager.LoadScene("GameScene");
//    }
//    public void SetPlayerCount(int count)
//    {
//        GameSettings.Instance.playerCount = count;
//        Debug.Log("Number of players :" + count);   
//        if(mainMenu != null)
//            mainMenu.SetActive(false);
//        PlayGame();

//    }

//    public void PlayGame()
//    {
//        Time.timeScale = 1f;
//        SceneManager.LoadScene("samplescene");
//    }

//    //___Pause___//

//    public void PauseGame()
//    {
//        isPaused = !isPaused;
//        pauseMenu.SetActive(isPaused);
//        Time.timeScale = isPaused? 0f : 1.0f;
//    }

//    //___Fin___//

//    public void ShowEndGame()
//    {
//        endMenu.SetActive(true);
//        Time.timeScale = 0f;
//    }

//    public void BackToMenu()
//    {
//        Time.timeScale = 1f;
//        SceneManager.LoadScene("MenuScene");
//    }


using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Menus (optionnels selon la scène)")]
    public GameObject startMenu;
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject endMenu;

    public GameObject menuCanvas;

    bool isPaused;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {

        // MENU SCENE

        if (startMenu != null)
            startMenu.SetActive(true);

        if (mainMenu != null)
            mainMenu.SetActive(false);

        // GAME SCENE
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (endMenu != null)
            endMenu.SetActive(false);
    }

    // -------- MENU --------

    public void StartGame(int playerCount)
    {
        startMenu.SetActive(true);
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        endMenu.SetActive(false);

        Time.timeScale = 0f;
        GameSettings.Instance.playerCount = playerCount;
        SceneManager.LoadScene("SampleScene");
        menuCanvas.SetActive(false);
    }

    //___Menu Principal___//


    public void ShowMainMenu()
    {
        startMenu.SetActive(false);
        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        endMenu.SetActive(false);

        Time.timeScale = 0f;
    }
    public void SetPlayerCount(int count)
    {
        GameSettings.Instance.playerCount = count;
        Debug.Log("Number of players :" + count);
        if (mainMenu != null)
            mainMenu.SetActive(false);
        PlayGame();

    }
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // -------- PAUSE --------

    public void TogglePause()
    {
        startMenu.SetActive(false);
        mainMenu.SetActive(false);
        pauseMenu.SetActive(true);
        endMenu.SetActive(false);
        if (pauseMenu == null) return;

        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }

    // -------- END --------

    public void ShowEndGame()
    {
        startMenu.SetActive(false);
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        endMenu.SetActive(true);
        if (endMenu == null) return;

        endMenu.SetActive(true);
        Time.timeScale = 0;
    }
}


