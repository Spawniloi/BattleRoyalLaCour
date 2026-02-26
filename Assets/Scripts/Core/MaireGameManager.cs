using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MaireGameManager : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;

    [Header("Prefab")]
    public GameObject racaillePrefab; // <- glisse ton prefab ici

    [Header("Positions de spawn")]
    public Transform[] spawnPoints; // <- 4 points dans la scène

    [Header("Corailles")]
    public GameObject coraillePrefab;
    public int nombreCorailles2J = 4;
    public int nombreCorailles3J = 6;
    public int nombreCorailles4J = 8;

    [Header("Etat")]
    public RacailleController mayorActuel;
    public float tempsRestant;
    public bool partieEnCours = false;

    private List<RacailleController> joueursActifs
        = new List<RacailleController>();

    private List<Coraille> corailles = new List<Coraille>();

    void Start()
    {
        tempsRestant = config.roundDuration;
        SpawnerJoueurs();
    }

    void SpawnerJoueurs()
    {
        int nb = GameData.nombreJoueurs;
        Debug.Log($"[MaireGameManager] Spawn de {nb} joueurs");

        for (int i = 0; i < nb; i++)
        {
            Vector3 pos = spawnPoints[i].position;
            GameObject go = Instantiate(racaillePrefab, pos, Quaternion.identity);
            go.name = $"Racaille_J{i + 1}";

            RacailleController rc = go.GetComponent<RacailleController>();
            rc.playerID = i + 1;
            rc.config = config;
            rc.gameManager = this;

            // Assigne le playerID à l'InputHandler aussi
            InputHandler ih = go.GetComponent<InputHandler>();
            if (ih != null) ih.playerID = i + 1;

            joueursActifs.Add(rc);
            Debug.Log($"[Spawn] Racaille_J{i + 1} spawné");
        }

        DemarrerPartie();
    }

    IEnumerator AssignerInputDelai(PlayerInput pi, int index)
    {
        // Attend que le PlayerInput soit bien initialisé
        yield return new WaitForEndOfFrame();

        AssignerInput(pi, index);
    }

    void AssignerInput(PlayerInput pi, int index)
    {
        try
        {
            var manettes = UnityEngine.InputSystem.Gamepad.all;

            if (index < manettes.Count)
            {
                pi.SwitchCurrentControlScheme("Gamepad", manettes[index]);
                Debug.Log($"[Input] J{index + 1} → Manette {index + 1}");
            }
            else if (index == 0)
            {
                pi.SwitchCurrentControlScheme("KeyboardJ1",
                    UnityEngine.InputSystem.Keyboard.current);
                Debug.Log($"[Input] J{index + 1} → Clavier ZQSD");
            }
            else if (index == 1)
            {
                pi.SwitchCurrentControlScheme("KeyboardJ2",
                    UnityEngine.InputSystem.Keyboard.current);
                Debug.Log($"[Input] J{index + 1} → Clavier Fleches");
            }
            else
            {
                Debug.LogWarning($"[Input] J{index + 1} → Pas de manette dispo !");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Input] Erreur J{index + 1} : {e.Message}");
        }
    }


    void DemarrerPartie()
    {
        SpawnerCorailles();
        SetMayor(joueursActifs[0]);
        partieEnCours = true;
        StartCoroutine(SliderUpdateLoop());
        StartCoroutine(TimerLoop());
        Debug.Log("[MaireGameManager] Partie démarrée !");
    }

    IEnumerator SliderUpdateLoop()
    {
        while (partieEnCours)
        {
            yield return new WaitForSeconds(config.mayorTimeTickRate);
            foreach (var j in joueursActifs)
                j.UpdateSlider(j == mayorActuel);
        }
    }

    IEnumerator TimerLoop()
    {
        while (tempsRestant > 0 && partieEnCours)
        {
            yield return new WaitForSeconds(1f);
            tempsRestant -= 1f;
            //Debug.Log($"[Timer] {tempsRestant}s restantes");
            if (tempsRestant <= 0) TerminerPartie();
        }
    }

    public void TenterTransfert(RacailleController attaquant,
                            RacailleController cible)
    {
        if (attaquant != mayorActuel) return;
        if (cible == mayorActuel) return;

        // Direction de séparation entre les 2
        Vector2 dir = (cible.transform.position
                     - attaquant.transform.position).normalized;

        if (dir == Vector2.zero)
            dir = Random.insideUnitCircle.normalized;

        // Knockback ÉGAL sur les 2 AVANT le changement de rôle
        float force = 4f;
        attaquant.GetComponent<Rigidbody2D>()
            .AddForce(-dir * force, ForceMode2D.Impulse);
        cible.GetComponent<Rigidbody2D>()
            .AddForce(dir * force, ForceMode2D.Impulse);

        attaquant.SyncVelocity(-dir * force);
        cible.SyncVelocity(dir * force);

        // Changement de rôle après le knockback
        SetMayor(cible);

        Debug.Log($"[Transfert] J{attaquant.playerID} → J{cible.playerID}");
    }

    void SetMayor(RacailleController nouveau)
    {
        // Retire le rôle à l'ancien maire
        if (mayorActuel != null)
            mayorActuel.SetMayor(false);

        // Donne le rôle au nouveau
        mayorActuel = nouveau;
        nouveau.SetMayor(true);

        Debug.Log($"[Maire] Nouveau maire : {nouveau.name}");
    }

    void TerminerPartie()
    {
        partieEnCours = false;
        var gagnant = joueursActifs.OrderBy(j => j.sliderValue).First();
        Debug.Log($"[FIN] Gagnant : {gagnant.name} (slider = {gagnant.sliderValue:F2})");
        foreach (var j in joueursActifs)
            Debug.Log($"  J{j.playerID} : slider = {j.sliderValue:F2}");
    }

    // Test rapide sans Hub
    [ContextMenu("Test 2 joueurs")]
    void Test2J() { GameData.nombreJoueurs = 2; }
    [ContextMenu("Test 3 joueurs")]
    void Test3J() { GameData.nombreJoueurs = 3; }
    [ContextMenu("Test 4 joueurs")]
    void Test4J() { GameData.nombreJoueurs = 4; }

    void SpawnerCorailles()
    {
        int nb = GameData.nombreJoueurs;
        int nbCorailles = nb == 2 ? nombreCorailles2J :
                          nb == 3 ? nombreCorailles3J : nombreCorailles4J;

        Vector2 terrainSize = config.GetTerrainSize(nb);
        float distanceMin = 3.0f; // distance minimum entre 2 corailles
        int maxEssais = 50;   // évite boucle infinie

        List<Vector2> positionsDejaUtilisees = new List<Vector2>();

        float[] angles = { 0f, 90f, 45f, -45f };

        for (int i = 0; i < nbCorailles; i++)
        {
            Vector2 pos = Vector2.zero;
            bool trovee = false;

            for (int essai = 0; essai < maxEssais; essai++)
            {
                pos = new Vector2(
                    Random.Range(-terrainSize.x / 2f + 2f, terrainSize.x / 2f - 2f),
                    Random.Range(-terrainSize.y / 2f + 2f, terrainSize.y / 2f - 2f)
                );

                // Vérifie que la position est assez loin des autres
                bool tropProche = false;
                foreach (Vector2 posExistante in positionsDejaUtilisees)
                {
                    if (Vector2.Distance(pos, posExistante) < distanceMin)
                    {
                        tropProche = true;
                        break;
                    }
                }

                if (!tropProche)
                {
                    trovee = true;
                    break;
                }
            }

            if (!trovee)
                Debug.LogWarning($"[Coraille] Impossible de trouver position pour coraille {i + 1}");

            positionsDejaUtilisees.Add(pos);

            float angle = angles[Random.Range(0, angles.Length)];
            GameObject go = Instantiate(coraillePrefab,
                                        new Vector3(pos.x, pos.y, 0),
                                        Quaternion.Euler(0, 0, angle));
            go.name = $"Coraille_{i + 1}";

            Coraille c = go.GetComponent<Coraille>();
            c.config = config;
            AssignerBinome(c, i, nb);
            corailles.Add(c);
        }
    }

    void AssignerBinome(Coraille c, int index, int nbJoueurs)
    {
        int[,] combos2J = { { 0, 1 }, { 0, 1 }, { 0, 1 }, { 0, 1 } };
        int[,] combos3J = { { 0, 1 }, { 0, 2 }, { 1, 2 }, { 0, 1 }, { 0, 2 }, { 1, 2 } };
        int[,] combos4J = { { 0, 1 }, { 0, 2 }, { 0, 3 }, { 1, 2 }, { 1, 3 }, { 2, 3 }, { 0, 1 }, { 2, 3 } };

        int[,] combos = nbJoueurs == 2 ? combos2J :
                        nbJoueurs == 3 ? combos3J : combos4J;

        int idxA = combos[index, 0];
        int idxB = combos[index, 1];

        if (idxA < joueursActifs.Count && idxB < joueursActifs.Count)
        {
            c.slotA = joueursActifs[idxA];
            c.slotB = joueursActifs[idxB];

            // ← Initialise les visuels après avoir assigné les slots
            c.InitVisuel();

            Debug.Log($"[Coraille] {c.name} → J{idxA + 1} + J{idxB + 1}");
        }
    }
}