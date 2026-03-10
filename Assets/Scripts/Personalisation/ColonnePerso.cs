using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ColorUtility = UnityEngine.ColorUtility; // ← résout l'ambiguïté

public class ColonnePerso : MonoBehaviour
{
    [Header("Joueur")]
    public int playerID = 1;

    [Header("UI")]
    public TextMeshProUGUI txtNom;
    public PodiumVisuel podiumVisuel;

    [Header("Carousels")]
    public CarouselPerso carouselTete;
    public CarouselPerso carouselPeau;
    public CarouselPerso carouselDossard;

    [Header("Couleurs disponibles peau")]
    public List<Color> couleursPeau = new List<Color>()
    {
        new Color(0.96f, 0.78f, 0.60f),
        new Color(0.90f, 0.65f, 0.45f),
        new Color(0.75f, 0.50f, 0.30f),
        new Color(0.55f, 0.35f, 0.20f),
        new Color(0.95f, 0.85f, 0.75f),
        new Color(0.30f, 0.20f, 0.10f),
    };

    [Header("Couleurs disponibles dossard")]
    public List<Color> couleursDossard = new List<Color>()
    {
        new Color(0.90f, 0.22f, 0.27f),
        new Color(0.27f, 0.48f, 0.62f),
        new Color(0.16f, 0.61f, 0.56f),
        new Color(0.91f, 0.77f, 0.41f),
        new Color(0.58f, 0.24f, 0.80f),
        new Color(0.95f, 0.50f, 0.10f),
    };

    private PersonnalisationManager manager;

    public void Init(int id, PersonnalisationManager mgr)
    {
        playerID = id;
        manager = mgr;

        // Charge prefs
        int savedTete = PlayerPrefs.GetInt($"J{id}_tete", 0);
        int savedPeau = PlayerPrefs.GetInt($"J{id}_peau", 0);
        int savedDossard = PlayerPrefs.GetInt($"J{id}_dossard", id - 1);

        // Init couleurs dans les items du carousel peau
        if (carouselPeau != null)
        {
            for (int i = 0; i < carouselPeau.items.Count; i++)
            {
                Image img = carouselPeau.items[i].GetComponent<Image>();
                if (img != null && i < couleursPeau.Count)
                    img.color = couleursPeau[i];
            }
            carouselPeau.Init(savedPeau);
            carouselPeau.onSelectionChange += OnPeauChange;
        }

        // Init couleurs dans les items du carousel dossard
        if (carouselDossard != null)
        {
            for (int i = 0; i < carouselDossard.items.Count; i++)
            {
                Image img = carouselDossard.items[i].GetComponent<Image>();
                if (img != null && i < couleursDossard.Count)
                    img.color = couleursDossard[i];
            }
            carouselDossard.Init(savedDossard);
            carouselDossard.onSelectionChange += OnDossardChange;
        }

        // Init tetes
        if (carouselTete != null)
        {
            carouselTete.Init(savedTete);
            carouselTete.onSelectionChange += OnTeteChange;
        }

        MettreAJourVisuel();
        MettreAJourNom();
    }

    // ── Callbacks carousels ───────────────────────────────────────────────────
    void OnTeteChange(int index)
    {
        SauvegarderPrefs();
        MettreAJourVisuel();
    }

    void OnPeauChange(int index)
    {
        SauvegarderPrefs();
        MettreAJourVisuel();
    }

    void OnDossardChange(int index)
    {
        // Vérifie dispo
        if (!manager.EstDossardDisponible(index, playerID))
        {
            // Revient au précédent
            carouselDossard.SetIndex(GetIndexDossard());
            return;
        }

        SauvegarderPrefs();
        MettreAJourVisuel();
        MettreAJourNom();
        manager?.MettreAJourDisponibilites();
    }

    // ── Navigation publique ───────────────────────────────────────────────────
    public void TeteSuivante() => carouselTete?.Suivant();
    public void TetePrecedente() => carouselTete?.Precedent();
    public void PeauSuivante() => carouselPeau?.Suivant();
    public void PeauPrecedente() => carouselPeau?.Precedent();

    public void DossardSuivant()
    {
        int nb = carouselDossard.items.Count;
        int depart = GetIndexDossard();
        int current = depart;

        for (int i = 0; i < nb; i++)
        {
            current = (current + 1) % nb;
            if (manager.EstDossardDisponible(current, playerID))
            {
                carouselDossard.SetIndex(current);
                OnDossardChange(current);
                return;
            }
        }
    }

    public void DossardPrecedent()
    {
        int nb = carouselDossard.items.Count;
        int depart = GetIndexDossard();
        int current = depart;

        for (int i = 0; i < nb; i++)
        {
            current = (current - 1 + nb) % nb;
            if (manager.EstDossardDisponible(current, playerID))
            {
                carouselDossard.SetIndex(current);
                OnDossardChange(current);
                return;
            }
        }
    }

    // ── Visuel ────────────────────────────────────────────────────────────────
    void MettreAJourVisuel()
    {
        PlayerData data = GameData.GetJoueur(playerID);
        if (data == null) return;

        data.indexTete = GetIndexTete();

        if (GetIndexPeau() < couleursPeau.Count)
            data.couleurPeau = "#" + ColorUtility.ToHtmlStringRGB(
                couleursPeau[GetIndexPeau()]);

        if (GetIndexDossard() < couleursDossard.Count)
            data.couleurDossard = "#" + ColorUtility.ToHtmlStringRGB(
                couleursDossard[GetIndexDossard()]);

        podiumVisuel?.AppliquerData(data);
    }

    void MettreAJourNom()
    {
        if (txtNom == null) return;
        PlayerData data = GameData.GetJoueur(playerID);
        if (data == null) return;

        string hex = ColorUtility.ToHtmlStringRGB(data.GetCouleurDossard());
        txtNom.text = $"<color=#{hex}>J{playerID}</color>";
    }

    // ── Disponibilités dossard ────────────────────────────────────────────────
    public void MettreAJourSelections()
    {
        if (carouselDossard == null) return;

        for (int i = 0; i < carouselDossard.items.Count; i++)
        {
            bool pris = !manager.EstDossardDisponible(i, playerID);
            carouselDossard.SetGrise(i, pris);
        }
    }

    // ── Getters ───────────────────────────────────────────────────────────────
    public int GetIndexTete()
        => carouselTete != null ? carouselTete.GetIndex() : 0;
    public int GetIndexPeau()
        => carouselPeau != null ? carouselPeau.GetIndex() : 0;
    public int GetIndexDossard()
        => carouselDossard != null ? carouselDossard.GetIndex() : 0;

    // ── PlayerPrefs ───────────────────────────────────────────────────────────
    void SauvegarderPrefs()
    {
        PlayerPrefs.SetInt($"J{playerID}_tete", GetIndexTete());
        PlayerPrefs.SetInt($"J{playerID}_peau", GetIndexPeau());
        PlayerPrefs.SetInt($"J{playerID}_dossard", GetIndexDossard());
        PlayerPrefs.Save();
    }
}
