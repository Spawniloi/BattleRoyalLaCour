using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

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

    [Header("Couleurs peau")]
    public List<Color> couleursPeau = new List<Color>()
    {
        new Color(0.96f, 0.78f, 0.60f),
        new Color(0.90f, 0.65f, 0.45f),
        new Color(0.75f, 0.50f, 0.30f),
        new Color(0.55f, 0.35f, 0.20f),
        new Color(0.95f, 0.85f, 0.75f),
        new Color(0.30f, 0.20f, 0.10f),
    };

    [Header("Couleurs dossard")]
    public List<Color> couleursDossard = new List<Color>()
    {
        new Color(0.90f, 0.22f, 0.27f),
        new Color(0.27f, 0.48f, 0.62f),
        new Color(0.16f, 0.61f, 0.56f),
        new Color(0.91f, 0.77f, 0.41f),
        new Color(0.58f, 0.24f, 0.80f),
        new Color(0.95f, 0.50f, 0.10f),
    };

    private int carouselActif = 0;
    private bool surRetour = false;
    private PersonnalisationManager manager;

    public void Init(int id, PersonnalisationManager mgr)
    {
        playerID = id;
        manager = mgr;

        // ── Charge depuis PlayerPrefs en priorité
        //    sinon prend les valeurs de GameData ──────────────────────────────
        PlayerData gameData = GameData.GetJoueur(id);

        int savedTete = PlayerPrefs.HasKey($"J{id}_tete")
            ? PlayerPrefs.GetInt($"J{id}_tete")
            : gameData.indexTete;

        int savedPeau = PlayerPrefs.HasKey($"J{id}_peau")
            ? PlayerPrefs.GetInt($"J{id}_peau")
            : TrouverIndexPeau(gameData.couleurPeau);

        int savedDossard = PlayerPrefs.HasKey($"J{id}_dossard")
            ? PlayerPrefs.GetInt($"J{id}_dossard")
            : TrouverIndexDossard(gameData.couleurDossard);

        // Clamp sécurité
        savedTete = Mathf.Clamp(savedTete, 0,
            carouselTete != null ? carouselTete.items.Count - 1 : 0);
        savedPeau = Mathf.Clamp(savedPeau, 0, couleursPeau.Count - 1);
        savedDossard = Mathf.Clamp(savedDossard, 0, couleursDossard.Count - 1);

        // Assigne couleurs peau dans les items
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

        // Assigne couleurs dossard dans les items
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

        carouselActif = 0;
        surRetour = false;
        MettreAJourCarouselActif();
        MettreAJourVisuel();
        MettreAJourNom();
    }

    // ── Trouve index peau depuis couleur hex GameData ─────────────────────────
    int TrouverIndexPeau(string hexCouleur)
    {
        if (string.IsNullOrEmpty(hexCouleur)) return 0;

        Color cible;
        if (!ColorUtility.TryParseHtmlString(hexCouleur, out cible))
            return 0;

        float minDist = float.MaxValue;
        int bestIdx = 0;

        for (int i = 0; i < couleursPeau.Count; i++)
        {
            float dist = ColorDistance(couleursPeau[i], cible);
            if (dist < minDist)
            {
                minDist = dist;
                bestIdx = i;
            }
        }
        return bestIdx;
    }

    // ── Trouve index dossard depuis couleur hex GameData ──────────────────────
    int TrouverIndexDossard(string hexCouleur)
    {
        if (string.IsNullOrEmpty(hexCouleur)) return playerID - 1;

        Color cible;
        if (!ColorUtility.TryParseHtmlString(hexCouleur, out cible))
            return playerID - 1;

        float minDist = float.MaxValue;
        int bestIdx = 0;

        for (int i = 0; i < couleursDossard.Count; i++)
        {
            float dist = ColorDistance(couleursDossard[i], cible);
            if (dist < minDist)
            {
                minDist = dist;
                bestIdx = i;
            }
        }
        return bestIdx;
    }

    float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r)
             + Mathf.Abs(a.g - b.g)
             + Mathf.Abs(a.b - b.b);
    }

    // ── Sauvegarde finale vers GameData + PlayerPrefs ─────────────────────────
    public void SauvegarderFinal()
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

        PlayerPrefs.SetInt($"J{playerID}_tete", GetIndexTete());
        PlayerPrefs.SetInt($"J{playerID}_peau", GetIndexPeau());
        PlayerPrefs.SetInt($"J{playerID}_dossard", GetIndexDossard());
        PlayerPrefs.Save();

        Debug.Log($"[Perso] J{playerID} sauvegardé — " +
                  $"tete:{data.indexTete} " +
                  $"peau:{data.couleurPeau} " +
                  $"dossard:{data.couleurDossard}");
    }

    // ── Carousel actif ↑↓ ────────────────────────────────────────────────────
    public void CarouselSuivant()
    {
        int max = playerID == 1 ? 3 : 2;
        carouselActif = Mathf.Min(carouselActif + 1, max);
        surRetour = playerID == 1 && carouselActif == 3;

        MettreAJourCarouselActif();
        manager?.MettreAJourBoutonRetour(surRetour && playerID == 1);
    }

    public void CarouselPrecedent()
    {
        carouselActif = Mathf.Max(carouselActif - 1, 0);
        surRetour = false;

        MettreAJourCarouselActif();
        manager?.MettreAJourBoutonRetour(false);
    }

    void QuitterRetour()
    {
        if (!surRetour) return;
        surRetour = false;
        carouselActif = 2;
        MettreAJourCarouselActif();
        manager?.MettreAJourBoutonRetour(false);
    }

    void MettreAJourCarouselActif()
    {
        SetCarouselActifVisuel(carouselTete, carouselActif == 0);
        SetCarouselActifVisuel(carouselPeau, carouselActif == 1);
        SetCarouselActifVisuel(carouselDossard, carouselActif == 2);
    }

    void SetCarouselActifVisuel(CarouselPerso carousel, bool actif)
    {
        if (carousel == null) return;

        Image fond = carousel.GetComponent<Image>();
        if (fond != null)
            fond.color = actif
                ? new Color(1f, 1f, 1f, 0.15f)
                : new Color(1f, 1f, 1f, 0.05f);

        carousel.transform.localScale = actif
            ? Vector3.one * 1.05f
            : Vector3.one;
    }

    // ── Navigation ←/→ ───────────────────────────────────────────────────────
    public void NaviguerGauche()
    {
        if (surRetour) { QuitterRetour(); return; }
        switch (carouselActif)
        {
            case 0: TetePrecedente(); break;
            case 1: PeauPrecedente(); break;
            case 2: DossardPrecedent(); break;
        }
    }

    public void NaviguerDroite()
    {
        if (surRetour) { QuitterRetour(); return; }
        switch (carouselActif)
        {
            case 0: TeteSuivante(); break;
            case 1: PeauSuivante(); break;
            case 2: DossardSuivant(); break;
        }
    }

    void PeauSuivante()
    {
        int nb = carouselPeau.items.Count;
        int current = GetIndexPeau();

        for (int i = 0; i < nb; i++)
        {
            current = (current + 1) % nb;
            if (manager.EstPeauDisponible(current, playerID))
            {
                carouselPeau.SetIndex(current);
                OnPeauChange(current);
                return;
            }
        }
    }

    void PeauPrecedente()
    {
        int nb = carouselPeau.items.Count;
        int current = GetIndexPeau();

        for (int i = 0; i < nb; i++)
        {
            current = (current - 1 + nb) % nb;
            if (manager.EstPeauDisponible(current, playerID))
            {
                carouselPeau.SetIndex(current);
                OnPeauChange(current);
                return;
            }
        }
    }

    public bool EstSurRetour() => surRetour;

    // ── Tête avec vérif dispo ─────────────────────────────────────────────────
    void TeteSuivante()
    {
        int nb = carouselTete.items.Count;
        int current = GetIndexTete();

        for (int i = 0; i < nb; i++)
        {
            current = (current + 1) % nb;
            if (manager.EstTeteDisponible(current, playerID))
            {
                carouselTete.SetIndex(current);
                OnTeteChange(current);
                return;
            }
        }
    }

    void TetePrecedente()
    {
        int nb = carouselTete.items.Count;
        int current = GetIndexTete();

        for (int i = 0; i < nb; i++)
        {
            current = (current - 1 + nb) % nb;
            if (manager.EstTeteDisponible(current, playerID))
            {
                carouselTete.SetIndex(current);
                OnTeteChange(current);
                return;
            }
        }
    }

    // ── Dossard avec vérif dispo ──────────────────────────────────────────────
    void DossardSuivant()
    {
        int nb = carouselDossard.items.Count;
        int current = GetIndexDossard();

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

    void DossardPrecedent()
    {
        int nb = carouselDossard.items.Count;
        int current = GetIndexDossard();

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

    // ── Callbacks ─────────────────────────────────────────────────────────────
    void OnTeteChange(int index)
    {
        MettreAJourVisuel();
        manager?.MettreAJourDisponibilites();
    }

    void OnPeauChange(int index)
    {
        MettreAJourVisuel();
        manager?.MettreAJourDisponibilites(); // ← ajoute ça
    }

    void OnDossardChange(int index)
    {
        if (!manager.EstDossardDisponible(index, playerID))
        {
            carouselDossard.SetIndex(GetIndexDossard());
            return;
        }

        MettreAJourVisuel();
        MettreAJourNom();
        manager?.MettreAJourDisponibilites();
    }

    // ── Visuel podium (temps réel) ────────────────────────────────────────────
    void MettreAJourVisuel()
    {
        PlayerData data = GameData.GetJoueur(playerID);
        if (data == null) return;

        // Mise à jour temps réel pour voir dans le podiumVisuel
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

        string hex = ColorUtility.ToHtmlStringRGB(
            data.GetCouleurDossard());
        txtNom.text = $"<color=#{hex}>J{playerID}</color>";
    }

    // ── Disponibilités ────────────────────────────────────────────────────────
    public void MettreAJourSelections()
    {
        // Tête
        if (carouselTete != null)
            for (int i = 0; i < carouselTete.items.Count; i++)
                carouselTete.SetGrise(i,
                    !manager.EstTeteDisponible(i, playerID));

        // Peau
        if (carouselPeau != null)
            for (int i = 0; i < carouselPeau.items.Count; i++)
                carouselPeau.SetGrise(i,
                    !manager.EstPeauDisponible(i, playerID));

        // Dossard
        if (carouselDossard != null)
            for (int i = 0; i < carouselDossard.items.Count; i++)
                carouselDossard.SetGrise(i,
                    !manager.EstDossardDisponible(i, playerID));
    }

    // ── Getters ───────────────────────────────────────────────────────────────
    public int GetIndexTete()
        => carouselTete != null ? carouselTete.GetIndex() : 0;
    public int GetIndexPeau()
        => carouselPeau != null ? carouselPeau.GetIndex() : 0;
    public int GetIndexDossard()
        => carouselDossard != null ? carouselDossard.GetIndex() : 0;
}