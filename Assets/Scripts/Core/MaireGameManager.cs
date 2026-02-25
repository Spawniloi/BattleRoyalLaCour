using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MaireGameManager : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;

    [Header("Joueurs")]
    public List<RacailleController> joueurs = new List<RacailleController>();

    [Header("Etat")]
    public RacailleController mayorActuel;
    public float tempsRestant;
    public bool partieEnCours = false;

    void Start()
    {
        tempsRestant = config.roundDuration;
        StartCoroutine(AttendreJoueurs());
    }

    // Attend que les joueurs soient tous spawned
    IEnumerator AttendreJoueurs()
    {
        yield return new WaitForSeconds(0.5f);

        // Trouve toutes les racailles dans la scène
        joueurs = FindObjectsByType<RacailleController>(FindObjectsSortMode.None).ToList();

        if (joueurs.Count < 2)
        {
            Debug.LogError("[MaireGameManager] Pas assez de joueurs !");
            yield break;
        }

        DemarrerPartie();
    }

    void DemarrerPartie()
    {
        // Le J1 commence maire
        SetMayor(joueurs[0]);

        partieEnCours = true;

        StartCoroutine(SliderUpdateLoop());
        StartCoroutine(TimerLoop());

        Debug.Log("[MaireGameManager] Partie démarrée !");
    }

    // ── Slider ────────────────────────────────────────────────────────────────
    IEnumerator SliderUpdateLoop()
    {
        while (partieEnCours)
        {
            yield return new WaitForSeconds(config.mayorTimeTickRate);

            foreach (var joueur in joueurs)
            {
                joueur.UpdateSlider(joueur == mayorActuel);
            }
        }
    }

    // ── Timer ────────────────────────────────────────────────────────────────
    IEnumerator TimerLoop()
    {
        while (tempsRestant > 0 && partieEnCours)
        {
            yield return new WaitForSeconds(1f);
            tempsRestant -= 1f;

            Debug.Log($"[Timer] Temps restant : {tempsRestant}s");

            if (tempsRestant <= 0)
                TerminerPartie();
        }
    }

    // ── Transfert de Maire ───────────────────────────────────────────────────
    public void TenterTransfert(RacailleController attaquant, RacailleController cible)
    {
        // Seul le maire peut attaquer
        if (attaquant != mayorActuel) return;

        // La cible doit être un fugitif
        if (cible == mayorActuel) return;

        Debug.Log($"[Transfert] {attaquant.name} -> {cible.name}");

        SetMayor(cible);
    }

    void SetMayor(RacailleController nouveau)
    {
        // Retire le rôle à l'ancien maire
        if (mayorActuel != null)
        {
            mayorActuel.SetMayor(false);
        }

        // Donne le rôle au nouveau
        mayorActuel = nouveau;
        nouveau.SetMayor(true);

        // Freeze sur le nouveau requin
        nouveau.ApplyFreeze(config.freezeDuration);

        Debug.Log($"[Maire] Nouveau maire : {nouveau.name}");
    }

    // ── Fin de partie ────────────────────────────────────────────────────────
    void TerminerPartie()
    {
        partieEnCours = false;

        // Trouve le gagnant = slider le plus bas
        RacailleController gagnant = joueurs
            .OrderBy(j => j.sliderValue)
            .First();

        Debug.Log($"[FIN] Gagnant : {gagnant.name} avec slider = {gagnant.sliderValue:F2}");

        foreach (var j in joueurs)
            Debug.Log($"  {j.name} : slider = {j.sliderValue:F2}");
    }
}