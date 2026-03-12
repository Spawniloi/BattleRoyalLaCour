using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("PlayerSetup")]
    public GameObject[] playerPrefabs;

    public Transform[] spawnPoints;

    public bool showPlayerScores = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //int players = GameSettings.Instance.playerCount;
        //Debug.Log("Game started with " + players + " players ");
        SpawnPlayers();
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
            }

            //SnakePlayer controller = player.GetComponent<SnakePlayer>();
            //controller.playerID = i + 1;
            //Debug.Log("Spawn player " + (i + 1));
        }
    }
}
