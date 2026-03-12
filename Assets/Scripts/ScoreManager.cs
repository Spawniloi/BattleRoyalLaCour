using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int numberOfPlayers = 2;
    public int[] scores = new int[4];

    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;
    public TextMeshProUGUI player3Text;
    public TextMeshProUGUI player4Text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateScoreUI();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore(int player, int points)
    {
        scores[player] += points;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        player1Text.text = "P1 : " + scores[0];
        player2Text.text = "P2 : " + scores[1];

        if(numberOfPlayers>=3)
            player3Text.text = "P3 : " + scores[2];
        if (numberOfPlayers == 4)
            player4Text.text = "P4 : " + scores[3];
    }

}
