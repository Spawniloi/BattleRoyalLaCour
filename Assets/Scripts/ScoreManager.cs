using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int numberOfPlayers; //= 2;//
    public int[] scores = new int[4];
    public TextMeshProUGUI[] playerTexts;
    //public TextMeshProUGUI player1Text;
    //public TextMeshProUGUI player2Text;
    //public TextMeshProUGUI player3Text;
    //public TextMeshProUGUI player4Text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numberOfPlayers = GameSettings.Instance.playerCount;
        UpdateScoreUI();
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void Awake()
    {
        foreach(var text in playerTexts)
            text.gameObject.SetActive(false);
        //player1Text.gameObject.SetActive(false);
        //player2Text.gameObject.SetActive(false);
        //player3Text.gameObject.SetActive(false);
        //player4Text.gameObject.SetActive(false);
    }

    public void AddScoreUI(int player, int points)
    {
        scores[player] += points;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {

        for (int i = 0; i < playerTexts.Length; i++)
        {
            bool active = i < numberOfPlayers;
            playerTexts[i].gameObject.SetActive(active);

            if (active)
                playerTexts[i].text = "P" + (i + 1) + ":" + scores[i];
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
