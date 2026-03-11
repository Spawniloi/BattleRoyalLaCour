using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance;

    [Header("Panel")]
    public GameObject panelOptions;

    [Header("Sliders son")]
    public Slider sliderMusique;
    public Slider sliderVFX;

    [Header("Carousel langue")]
    public List<Button> btnsLangue = new List<Button>();
    public List<BoutonStylee> stylesLangue = new List<BoutonStylee>();
    public List<string> codesLangue = new List<string>()
        { "fr", "en", "cr" };

    [Header("Boutons")]
    public Button btnRetour;
    public Button btnQuitterJeu;
    public BoutonStylee styleBtnRetour;
    public BoutonStylee styleBtnQuitterJeu;

    [Header("Texte quitter (caché hors jeu)")]
    public GameObject zoneQuitterJeu;

    [Header("Audio")]
    public AudioSource sourceAudio;
    public AudioClip sfxFocus;
    public AudioClip sfxValider;
    public AudioClip sfxRetour;

    // Navigation interne
    // 0=sliderMusique 1=sliderVFX 2=langue 3=retour 4=quitterJeu
    private int navIndex = 3;
    private bool estOuvert = false;
    private bool etaitEnJeu = false;

    // Scènes considérées "en jeu" — pause activée
    private static readonly HashSet<string> scenesJeu = new HashSet<string>
    {
        "Scene_MaireCoraille",
        "Scene_BallonPrisonnier",
        "Scene_SnakeRacaille"
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        panelOptions?.SetActive(false);

        // Sliders
        if (sliderMusique != null)
        {
            sliderMusique.minValue = 0f;
            sliderMusique.maxValue = 1f;
            sliderMusique.wholeNumbers = false;
            sliderMusique.interactable = false;
            sliderMusique.value =
                AudioManager.Instance?.GetVolMusique() ?? 0.8f;
            sliderMusique.onValueChanged.AddListener(
                v => AudioManager.Instance?.AppliquerMusique(v));
        }

        if (sliderVFX != null)
        {
            sliderVFX.minValue = 0f;
            sliderVFX.maxValue = 1f;
            sliderVFX.wholeNumbers = false;
            sliderVFX.interactable = false;
            sliderVFX.value =
                AudioManager.Instance?.GetVolVFX() ?? 0.8f;
            sliderVFX.onValueChanged.AddListener(
                v => AudioManager.Instance?.AppliquerVFX(v));
        }

        // Boutons langue
        for (int i = 0; i < btnsLangue.Count; i++)
        {
            int idx = i;
            btnsLangue[i]?.onClick.AddListener(
                () => SelectionnerLangue(idx));
        }

        btnRetour?.onClick.AddListener(Fermer);
        btnQuitterJeu?.onClick.AddListener(QuitterJeu);

        // Surligne langue actuelle
        MettreAJourLangueVisuel();
    }

    void Update()
    {
        // ── Détection ouverture — tous joueurs ────────────────────────────────
        if (!estOuvert)
        {
            bool ouvrir = false;

            // Clavier
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
                ouvrir = true;

            // N'importe quelle manette — bouton Start
            foreach (var gp in Gamepad.all)
                if (gp.startButton.wasPressedThisFrame)
                    ouvrir = true;

            if (ouvrir) Ouvrir();
            return;
        }

        // ── Navigation dans le panel ──────────────────────────────────────────
        var kb = Keyboard.current;
        var gp0 = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        bool haut = false, bas = false;
        bool gauche = false, droite = false;
        bool valider = false, fermer = false;

        if (kb != null)
        {
            haut = kb.upArrowKey.wasPressedThisFrame
                   || kb.zKey.wasPressedThisFrame;
            bas = kb.downArrowKey.wasPressedThisFrame
                   || kb.sKey.wasPressedThisFrame;
            gauche = kb.leftArrowKey.wasPressedThisFrame
                   || kb.qKey.wasPressedThisFrame;
            droite = kb.rightArrowKey.wasPressedThisFrame
                   || kb.dKey.wasPressedThisFrame;
            valider = kb.spaceKey.wasPressedThisFrame
                   || kb.enterKey.wasPressedThisFrame;
            fermer = kb.escapeKey.wasPressedThisFrame;
        }

        // Une seule manette contrôle le panel (J1 ou celle qui a ouvert)
        if (gp0 != null)
        {
            haut = haut || gp0.dpad.up.wasPressedThisFrame
                              || gp0.leftStick.up.wasPressedThisFrame;
            bas = bas || gp0.dpad.down.wasPressedThisFrame
                              || gp0.leftStick.down.wasPressedThisFrame;
            gauche = gauche || gp0.dpad.left.wasPressedThisFrame
                              || gp0.leftStick.left.wasPressedThisFrame;
            droite = droite || gp0.dpad.right.wasPressedThisFrame
                              || gp0.leftStick.right.wasPressedThisFrame;
            valider = valider || gp0.buttonSouth.wasPressedThisFrame;
            fermer = fermer || gp0.buttonEast.wasPressedThisFrame
                              || gp0.startButton.wasPressedThisFrame;
        }

        if (haut) Naviguer(-1);
        if (bas) Naviguer(1);
        if (gauche) NaviguerSliderOuLangue(-1);
        if (droite) NaviguerSliderOuLangue(1);
        if (valider) ValiderNav();
        if (fermer) Fermer();
    }

    // ── Ouvrir ────────────────────────────────────────────────────────────────
    public void Ouvrir()
    {
        estOuvert = true;
        string scene = SceneManager.GetActiveScene().name;
        etaitEnJeu = scenesJeu.Contains(scene);

        // Pause si en jeu
        if (etaitEnJeu)
            Time.timeScale = 0f;

        // Montre bouton quitter seulement en jeu
        zoneQuitterJeu?.SetActive(etaitEnJeu);

        panelOptions?.SetActive(true);

        // Refresh sliders
        if (sliderMusique != null && AudioManager.Instance != null)
            sliderMusique.value = AudioManager.Instance.GetVolMusique();
        if (sliderVFX != null && AudioManager.Instance != null)
            sliderVFX.value = AudioManager.Instance.GetVolVFX();

        MettreAJourLangueVisuel();

        navIndex = 3; // Retour par défaut
        SurlígnerNav();
        JouerSFX(sfxValider);
    }

    // ── Fermer ────────────────────────────────────────────────────────────────
    public void Fermer()
    {
        estOuvert = false;

        // Reprend le jeu
        if (etaitEnJeu)
            Time.timeScale = 1f;

        AudioManager.Instance?.Sauvegarder();
        panelOptions?.SetActive(false);
        JouerSFX(sfxRetour);
    }

    // ── Quitter le jeu ────────────────────────────────────────────────────────
    void QuitterJeu()
    {
        Time.timeScale = 1f;
        estOuvert = false;
        AudioManager.Instance?.Sauvegarder();
        panelOptions?.SetActive(false);
        SceneManager.LoadScene("Scene_Hub");
        JouerSFX(sfxValider);
    }

    // ── Navigation ↑↓ ────────────────────────────────────────────────────────
    void Naviguer(int dir)
    {
        int max = etaitEnJeu ? 4 : 3; // quitterJeu seulement en jeu
        navIndex = (navIndex + dir + max + 1) % (max + 1);
        SurlígnerNav();
        JouerSFX(sfxFocus);
    }

    void ValiderNav()
    {
        switch (navIndex)
        {
            case 3: Fermer(); break;
            case 4: QuitterJeu(); break;
        }
    }

    // ── Navigation ←/→ sur slider ou langue ──────────────────────────────────
    void NaviguerSliderOuLangue(int dir)
    {
        switch (navIndex)
        {
            case 0: // Slider musique
                if (sliderMusique != null)
                {
                    sliderMusique.value = Mathf.Clamp01(
                        sliderMusique.value + dir * 0.05f);
                    JouerSFX(sfxFocus);
                }
                break;

            case 1: // Slider VFX
                if (sliderVFX != null)
                {
                    sliderVFX.value = Mathf.Clamp01(
                        sliderVFX.value + dir * 0.05f);
                    JouerSFX(sfxFocus);
                }
                break;

            case 2: // Langue
                int idxLang = codesLangue.IndexOf(
                    LocalisationManager.Instance?.GetLangueActuelle()
                    ?? "fr");
                int newIdx = (idxLang + dir + codesLangue.Count)
                           % codesLangue.Count;
                SelectionnerLangue(newIdx);
                break;
        }
    }

    void SurlígnerNav()
    {
        // Sliders — scale si actif
        if (sliderMusique != null)
            sliderMusique.transform.localScale =
                navIndex == 0 ? Vector3.one * 1.05f : Vector3.one;

        if (sliderVFX != null)
            sliderVFX.transform.localScale =
                navIndex == 1 ? Vector3.one * 1.05f : Vector3.one;

        // Boutons
        styleBtnRetour?.SetSelectionne(navIndex == 3);
        styleBtnQuitterJeu?.SetSelectionne(navIndex == 4);
    }

    // ── Langue ────────────────────────────────────────────────────────────────
    void SelectionnerLangue(int index)
    {
        if (index < 0 || index >= codesLangue.Count) return;
        LocalisationManager.Instance?.ChangerLangue(codesLangue[index]);
        MettreAJourLangueVisuel();
        JouerSFX(sfxFocus);
    }

    void MettreAJourLangueVisuel()
    {
        string actuelle =
            LocalisationManager.Instance?.GetLangueActuelle() ?? "fr";

        for (int i = 0; i < stylesLangue.Count; i++)
            stylesLangue[i]?.SetSelectionne(
                i < codesLangue.Count &&
                codesLangue[i] == actuelle);
    }

    void JouerSFX(AudioClip clip)
    {
        if (sourceAudio != null && clip != null)
            sourceAudio.PlayOneShot(clip);
    }
}
