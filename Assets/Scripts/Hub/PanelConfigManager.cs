using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Style slider manches")]
    public BoutonStylee styleSliderManches;

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

    // indexNav : 0=slider  1=ModeTest  2=Retour  3=Continuer
    private int indexNav = 3;
    private bool navActif = false;
    private bool inputBloque = false;

    void Awake()
    {
        panelConfig?.SetActive(false);
    }

    void Start()
    {
        sliderManches.minValue = manchesMin;
        sliderManches.maxValue = manchesMax;
        sliderManches.wholeNumbers = true;
        sliderManches.value = manchesDefaut;
        sliderManches.interactable = true; // ← on laisse interactable
        sliderManches.onValueChanged.AddListener(OnSliderChange);
        MettreAJourTexteManches();

        btn2J?.onClick.AddListener(() => SelectionnerJoueurs(2));
        btn3J?.onClick.AddListener(() => SelectionnerJoueurs(3));
        btn4J?.onClick.AddListener(() => SelectionnerJoueurs(4));

        btnModeTest?.onClick.AddListener(LancerModeTest);
        btnRetour?.onClick.AddListener(Fermer);
        btnContinuer?.onClick.AddListener(Continuer);

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
        indexNav = 3; // Continuer par défaut
        SurlígnerNav();

        if (LocalisationManager.Instance != null &&
            !LocalisationManager.Instance.EstCharge())
            LocalisationManager.Instance.onTraductionsChargees
                += MettreAJourTexteManches;
    }

    // ── Ferme ─────────────────────────────────────────────────────────────────
    public void Fermer()
    {
        navActif = false;
        JouerSFX(sfxRetour);
        StartCoroutine(BloquerInputUnFrame());
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
        }

        if (gp != null)
        {
            gauche = gauche || gp.leftStick.left.wasPressedThisFrame
                              || gp.dpad.left.wasPressedThisFrame;
            droite = droite || gp.leftStick.right.wasPressedThisFrame
                              || gp.dpad.right.wasPressedThisFrame;
            haut = haut || gp.leftStick.up.wasPressedThisFrame
                              || gp.dpad.up.wasPressedThisFrame;
            bas = bas || gp.leftStick.down.wasPressedThisFrame
                              || gp.dpad.down.wasPressedThisFrame;
            valider = valider || gp.buttonSouth.wasPressedThisFrame;
            retour = retour || gp.buttonEast.wasPressedThisFrame;
        }

        // ── Slider sélectionné (indexNav == 0) — gachettes / Q+D ─────────────
        if (indexNav == 0)
        {
            bool plus = false;
            bool moins = false;

            if (gp != null)
            {
                plus = gp.rightShoulder.wasPressedThisFrame;
                moins = gp.leftShoulder.wasPressedThisFrame;
            }
            if (kb != null)
            {
                plus = plus || droite;
                moins = moins || gauche;
            }

            if (plus)
            {
                sliderManches.value = Mathf.Min(sliderManches.value + 1, manchesMax);
                JouerSFX(sfxFocus);
            }
            if (moins)
            {
                sliderManches.value = Mathf.Max(sliderManches.value - 1, manchesMin);
                JouerSFX(sfxFocus);
            }
        }
        else
        {
            // ── Nb joueurs ←/→ (seulement si slider pas sélectionné) ──────────
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
        }

        if (haut) Naviguer(-1);
        if (bas) Naviguer(1);
        if (valider) ValiderNav();
        if (retour) Fermer();
    }

    // ── Navigation ────────────────────────────────────────────────────────────
    // 0=slider  1=ModeTest  2=Retour  3=Continuer
    void Naviguer(int dir)
    {
        indexNav = (indexNav + dir + 4) % 4;
        SurlígnerNav();
        JouerSFX(sfxFocus);
    }

    void ValiderNav()
    {
        switch (indexNav)
        {
            case 1: LancerModeTest(); break;
            case 2: Fermer(); break;
            case 3: Continuer(); break;
                // case 0 : slider — rien à valider
        }
    }

    void SurlígnerNav()
    {
        styleSliderManches?.SetSelectionne(indexNav == 0);
        styleBtnModeTest?.SetSelectionne(indexNav == 1);
        styleBtnRetour?.SetSelectionne(indexNav == 2);
        styleBtnContinuer?.SetSelectionne(indexNav == 3);
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

        if (LocalisationManager.Instance != null &&
            LocalisationManager.Instance.EstCharge())
        {
            txtManches.text = LocalisationManager.Instance.Get(
                "config_manches", nbManches.ToString());
        }
        else
        {
            txtManches.text = $"Manches : {nbManches}";
        }
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
            "entrainement", 1,
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

        Debug.Log($"[Config] DemarrerSession manches={nbManches} joueurs={nbJoueurs}");

        GameSessionManager.Instance?.DemarrerSession(
            "manche", nbManches,
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