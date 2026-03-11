using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StatsTracker : MonoBehaviour
{
    public static StatsTracker Instance;

    private Dictionary<int, StatsJoueur> stats
        = new Dictionary<int, StatsJoueur>();
    private DevStats devStats = new DevStats();

    // Suivi interne
    private Dictionary<int, Vector3> dernierePos
        = new Dictionary<int, Vector3>();
    private Dictionary<int, float> debutTempsSansContact
        = new Dictionary<int, float>();
    private Dictionary<int, bool> etaitEnMouvement
        = new Dictionary<int, bool>();
    private Dictionary<int, float> tempsBloque
        = new Dictionary<int, float>();

    private float tempsDebutPartie = 0f;
    private bool premierTransfert = true;
    private float debutRegneActuel = 0f;
    private List<float> dureesRegnes = new List<float>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    // ── Init ──────────────────────────────────────────────────────────────────
    public void Init(List<RacailleController> joueurs)
    {
        tempsDebutPartie = Time.time;
        stats.Clear();
        dernierePos.Clear();
        debutTempsSansContact.Clear();
        etaitEnMouvement.Clear();
        tempsBloque.Clear();

        foreach (var j in joueurs)
        {
            stats[j.playerID] = new StatsJoueur();
            dernierePos[j.playerID] = j.transform.position;
            debutTempsSansContact[j.playerID] = Time.time;
            etaitEnMouvement[j.playerID] = false;
            tempsBloque[j.playerID] = 0f;
        }
    }

    // ── Tick — appelé chaque frame par MaireGameManager ───────────────────────
    public void Tick(List<RacailleController> joueurs)
    {
        foreach (var j in joueurs)
        {
            if (!stats.ContainsKey(j.playerID)) continue;

            // Distance parcourue
            float dist = Vector3.Distance(
                j.transform.position,
                dernierePos[j.playerID]);
            stats[j.playerID].distanceParcourue += dist;
            dernierePos[j.playerID] = j.transform.position;

            // Temps sans contact — que pour les poissons
            if (!j.isMayor)
            {
                float tempsSans = Time.time
                    - debutTempsSansContact[j.playerID];
                if (tempsSans > stats[j.playerID].tempsPoissonMax)
                    stats[j.playerID].tempsPoissonMax = tempsSans;
            }

            // Détection bloqué mur
            Rigidbody2D rb = j.GetComponent<Rigidbody2D>();
            InputHandler ih = j.GetComponent<InputHandler>();
            if (rb != null && ih != null)
            {
                bool bouge = rb.linearVelocity.magnitude > 0.2f;
                bool inputActif = ih.MoveInput.magnitude > 0.2f;

                if (inputActif && !bouge)
                {
                    tempsBloque[j.playerID] += Time.deltaTime;
                    if (tempsBloque[j.playerID] > 2f)
                        devStats.nbFoisBloqueMur++;
                }
                else
                {
                    tempsBloque[j.playerID] = 0f;
                }
            }
        }
    }

    // ── Événements ────────────────────────────────────────────────────────────
    public void OnTransfert(int ancienMaireID, int nouveauMaireID)
    {
        devStats.nbTransfertsTotal++;

        if (premierTransfert)
        {
            devStats.tempsAvantPremierTransfert =
                Time.time - tempsDebutPartie;
            premierTransfert = false;
        }

        if (debutRegneActuel > 0f)
        {
            float dureeRegne = Time.time - debutRegneActuel;
            dureesRegnes.Add(dureeRegne);
            devStats.dureeMoyenneRegne = MoyenneListe(dureesRegnes);
        }

        debutRegneActuel = Time.time;

        // Reset timer sans contact pour l'ancien maire
        if (debutTempsSansContact.ContainsKey(ancienMaireID))
            debutTempsSansContact[ancienMaireID] = Time.time;
    }

    public void OnRebondCoraille(int playerID)
    {
        if (!stats.ContainsKey(playerID)) return;
        stats[playerID].nbRebonds++;
        devStats.nbRebondsCoraille++;

        // Reset timer sans contact car touché
        if (debutTempsSansContact.ContainsKey(playerID))
            debutTempsSansContact[playerID] = Time.time;
    }

    public void OnRebondJoueur(int playerID)
    {
        if (!stats.ContainsKey(playerID)) return;
        devStats.nbRebondsJoueur++;

        // Reset timer sans contact car touché
        if (debutTempsSansContact.ContainsKey(playerID))
            debutTempsSansContact[playerID] = Time.time;
    }

    public void OnRebondMur(int playerID)
    {
        if (!stats.ContainsKey(playerID)) return;
        devStats.nbRebondsMur++;
    }

    public void OnPassageCoraille(int playerID)
    {
        if (!stats.ContainsKey(playerID)) return;
        stats[playerID].nbPassagesCoraille++;
    }

    public void OnDashUtilise()
        => devStats.nbDashUtilises++;

    public void OnDashCollecte()
        => devStats.nbDashCollectes++;

    public void OnDashTransfert()
        => devStats.nbDashMenantTransfert++;

    // ── Calcul résultats ──────────────────────────────────────────────────────
    public PartieData CalculerResultats(
     List<RacailleController> joueurs,
     int nbJoueurs, float dureePartie)
    {
        PartieData partie = new PartieData();
        partie.partieId = System.Guid.NewGuid().ToString();
        partie.jeuActuel = "MaireCoraille";
        partie.nbJoueurs = nbJoueurs;
        partie.dureePartie = dureePartie;
        partie.devStats = devStats;

        List<JoueurResultat> resultats = new List<JoueurResultat>();

        foreach (var j in joueurs)
        {
            // Crée les stats si elles n'existent pas
            if (!stats.ContainsKey(j.playerID))
                stats[j.playerID] = new StatsJoueur();

            JoueurResultat jr = new JoueurResultat();
            jr.playerID = j.playerID;
            jr.stats = stats[j.playerID];
            jr.stats.tempsMaire = j.sliderValue;
            jr.titres = new System.Collections.Generic.List<string>();
            jr.score = 0f;

            resultats.Add(jr);
        }

        // Vérifie qu'on a des joueurs
        if (resultats.Count == 0)
        {
            Debug.LogWarning("[StatsTracker] Aucun joueur !");
            partie.gagnant = 1;
            return partie;
        }

        // Attribution points
        AttribuerPoints(resultats,
            r => r.stats.distanceParcourue, true);
        AttribuerPoints(resultats,
            r => r.stats.tempsPoissonMax, true);
        AttribuerPoints(resultats,
            r => (float)r.stats.nbRebonds, false);
        AttribuerPoints(resultats,
            r => (float)r.stats.nbPassagesCoraille, true);
        AttribuerPoints(resultats,
            r => r.stats.tempsMaire, true);

        foreach (var jr in resultats)
            partie.joueurs.Add(jr);

        // Gagnant
        var classes = resultats
            .OrderByDescending(r => r.score)
            .ThenByDescending(r => r.stats.tempsMaire)
            .ToList();

        partie.gagnant = classes[0].playerID;

        AssignerTitres(resultats);

        return partie;
    }

    void AttribuerPoints(
    List<JoueurResultat> resultats,
    System.Func<JoueurResultat, float> selector,
    bool plusGrandGagne)
    {
        if (resultats == null || resultats.Count == 0) return;

        int[] pointsParRang = { 3, 2, 1, 0 };

        List<JoueurResultat> tries = plusGrandGagne
            ? resultats.OrderByDescending(selector).ToList()
            : resultats.OrderBy(selector).ToList();

        for (int i = 0; i < tries.Count; i++)
        {
            int points = i < pointsParRang.Length
                ? pointsParRang[i] : 0;
            tries[i].score += points;
        }
    }

    void AssignerTitres(List<JoueurResultat> resultats)
    {
        MeilleurTitre(resultats,
            r => r.stats.distanceParcourue, true, "Splash !");
        MeilleurTitre(resultats,
            r => r.stats.tempsPoissonMax, true, "Caché !");
        MeilleurTitre(resultats,
            r => r.stats.nbRebonds, false, "Hop !");
        MeilleurTitre(resultats,
            r => r.stats.nbPassagesCoraille, true, "Ensemble !");
        MeilleurTitre(resultats,
            r => r.stats.tempsMaire, true, "Méchant !");
    }

    void MeilleurTitre(
        List<JoueurResultat> resultats,
        System.Func<JoueurResultat, float> selector,
        bool plusGrandGagne, string titre)
    {
        JoueurResultat gagnant = plusGrandGagne
            ? resultats.OrderByDescending(selector).First()
            : resultats.OrderBy(selector).First();

        gagnant.titres.Add(titre);
    }

    float MoyenneListe(List<float> liste)
    {
        if (liste.Count == 0) return 0f;
        float total = 0f;
        foreach (float v in liste) total += v;
        return total / liste.Count;
    }
}