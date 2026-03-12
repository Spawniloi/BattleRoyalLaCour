using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    // Temps
    private float tempsDebut;

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
        tempsDebut = Time.time;
        Debug.Log("Game starting");
    }

    private void StartGame()
    {
        Debug.Log("Game playing");
    }

    private void EndGame()
    {
        if (ballSpawner != null) ballSpawner.enabled = false;
        UpdateRemainingUnits();
        BuildAndSendPartieData();
        StartCoroutine(AllerResultats());
    }

    IEnumerator AllerResultats()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("Scene_Resultats");
    }

    private void BuildAndSendPartieData()
    {
        // Trouve le gagnant
        TeamScore winner = null;
        foreach (TeamScore ts in teamScores.Values)
            if (winner == null || ts.TotalScore > winner.TotalScore)
                winner = ts;

        float duree = Time.time - tempsDebut;

        PartieData partie = new PartieData();
        partie.partieId = System.Guid.NewGuid().ToString();
        partie.jeuActuel = "BallonPrisonnier";
        partie.nbJoueurs = teamScores.Count;
        partie.dureePartie = duree;
        partie.gagnant = winner != null ? winner.teamID + 1 : -1;

        foreach (var kvp in teamScores)
        {
            int teamIdx = kvp.Key;
            TeamScore ts = kvp.Value;

            JoueurResultat jr = new JoueurResultat();
            jr.playerID = teamIdx + 1;
            jr.score = ts.TotalScore;
            jr.stats = new StatsJoueur();
            jr.stats.distanceParcourue = ts.eliminations;
            jr.stats.tempsPoissonMax = ts.ballsPicked;
            jr.stats.nbRebonds = ts.ballsThrown;
            jr.stats.nbPassagesCoraille = ts.unitsRemaining;
            jr.stats.tempsMaire = ts.TotalScore;

            partie.joueurs.Add(jr);
        }

        GameData.dernierePartie = partie;
        GameData.jeuActuel = "BallonPrisonnier";

        GameSessionManager.Instance?.EnregistrerPartie(partie);

        Debug.Log($"[Dodgeball] Gagnant J{partie.gagnant}");
        foreach (var ts in teamScores.Values)
            Debug.Log($"J{ts.teamID + 1} — " +
                      $"Elim:{ts.eliminations} " +
                      $"Ramassées:{ts.ballsPicked} " +
                      $"Lancées:{ts.ballsThrown} " +
                      $"Unités:{ts.unitsRemaining} " +
                      $"Score:{ts.TotalScore}");
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
            GameManager.Instance.JoinPlayer(Keyboard.current);

        if (Gamepad.current != null &&
            Gamepad.current.startButton.wasPressedThisFrame)
            GameManager.Instance.JoinPlayer(Gamepad.current);
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

        if (index < playerSpawns.Length)
            root.position = playerSpawns[index].position;

        Debug.Log("Player " + index + " joined with " + player.devices[0]);

        InitializeScores();

        PlayerManager manager = player.GetComponent<PlayerManager>();
        if (manager != null)
            manager.teamID = index;

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
            if (u.isAlive) aliveTeams.Add(u.teamID);

        if (aliveTeams.Count <= 1)
        {
            foreach (int team in aliveTeams)
                Debug.Log("TEAM " + team + " WINS!");

            SetGameState(GameState.GameOver);
        }
    }
    #endregion

    #region SCORES
    private void InitializeScores()
    {
        teamScores.Clear();
        for (int i = 0; i < inputManager.playerCount; i++)
        {
            TeamScore ts = new TeamScore();
            ts.teamID = i;
            teamScores.Add(i, ts);
        }
    }

    private void UpdateRemainingUnits()
    {
        foreach (var ts in teamScores.Values)
            ts.unitsRemaining = 0;

        Unit[] allUnits = FindObjectsOfType<Unit>();
        foreach (Unit u in allUnits)
            if (u.isAlive && teamScores.ContainsKey(u.teamID))
                teamScores[u.teamID].unitsRemaining++;
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
            score += eliminations * 20;
            score += ballsPicked * 5;
            score += ballsThrown * 7;
            score += unitsRemaining * 10;
            return score;
        }
    }
}