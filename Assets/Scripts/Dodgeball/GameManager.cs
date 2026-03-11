using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private BallSpawner ballSpawner;
    [SerializeField] Transform[] playerSpawns;
    
    public enum GameState
    {
        Lobby,
        StartingGame,
        Playing,
        GameOver
    }
    public GameState currentState;

    //INPUT_JOIN
    PlayerInputManager inputManager;
    List<InputDevice> joinedDevices = new List<InputDevice>();
    public bool allowJoin = true;

    //SCORES
    public Dictionary<int, TeamScore> teamScores = new Dictionary<int, TeamScore>();


    #region STATES
    public void SetGameState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.Lobby:
                EnterLobby();
                break;

            case GameState.StartingGame:
                StartGame();
                break;

            case GameState.Playing:
                BeginGameplay();
                break;

            case GameState.GameOver:
                EndGame();
                break;
        }
    }
    private void EnterLobby()
    {
        allowJoin = true;
        Debug.Log("Lobby started");
    }

    private void BeginGameplay()
    {
        allowJoin = false;
        ballSpawner.enabled = true;

        Debug.Log("Game starting");
    }

    private void StartGame()
    {
        Debug.Log("Game playing");
    }
    private void EndGame()
    {
        ballSpawner.enabled = false;
        Debug.Log("Game over");

        UpdateRemainingUnits();
    }

    #endregion

    #region INIT_LOOP
    void OnEnable()
    {
        Debug.Log("GameManager enabled");
        inputManager.onPlayerJoined += OnPlayerJoined;
    }
    void Awake()
    {
        Instance = this;
        inputManager = FindObjectOfType<PlayerInputManager>();
        ballSpawner = GetComponent<BallSpawner>();
        Debug.Log(inputManager);
    }

    void Start()
    {
        SetGameState(GameState.Lobby);
    }

    void OnDisable()
    {
        inputManager.onPlayerJoined -= OnPlayerJoined;
    }
    private void Update()
    {
        ManualJoin();
    }
    #endregion

    #region PLAYER_JOIN

    private void ManualJoin()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            GameManager.Instance.JoinPlayer(Keyboard.current);
        }

        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            GameManager.Instance.JoinPlayer(Gamepad.current);
        }
    }

    public void JoinPlayer(InputDevice device)
    {
        if (!allowJoin) return;
        if (joinedDevices.Contains(device)) return;

        PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
        joinedDevices.Add(device);

    }

    public void OnPlayerJoined(PlayerInput player)
    {
        int index = player.playerIndex;
        Transform root = player.transform.root;
        // POSITIONNER LE JOUEUR
        if (index < playerSpawns.Length)
        {
            root.position = playerSpawns[index].position;
        }
        Debug.Log("Player " + index + " joined with " + player.devices[0]);

        //SCORES INIT
        InitializeScores();


        // INITIALISER PLAYER MANAGER
        PlayerManager manager = player.GetComponent<PlayerManager>();
        if (manager != null)
        {
            manager.teamID = index;
        }

        if (PlayerInput.all.Count >= 2)
        {
            SetGameState(GameState.StartingGame);
            StartCoroutine(GameStarting());
        }
    }

    IEnumerator GameStarting()
    {
        Debug.Log("GAME LAUNCHING IN 3 SECONDS");
        yield return new WaitForSeconds(3f);

        SetGameState(GameState.Playing);
    }

    public void CheckVictory()
    {
        Unit[] units = FindObjectsOfType<Unit>();

        HashSet<int> aliveTeams = new HashSet<int>();

        foreach (Unit u in units)
        {
            if (u.isAlive)
                aliveTeams.Add(u.teamID);
        }

        if (aliveTeams.Count <= 1)
        {
            foreach (int team in aliveTeams)
            {
                Debug.Log("TEAM " + team + " WINS!");
            }

            SetGameState(GameState.GameOver);
        }
    }
    #endregion

    #region SCORES
    private void InitializeScores()
    {
        teamScores.Clear();

        // Pour chaque équipe présente
        for (int i = 0; i < 4; i++) // ou le nombre d’équipes réelles
        {
            TeamScore ts = new TeamScore();
            ts.teamID = i;
            teamScores.Add(i, ts);
        }
    }

    private void UpdateRemainingUnits()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        foreach (Unit u in allUnits)
        {
            if (u.isAlive)
            {
                if (teamScores.ContainsKey(u.teamID))
                    teamScores[u.teamID].unitsRemaining++;
            }
        }
    }

    #endregion

}


[System.Serializable]
public class TeamScore
{
    public int teamID;
    public int eliminations = 0;
    public int ballsPicked = 0;
    public int ballsThrown = 0;
    public int unitsRemaining = 0;

    public int TotalScore
    {
        get
        {
            int score = 0;
            score += eliminations * 20;   // +20 par élimination
            score += ballsPicked * 5;     // +5 par balle ramassée
            score += ballsThrown * 7;     // +7 par balle envoyée
            score += unitsRemaining * 10; // +10 par unité restante
            return score;
        }
    }
}
