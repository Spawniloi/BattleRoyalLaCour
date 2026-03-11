using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

    [Header("Avertissement")]
    public TextMeshProUGUI txtAvertissement;

    [Header("Style boutons joueurs")]
    public BoutonStylee styleBtn2J;
    public BoutonStylee styleBtn3J;
    public BoutonStylee styleBtn4J;

    [Header("Boutons bas — dans l'ordre : ModeTest, Retour, Continuer")]
    public Button btnModeTest;
    public Button btnRetour;
    public Button btnContinuer;
    public BoutonStylee styleBtnModeTest;
    public BoutonStylee styleBtnRetour;
    public BoutonStylee styleBtnContinuer;

    [Header("Audio")]
    public AudioSource sourceAudio;
    public AudioClip sfxFocus;
    public AudioClip sfxValider;
    public AudioClip sfxRetour;

    private int nbJoueurs = 2;
    private int nbManches = 3;

    private List<Button> boutonsNav = new List<Button>();
    private List<BoutonStylee> stylesNav = new List<BoutonStylee>();
    private int indexNav = 2; // Continuer par défaut
    private bool navActif = false;
    private bool inputBloque = false;

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
        sliderManches.interactable = false;

        // Boutons joueurs
        btn2J?.onClick.AddListener(() => SelectionnerJoueurs(2));
        btn3J?.onClick.AddListener(() => SelectionnerJoueurs(3));
        btn4J?.onClick.AddListener(() => SelectionnerJoueurs(4));

        // Boutons bas
        btnModeTest?.onClick.AddListener(LancerModeTest);
        btnRetour?.onClick.AddListener(Fermer);
        btnContinuer?.onClick.AddListener(Continuer);

        // Navigation bas — ModeTest / Retour / Continuer
        boutonsNav.Clear();
        stylesNav.Clear();
        boutonsNav.Add(btnModeTest);
        boutonsNav.Add(btnRetour);
        boutonsNav.Add(btnContinuer);
        stylesNav.Add(styleBtnModeTest);
        stylesNav.Add(styleBtnRetour);
        stylesNav.Add(styleBtnContinuer);

        MettreAJourBoutonsJoueurs();
        MasquerAvertissement();
    }

    // ── Ouvre ─────────────────────────────────────────────────────────────────
    public void Ouvrir()
    {
        panelConfig?.SetActive(true);
        nbJoueurs = 2;
        nbManches = manchesDefaut;
        sliderManches.value = manchesDefaut;
        MettreAJourBoutonsJoueurs();
        MettreAJourTexteManches();
        MasquerAvertissement();
        navActif = true;
        indexNav = 2; // Continuer sélectionné par défaut
        SurlígnerNav(indexNav);
    }

    // ── Ferme ─────────────────────────────────────────────────────────────────
    public void Fermer()
    {
        navActif = false;
        JouerSFX(sfxRetour);
        StartCoroutine(BloquerInputUnFrame()); // ← nom correct
        panelConfig?.SetActive(false);
    }

    IEnumerator BloquerInputUnFrame()
    {
        inputBloque = true;
        yield return null;

        bool encoreAppuye = true;
        while (encoreAppuye)
        {
            encoreAppuye = false;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.isPressed) encoreAppuye = true;
                if (Keyboard.current.spaceKey.isPressed) encoreAppuye = true;
                if (Keyboard.current.enterKey.isPressed) encoreAppuye = true;
            }

            foreach (var gp in Gamepad.all)
            {
                if (gp.startButton.isPressed) encoreAppuye = true;
                if (gp.buttonEast.isPressed) encoreAppuye = true;
                if (gp.buttonSouth.isPressed) encoreAppuye = true;
            }

            if (encoreAppuye) yield return null;
        }

        yield return null;
        inputBloque = false;
    }

    // ── Update ────────────────────────────────────────────────────────────────
    void Update()
    {
        if (!navActif) return;
        if (inputBloque) return;

        var kb = Keyboard.current;
        var gp = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        bool gauche = false, droite = false;
        bool haut = false, bas = false;
        bool valider = false, retour = false;
        float sliderDelta = 0f;

        if (kb != null)
        {
            gauche = kb.qKey.wasPressedThisFrame
                   || kb.leftArrowKey.wasPressedThisFrame;
            droite = kb.dKey.wasPressedThisFrame
                   || kb.rightArrowKey.wasPressedThisFrame;
            haut = kb.zKey.wasPressedThisFrame
                   || kb.upArrowKey.wasPressedThisFrame;
            bas = kb.sKey.wasPressedThisFrame
                   || kb.downArrowKey.wasPressedThisFrame;
            valider = kb.spaceKey.wasPressedThisFrame
                   || kb.enterKey.wasPressedThisFrame;
            retour = kb.escapeKey.wasPressedThisFrame;

            if (kb.eKey.isPressed) sliderDelta = 1f;
            if (kb.aKey.isPressed) sliderDelta = -1f;
        }

        if (gp != null)
        {
            // Joueurs nb — stick gauche gauche/droite
            gauche = gauche || gp.leftStick.left.wasPressedThisFrame
                             || gp.dpad.left.wasPressedThisFrame;
            droite = droite || gp.leftStick.right.wasPressedThisFrame
                             || gp.dpad.right.wasPressedThisFrame;

            // Boutons bas — stick gauche haut/bas
            haut = haut || gp.leftStick.up.wasPressedThisFrame
                           || gp.dpad.up.wasPressedThisFrame;
            bas = bas || gp.leftStick.down.wasPressedThisFrame
                           || gp.dpad.down.wasPressedThisFrame;

            valider = valider || gp.buttonSouth.wasPressedThisFrame;
            retour = retour || gp.buttonEast.wasPressedThisFrame;

            // Slider — bumpers R1/L1 (plus simple et fiable)
            if (gp.rightShoulder.wasPressedThisFrame)
            {
                sliderManches.value = Mathf.Min(
                    sliderManches.value + 1,
                    manchesMax);
                JouerSFX(sfxFocus);
            }
            if (gp.leftShoulder.wasPressedThisFrame)
            {
                sliderManches.value = Mathf.Max(
                    sliderManches.value - 1,
                    manchesMin);
                JouerSFX(sfxFocus);
            }
        }
        // Clavier — même chose
        if (kb != null)
        {
            if (kb.eKey.wasPressedThisFrame)
            {
                sliderManches.value = Mathf.Min(
                    sliderManches.value + 1,
                    manchesMax);
                JouerSFX(sfxFocus);
            }
            if (kb.aKey.wasPressedThisFrame)
            {
                sliderManches.value = Mathf.Max(
                    sliderManches.value - 1,
                    manchesMin);
                JouerSFX(sfxFocus);
            }
        }
        // Nb joueurs ←/→
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

        // Boutons bas ↑/↓
        if (haut) Naviguer(-1);
        if (bas) Naviguer(1);
        if (valider) ValiderNav();
        if (retour) Fermer();

        // Slider manches — gâchettes
        if (Mathf.Abs(sliderDelta) > 0.05f)
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
        txtManches.text = $"Manches : {nbManches}";
    }

    // ── Mode test ─────────────────────────────────────────────────────────────
    void LancerModeTest()
    {
        if (nbJoueurs < 2)
        {
            AfficherAvertissement("Selectionne au moins 2 joueurs !");
            return;
        }

        JouerSFX(sfxValider);

        GameData.nombreJoueurs = nbJoueurs;

        GameSessionManager.Instance?.DemarrerSession(
            "entrainement",
            1,
            new List<string> { "maire", "ballon", "snake" },
            true);

        navActif = false;
        panelConfig?.SetActive(false);
        SceneManager.LoadScene("Scene_ChoixJeu");
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

        GameSessionManager.Instance?.DemarrerSession(
            "manche",
            nbManches,
            new List<string> { "maire", "ballon", "snake" },
            true);

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
