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

    [Header("Etat")]
    public RacailleController mayorActuel;
    public float tempsRestant;
    public bool partieEnCours = false;

    private List<RacailleController> joueursActifs
        = new List<RacailleController>();

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
            Debug.Log($"[Timer] {tempsRestant}s restantes");
            if (tempsRestant <= 0) TerminerPartie();
        }
    }

    public void TenterTransfert(RacailleController attaquant,
                                RacailleController cible)
    {
        if (attaquant != mayorActuel) return;
        if (cible == mayorActuel) return;
        Debug.Log($"[Transfert] {attaquant.name} -> {cible.name}");
        SetMayor(cible);
    }

    void SetMayor(RacailleController nouveau)
    {
        if (mayorActuel != null)
            mayorActuel.SetMayor(false);

        mayorActuel = nouveau;
        nouveau.SetMayor(true);
        nouveau.ApplyFreeze(config.freezeDuration);
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
}