using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SessionData
{
    public string mode = "manche";
    public int nombreManches = 3;
    public int mancheActuelle = 1;
    public List<string> jeuxSelectionnes = new List<string>();
    public bool ordreAleatoire = true;

    public Dictionary<int, float> scores = new Dictionary<int, float>();
    public Dictionary<int, int> sucettesOr = new Dictionary<int, int>();
}

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance;

    public SessionData session = new SessionData();

    private Queue<string> fileJeux = new Queue<string>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Init scores ───────────────────────────────────────────────────────────
    void InitScores()
    {
        session.scores.Clear();
        session.sucettesOr.Clear();

        for (int i = 1; i <= GameData.nombreJoueurs; i++)
        {
            session.scores[i] = 0f;
            session.sucettesOr[i] = 0;
        }

        Debug.Log($"[Session] Scores init pour " +
                  $"{GameData.nombreJoueurs} joueurs");
    }

    // ── Démarre session ───────────────────────────────────────────────────────
    public void DemarrerSession(
        string mode,
        int nombreManches,
        List<string> jeux,
        bool aleatoire)
    {
        session.mode = mode;
        session.nombreManches = nombreManches;
        session.mancheActuelle = 1;
        session.jeuxSelectionnes = new List<string>(jeux);
        session.ordreAleatoire = aleatoire;

        InitScores();
        PreparerFileJeux();

        Debug.Log($"[Session] Démarrage mode:{mode} " +
                  $"manches:{nombreManches} " +
                  $"jeux:{string.Join(",", jeux)}");
    }

    // ── File des jeux ─────────────────────────────────────────────────────────
    void PreparerFileJeux()
    {
        fileJeux.Clear();

        List<string> liste = new List<string>(session.jeuxSelectionnes);

        if (session.ordreAleatoire)
            Melanger(liste);

        foreach (string jeu in liste)
            fileJeux.Enqueue(jeu);
    }

    void Melanger(List<string> liste)
    {
        for (int i = liste.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string tmp = liste[i];
            liste[i] = liste[j];
            liste[j] = tmp;
        }
    }

    public string ProchainJeu()
    {
        if (fileJeux.Count == 0)
            PreparerFileJeux();

        return fileJeux.Count > 0
            ? fileJeux.Dequeue()
            : null;
    }

    public bool MancheTerminee()
        => fileJeux.Count == 0;

    // ── Enregistre une partie ─────────────────────────────────────────────────
    public void EnregistrerPartie(PartieData partie)
    {
        foreach (var jr in partie.joueurs)
        {
            if (!session.scores.ContainsKey(jr.playerID))
                session.scores[jr.playerID] = 0f;

            session.scores[jr.playerID] += jr.score;
        }

        if (partie.gagnant > 0)
        {
            if (!session.sucettesOr.ContainsKey(partie.gagnant))
                session.sucettesOr[partie.gagnant] = 0;

            session.sucettesOr[partie.gagnant]++;
        }

        Debug.Log($"[Session] Partie enregistrée — " +
                  $"gagnant J{partie.gagnant}");
    }

    // ── Manche suivante ───────────────────────────────────────────────────────
    public void MancheSuivante()
    {
        session.mancheActuelle++;
        PreparerFileJeux();

        Debug.Log($"[Session] Manche " +
                  $"{session.mancheActuelle}/{session.nombreManches}");
    }

    public bool SessionTerminee()
        => session.mancheActuelle > session.nombreManches;

    public bool EstEntrainement()
        => session.mode == "entrainement";

    // ── Classement ────────────────────────────────────────────────────────────
    public List<(int playerID, float score, int sucettes)> GetClassementFinal()
    {
        var classement = new List<(int, float, int)>();

        // Scores vides — fallback tous les joueurs actifs à 0
        if (session.scores.Count == 0)
        {
            for (int i = 1; i <= GameData.nombreJoueurs; i++)
                classement.Add((i, 0f, 0));
            return classement;
        }

        foreach (var kvp in session.scores)
        {
            int id = kvp.Key;
            float score = kvp.Value;
            int sucettes = session.sucettesOr.ContainsKey(id)
                ? session.sucettesOr[id] : 0;

            classement.Add((id, score, sucettes));
        }

        // Tri score desc, sucettes desc, playerID asc
        classement.Sort((a, b) => {
            int cmp = b.Item2.CompareTo(a.Item2);
            if (cmp != 0) return cmp;
            cmp = b.Item3.CompareTo(a.Item3);
            if (cmp != 0) return cmp;
            return a.Item1.CompareTo(b.Item1);
        });

        return classement;
    }

    // ── Scène selon jeu ───────────────────────────────────────────────────────
    public string GetNomScene(string jeu)
    {
        return jeu switch
        {
            "maire" => "Scene_MaireCoraille",
            "ballon" => "Scene_BallonPrisonnier",
            "snake" => "Scene_SnakeRacaille",
            _ => "Scene_Hub"
        };
    }

    // ── Lance prochain jeu ────────────────────────────────────────────────────
    public void LancerProchainJeu()
    {
        // Retourne toujours à ChoixJeu pour que le joueur choisisse
        SceneManager.LoadScene("Scene_ChoixJeu");
    }

    // ── Lance un jeu spécifique ───────────────────────────────────────────────
    public void LancerJeu(string jeu)
    {
        string scene = GetNomScene(jeu);
        GameData.jeuActuel = jeu;

        Debug.Log($"[Session] Lancement {jeu} → {scene}");
        SceneManager.LoadScene(scene);
    }
}