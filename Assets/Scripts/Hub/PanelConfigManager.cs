using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelConfigManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelConfig;

    [Header("Joueurs — boutons")]
    public Button btn2J;
    public Button btn3J;
    public Button btn4J;

    [Header("Manches")]
    public Slider sliderManches;
    public TextMeshProUGUI txtManches;
    public int manchesMin = 1;
    public int manchesMax = 10;
    public int manchesDefaut = 3;

    [Header("Mode test")]
    public Button btnModeTest;

    [Header("Boutons action")]
    public Button btnRetour;
    public Button btnContinuer;

    [Header("Avertissement")]
    public TextMeshProUGUI txtAvertissement;

    [Header("Style boutons joueurs")]
    public BoutonStylee styleBtn2J;
    public BoutonStylee styleBtn3J;
    public BoutonStylee styleBtn4J;

    [Header("Style boutons action")]
    public BoutonStylee styleBtnModeTest;
    public BoutonStylee styleBtnRetour;
    public BoutonStylee styleBtnContinuer;

    [Header("Navigation")]
    public float echelleNormal = 1f;
    public float echelleSelect = 1.1f;

    [Header("Audio")]
    public AudioSource sourceAudio;
    public AudioClip sfxFocus;
    public AudioClip sfxValider;
    public AudioClip sfxRetour;

    private int nbJoueurs = 2;
    private int nbManches = 3;
    private bool modeTest = false;

    // Navigation dans le panel
    private List<Button> boutonsNav = new List<Button>();
    private List<BoutonStylee> stylesNav = new List<BoutonStylee>();
    private int indexNav = 0;
    private bool navActif = false;

    void Awake()
    {
        panelConfig?.SetActive(false);
    }

    void Start()
    {
        // Slider
        sliderManches.minValue = manchesMin;
        sliderManches.maxValue = manchesMax;
        sliderManches.wholeNumbers = true;
        sliderManches.value = manchesDefaut;
        sliderManches.onValueChanged.AddListener(OnSliderChange);
        MettreAJourTexteManches();

        // Boutons joueurs
        btn2J?.onClick.AddListener(() => SelectionnerJoueurs(2));
        btn3J?.onClick.AddListener(() => SelectionnerJoueurs(3));
        btn4J?.onClick.AddListener(() => SelectionnerJoueurs(4));

        // Mode test
        btnModeTest?.onClick.AddListener(ToggleModeTest);

        // Action
        btnRetour?.onClick.AddListener(Fermer);
        btnContinuer?.onClick.AddListener(Continuer);

        // Navigation manette — ordre des boutons
        boutonsNav.Add(btnRetour);
        boutonsNav.Add(btnContinuer);
        stylesNav.Add(styleBtnRetour);
        stylesNav.Add(styleBtnContinuer);

        MettreAJourBoutonsJoueurs();
        MasquerAvertissement();
    }

    // ── Ouvre ────────────────────────────────────────────────────────────────
    public void Ouvrir()
    {
        panelConfig?.SetActive(true);
        nbJoueurs = 2;
        MettreAJourBoutonsJoueurs();
        MasquerAvertissement();
        navActif = true;
        indexNav = 1; // Continuer sélectionné par défaut
        SurlígnerNav(indexNav);
    }

    // ── Ferme ────────────────────────────────────────────────────────────────
    public void Fermer()
    {
        JouerSFX(sfxRetour);
        navActif = false;
        panelConfig?.SetActive(false);
    }

    // ── Update — navigation manette ───────────────────────────────────────────
    void Update()
    {
        if (!navActif) return;

        var kb = Keyboard.current;
        var gp = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        bool gauche = false, droite = false;
        bool haut = false, bas = false;
        bool valider = false, retour = false;
        float sliderDelta = 0f;

        if (kb != null)
        {
            // Joueurs — gauche/droite
            gauche = kb.qKey.wasPressedThisFrame
                   || kb.leftArrowKey.wasPressedThisFrame;
            droite = kb.dKey.wasPressedThisFrame
                   || kb.rightArrowKey.wasPressedThisFrame;

            // Actions — haut/bas
            haut = kb.zKey.wasPressedThisFrame
                   || kb.upArrowKey.wasPressedThisFrame;
            bas = kb.sKey.wasPressedThisFrame
                   || kb.downArrowKey.wasPressedThisFrame;

            valider = kb.spaceKey.wasPressedThisFrame
                   || kb.enterKey.wasPressedThisFrame;
            retour = kb.escapeKey.wasPressedThisFrame;

            // Slider — A/E ou PageUp/Down
            if (kb.eKey.isPressed) sliderDelta = 1f;
            if (kb.aKey.isPressed) sliderDelta = -1f;
        }

        if (gp != null)
        {
            // Joueurs — dpad gauche/droite
            gauche = gauche || gp.dpad.left.wasPressedThisFrame;
            droite = droite || gp.dpad.right.wasPressedThisFrame;

            // Actions — dpad haut/bas
            haut = haut || gp.dpad.up.wasPressedThisFrame;
            bas = bas || gp.dpad.down.wasPressedThisFrame;

            valider = valider || gp.buttonSouth.wasPressedThisFrame;
            retour = retour || gp.buttonEast.wasPressedThisFrame;

            // Slider — gâchettes L2/R2
            sliderDelta += gp.rightTrigger.ReadValue()
                         - gp.leftTrigger.ReadValue();
        }

        // ── Joueurs gauche/droite ─────────────────────────────────────────────
        if (gauche)
        {
            int nouveau = nbJoueurs - 1;
            if (nouveau < 2) nouveau = 4;
            SelectionnerJoueurs(nouveau);
        }
        if (droite)
        {
            int nouveau = nbJoueurs + 1;
            if (nouveau > 4) nouveau = 2;
            SelectionnerJoueurs(nouveau);
        }

        // ── Actions haut/bas ──────────────────────────────────────────────────
        if (haut) Naviguer(-1);
        if (bas) Naviguer(1);
        if (valider) ValiderNav();
        if (retour) Fermer();

        // ── Slider manches ────────────────────────────────────────────────────
        if (!modeTest && Mathf.Abs(sliderDelta) > 0.05f)
            sliderManches.value += sliderDelta * Time.deltaTime * 5f;
    }

    void Naviguer(int dir)
    {
        indexNav = (indexNav + dir + boutonsNav.Count) % boutonsNav.Count;
        SurlígnerNav(indexNav);
        JouerSFX(sfxFocus);
    }

    void ValiderNav()
    {
        boutonsNav[indexNav]?.onClick.Invoke();
    }

    void SurlígnerNav(int index)
    {
        for (int i = 0; i < stylesNav.Count; i++)
            stylesNav[i]?.SetSelectionne(i == index);
    }

    // ── Joueurs ───────────────────────────────────────────────────────────────
    void SelectionnerJoueurs(int nb)
    {
        nbJoueurs = nb;
        MettreAJourBoutonsJoueurs();
        MasquerAvertissement();
        JouerSFX(sfxFocus);
    }

    void MettreAJourBoutonsJoueurs()
    {
        styleBtn2J?.SetSelectionne(nbJoueurs == 2);
        styleBtn3J?.SetSelectionne(nbJoueurs == 3);
        styleBtn4J?.SetSelectionne(nbJoueurs == 4);
    }

    // ── Slider ────────────────────────────────────────────────────────────────
    void OnSliderChange(float valeur)
    {
        nbManches = (int)valeur;
        MettreAJourTexteManches();
    }

    void MettreAJourTexteManches()
    {
        if (txtManches == null) return;
        txtManches.text = modeTest
            ? "Mode test — pas de classement"
            : $"Manches : {nbManches}";
    }

    // ── Mode test ─────────────────────────────────────────────────────────────
    void ToggleModeTest()
    {
        modeTest = !modeTest;
        sliderManches.interactable = !modeTest;
        styleBtnModeTest?.SetSelectionne(modeTest);
        MettreAJourTexteManches();
        JouerSFX(sfxFocus);
    }

    // ── Continuer ─────────────────────────────────────────────────────────────
    void Continuer()
    {
        if (nbJoueurs < 2)
        {
            AfficherAvertissement("Selectionne au moins 2 joueurs !");
            return;
        }

        JouerSFX(sfxValider);

        GameData.nombreJoueurs = nbJoueurs;

        if (GameSessionManager.Instance != null)
        {
            string mode = modeTest ? "entrainement" : "manche";
            int manches = modeTest ? 1 : nbManches;

            GameSessionManager.Instance.DemarrerSession(
                mode,
                manches,
                new System.Collections.Generic.List<string> {
                    "maire", "ballon", "snake" },
                true);
        }

        navActif = false;
        panelConfig?.SetActive(false);
        SceneManager.LoadScene("Scene_ChoixJeu");
    }

    // ── Avertissement ─────────────────────────────────────────────────────────
    void AfficherAvertissement(string msg)
    {
        if (txtAvertissement == null) return;
        txtAvertissement.text = msg;
        txtAvertissement.enabled = true;
    }

    void MasquerAvertissement()
    {
        if (txtAvertissement == null) return;
        txtAvertissement.enabled = false;
    }

    void JouerSFX(AudioClip clip)
    {
        if (sourceAudio != null && clip != null)
            sourceAudio.PlayOneShot(clip);
    }
}
