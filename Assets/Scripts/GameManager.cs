using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("PlayerSetup")]
    public GameObject[] playerPrefabs;

    public Transform[] spawnPoints;

    public bool showPlayerScores = true;

    public static GameManager Instance;

    public List<SnakePlayer> alivePlayers = new List<SnakePlayer>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //int players = GameSettings.Instance.playerCount;
        //Debug.Log("Game started with " + players + " players ");
        SpawnPlayers();
    }

    void Awake()
    {
        Instance = this;
    }
    void SpawnPlayers()
    {
        int playerCount = GameSettings.Instance.playerCount;
        Debug.Log("Spawn" + playerCount + "players");

        for(int i = 0; i < playerCount; i++)
        {
            //GameObject player = Instantiate(
            //    playerPrefabs[i],
            //    spawnPoints[i].position,
            //    spawnPoints[i].rotation
            //);

            if(spawnPoints[i] == null)
            {
                Debug.LogError("spawnPoints[" + i + "] isn't assigned");
                continue;
            }

            GameObject player = Instantiate(playerPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);

            SnakePlayer sp = player.GetComponent<SnakePlayer>();
            if(sp != null)
            {
                sp.playerID = i + 1;
                sp.showScore = showPlayerScores;

                alivePlayers.Add(sp);
            }



            //SnakePlayer controller = player.GetComponent<SnakePlayer>();
            //controller.playerID = i + 1;
            //Debug.Log("Spawn player " + (i + 1));
        }
    }

    public void PlayerDied(SnakePlayer player)
    {
        Debug.Log("GameManager received death");

        alivePlayers.Remove(player);

        Debug.Log("Players left: " + alivePlayers.Count);
        if (alivePlayers.Count <= 1)
        {
            EndGame(alivePlayers.Count == 1 ? alivePlayers[0] : null);
        }
    }

    void EndGame(SnakePlayer winner)
    {
        Debug.Log("Winner is Player" + winner.playerID);
        UIManager.Instance.ShowEndGame(winner != null ? winner.playerID : 0); //(winner.playerID);
    }

}
