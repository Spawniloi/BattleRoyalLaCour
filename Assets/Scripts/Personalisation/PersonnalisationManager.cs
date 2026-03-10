using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PersonnalisationManager : MonoBehaviour
{
    [Header("Colonnes joueurs")]
    public List<ColonnePerso> colonnes = new List<ColonnePerso>();

    [Header("Bouton retour")]
    public Button btnRetour;

    [Header("Audio")]
    public AudioSource sourceAudio;
    public AudioClip sfxFocus;
    public AudioClip sfxValider;
    public AudioClip sfxRetour;
    public AudioClip sfxBloque;

    void Start()
    {
        btnRetour?.onClick.AddListener(Retour);

        // Init chaque colonne selon nb joueurs actifs
        for (int i = 0; i < colonnes.Count; i++)
        {
            int id = i + 1;
            bool actif = id <= GameData.nombreJoueurs;

            colonnes[i].gameObject.SetActive(actif);

            if (actif)
                colonnes[i].Init(id, this);
        }

        MettreAJourDisponibilites();
    }

    void Update()
    {
        // Chaque joueur contrôle sa propre colonne
        for (int i = 0; i < colonnes.Count; i++)
        {
            int id = i + 1;
            if (id > GameData.nombreJoueurs) continue;

            GererInputJoueur(id, colonnes[i]);
        }

        // Retour — J1 seulement
        var kb = Keyboard.current;
        var gp = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        if (kb != null && kb.escapeKey.wasPressedThisFrame)
            Retour();
        if (gp != null && gp.buttonEast.wasPressedThisFrame)
            Retour();
    }

    void GererInputJoueur(int playerID, ColonnePerso colonne)
    {
        var kb = Keyboard.current;
        var gps = Gamepad.all;
        var gp = playerID - 1 < gps.Count ? gps[playerID - 1] : null;

        bool tetePrev = false, teteSuiv = false;
        bool peauPrev = false, peauSuiv = false;
        bool dossardPrev = false, dossardSuiv = false;

        if (gp != null)
        {
            tetePrev = gp.leftShoulder.wasPressedThisFrame;
            teteSuiv = gp.rightShoulder.wasPressedThisFrame;
            dossardPrev = gp.dpad.left.wasPressedThisFrame;
            dossardSuiv = gp.dpad.right.wasPressedThisFrame;
            peauPrev = gp.dpad.up.wasPressedThisFrame;
            peauSuiv = gp.dpad.down.wasPressedThisFrame;
        }

        if (kb != null)
        {
            if (playerID == 1)
            {
                tetePrev = tetePrev || kb.qKey.wasPressedThisFrame;
                teteSuiv = teteSuiv || kb.eKey.wasPressedThisFrame;
                dossardPrev = dossardPrev || kb.leftArrowKey.wasPressedThisFrame;
                dossardSuiv = dossardSuiv || kb.rightArrowKey.wasPressedThisFrame;
                peauPrev = peauPrev || kb.zKey.wasPressedThisFrame;
                peauSuiv = peauSuiv || kb.sKey.wasPressedThisFrame;
            }
            else if (playerID == 2)
            {
                tetePrev = tetePrev || kb.jKey.wasPressedThisFrame;
                teteSuiv = teteSuiv || kb.lKey.wasPressedThisFrame;
                dossardPrev = dossardPrev || kb.numpad4Key.wasPressedThisFrame;
                dossardSuiv = dossardSuiv || kb.numpad6Key.wasPressedThisFrame;
                peauPrev = peauPrev || kb.iKey.wasPressedThisFrame;
                peauSuiv = peauSuiv || kb.kKey.wasPressedThisFrame;
            }
        }

        if (tetePrev) colonne.TetePrecedente();
        if (teteSuiv) colonne.TeteSuivante();
        if (peauPrev) colonne.PeauPrecedente();
        if (peauSuiv) colonne.PeauSuivante();
        if (dossardPrev) colonne.DossardPrecedent();
        if (dossardSuiv) colonne.DossardSuivant();
    }

    // ── Disponibilités dossard ────────────────────────────────────────────────
    public bool EstDossardDisponible(int indexDossard, int playerID)
    {
        foreach (var col in colonnes)
        {
            if (!col.gameObject.activeSelf) continue;
            if (col.playerID == playerID) continue;

            if (col.GetIndexDossard() == indexDossard)
                return false;
        }
        return true;
    }

    public void MettreAJourDisponibilites()
    {
        foreach (var col in colonnes)
            if (col.gameObject.activeSelf)
                col.MettreAJourSelections();
    }

    // ── Retour ────────────────────────────────────────────────────────────────
    void Retour()
    {
        JouerSFX(sfxRetour);
        SceneManager.LoadScene("Scene_Hub");
    }

    void JouerSFX(AudioClip clip)
    {
        if (sourceAudio != null && clip != null)
            sourceAudio.PlayOneShot(clip);
    }
}