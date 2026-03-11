using System.Collections.Generic;
using Unity.VisualScripting;
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
    public BoutonStylee styleBtnRetour;

    [Header("Audio")]
    public AudioSource sourceAudio;
    public AudioClip sfxFocus;
    public AudioClip sfxValider;
    public AudioClip sfxRetour;
    public AudioClip sfxBloque;

    void Start()
    {
        btnRetour?.onClick.AddListener(Retour);

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
        for (int i = 0; i < colonnes.Count; i++)
        {
            int id = i + 1;
            if (id > GameData.nombreJoueurs) continue;
            GererInputJoueur(id, colonnes[i]);
        }
    }

    // ── Input par joueur ──────────────────────────────────────────────────────
    void GererInputJoueur(int playerID, ColonnePerso colonne)
    {
        var kb = Keyboard.current;
        var manettes = Gamepad.all;
        var gp = playerID - 1 < manettes.Count
            ? manettes[playerID - 1] : null;

        bool gauche = false;
        bool droite = false;
        bool carouselHaut = false;
        bool carouselBas = false;
        bool valider = false;

        // ── Manette prioritaire ───────────────────────────────────────────────
        if (gp != null)
        {
            gauche = gp.leftShoulder.wasPressedThisFrame;
            droite = gp.rightShoulder.wasPressedThisFrame;
            carouselHaut = gp.dpad.up.wasPressedThisFrame
                        || gp.leftStick.up.wasPressedThisFrame;
            carouselBas = gp.dpad.down.wasPressedThisFrame
                        || gp.leftStick.down.wasPressedThisFrame;
            valider = gp.buttonSouth.wasPressedThisFrame;
        }
        // ── Clavier — seulement si pas de manette pour ce joueur ─────────────
        else if (kb != null)
        {
            switch (playerID)
            {
                case 1:
                    gauche = kb.qKey.wasPressedThisFrame;
                    droite = kb.dKey.wasPressedThisFrame;
                    carouselHaut = kb.zKey.wasPressedThisFrame;
                    carouselBas = kb.sKey.wasPressedThisFrame;
                    valider = kb.spaceKey.wasPressedThisFrame
                                || kb.enterKey.wasPressedThisFrame;
                    break;
                case 2:
                    gauche = kb.leftArrowKey.wasPressedThisFrame;
                    droite = kb.rightArrowKey.wasPressedThisFrame;
                    carouselHaut = kb.upArrowKey.wasPressedThisFrame;
                    carouselBas = kb.downArrowKey.wasPressedThisFrame;
                    break;
                case 3:
                    gauche = kb.jKey.wasPressedThisFrame;
                    droite = kb.lKey.wasPressedThisFrame;
                    carouselHaut = kb.iKey.wasPressedThisFrame;
                    carouselBas = kb.kKey.wasPressedThisFrame;
                    break;
                case 4:
                    gauche = kb.numpad4Key.wasPressedThisFrame;
                    droite = kb.numpad6Key.wasPressedThisFrame;
                    carouselHaut = kb.numpad8Key.wasPressedThisFrame;
                    carouselBas = kb.numpad5Key.wasPressedThisFrame;
                    break;
            }
        }

        // ── Applique navigation ───────────────────────────────────────────────
        if (gauche)
        {
            colonne.NaviguerGauche();
            JouerSFX(sfxFocus);
        }
        if (droite)
        {
            colonne.NaviguerDroite();
            JouerSFX(sfxFocus);
        }
        if (carouselHaut)
        {
            colonne.CarouselPrecedent();
            JouerSFX(sfxFocus);
        }
        if (carouselBas)
        {
            colonne.CarouselSuivant();
            JouerSFX(sfxFocus);
        }

        // ── Valider retour — J1 seulement ─────────────────────────────────────
        if (valider && playerID == 1 && colonne.EstSurRetour())
        {
            Retour();
        }
    }

    // ── Focus bouton retour ───────────────────────────────────────────────────
    public void MettreAJourBoutonRetour(bool selectionne)
    {
        styleBtnRetour?.SetSelectionne(selectionne);
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
    public bool EstTeteDisponible(int indexTete, int playerID)
    {
        // ← index 0 maintenant exclusif aussi !
        foreach (var col in colonnes)
        {
            if (!col.gameObject.activeSelf) continue;
            if (col.playerID == playerID) continue;
            if (col.GetIndexTete() == indexTete)
                return false;
        }
        return true;
    }
    public bool EstPeauDisponible(int indexPeau, int playerID)
    {
        foreach (var col in colonnes)
        {
            if (!col.gameObject.activeSelf) continue;
            if (col.playerID == playerID) continue;
            if (col.GetIndexPeau() == indexPeau)
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
        // Sauvegarde toutes les colonnes actives avant de partir
        foreach (var col in colonnes)
            if (col.gameObject.activeSelf)
                col.SauvegarderFinal();

        JouerSFX(sfxRetour);
        SceneManager.LoadScene("Scene_Menu");
    }

    void JouerSFX(AudioClip clip)
    {
        if (sourceAudio != null && clip != null)
            sourceAudio.PlayOneShot(clip);
    }
    

    
}
