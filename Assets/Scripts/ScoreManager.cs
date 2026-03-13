using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public int numberOfPlayers; //= 2;//
    public int[] scores = new int[4];
    public TextMeshProUGUI[] playerTexts = new TextMeshProUGUI[4];

    public static ScoreManager Instance;

    //public TextMeshProUGUI player1Text;
    //public TextMeshProUGUI player2Text;
    //public TextMeshProUGUI player3Text;
    //public TextMeshProUGUI player4Text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {

        //foreach(var text in playerTexts)
        //    text.gameObject.SetActive(false);

        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("ScoreManager Awake");

        //player1Text.gameObject.SetActive(false);
        //player2Text.gameObject.SetActive(false);
        //player3Text.gameObject.SetActive(false);
        //player4Text.gameObject.SetActive(false);
    }
    void Start()
    {
        numberOfPlayers = GameSettings.Instance.playerCount;
        //SetupUI();
        //UpdateScoreUI();
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("SceneLoaded detected -> reconnect UI");
        //Invoke(nameof(SetupUI), 0.1f);
        SetupUI();  
    }

    void SetupUI()
    {
        Debug.Log("SetupUI connected");
        FindScoreUI();
        UpdateScoreUI();
    }

    void FindScoreUI()
    {

        Debug.Log("Searching score UI");

        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();

        if(canvas != null)
        {
            playerTexts[0] = canvas.transform.Find("P1Score")?.GetComponent<TextMeshProUGUI>();
            playerTexts[1] = canvas.transform.Find("P2Score")?.GetComponent<TextMeshProUGUI>();
            playerTexts[2] = canvas.transform.Find("P3Score")?.GetComponent<TextMeshProUGUI>();
            playerTexts[3] = canvas.transform.Find("P4Score")?.GetComponent<TextMeshProUGUI>();

        }

        //playerTexts = new TextMeshProUGUI[4];

        //for (int i = 0; i < playerTexts.Length; i++)
        //{
        //    GameObject obj = GameObject.Find("P" + (i + 1) + "Score");
        //    if (obj != null)
        //        playerTexts[i] = obj.GetComponent<TextMeshProUGUI>();
        //}
    }

    public void AddScoreUI(int player, int points)
    {
        scores[player] += points;

        Debug.Log("Score added to P" + (player + 1) + " : " + scores[player]);
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        bool needReconnect = false;
        for(int i = 0; i < numberOfPlayers; i++)
        {
            if(playerTexts[i] == null)
            {
                needReconnect = true;
                break;
            }
        }

        if (needReconnect)
        {
            FindScoreUI();
        }


        for (int i = 0; i < numberOfPlayers; i++) //playerTexts.Length
        {

            if (playerTexts[i] != null)
            {
                playerTexts[i].gameObject.SetActive(true);
                playerTexts[i].text = "P" + (i+1) + " : " + scores[i];
            }


            //if (playerTexts[i] != null)
            //    continue;
            //bool active = i < numberOfPlayers;
            //playerTexts[i].gameObject.SetActive(active);

            //if (active)
            //    playerTexts[i].text = "P" + (i + 1) + ":" + scores[i];
        }

        //player1Text.text = "P1 : " + scores[0];
        //player2Text.text = "P2 : " + scores[1];
        //player2Text.text = "P3 : " + scores[2];
        //player2Text.text = "P4 : " + scores[3];

        //if (numberOfPlayers >= 1)
        //    player3Text.text = "P1 : " + scores[0];
        //if (numberOfPlayers >= 2)
        //    player3Text.text = "P2 : " + scores[1];
        //if (numberOfPlayers>=3)
        //    player3Text.text = "P3 : " + scores[2];
        //if (numberOfPlayers == 4)
        //    player4Text.text = "P4 : " + scores[3];
    }

}
