using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PanelConfigManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelConfig;

    [Header("Joueurs")]
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

    [Header("Boutons")]
    public Button btnRetour;
    public Button btnContinuer;

    [Header("Avertissement")]
    public TextMeshProUGUI txtAvertissement;

    [Header("Couleurs")]
    public Color couleurNormal = Color.white;
    public Color couleurSelectionne = new Color(1f, 0.84f, 0f, 1f);

    [Header("Audio")]
    public AudioSource sourceAudio;
    public AudioClip sfxFocus;
    public AudioClip sfxValider;
    public AudioClip sfxRetour;

    private int nbJoueurs = 2;
    private int nbManches = 3;
    private bool modeTest = false;

    void Awake()
    {
        // Panel fermé au départ
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

        // Retour / Continuer
        btnRetour?.onClick.AddListener(Fermer);
        btnContinuer?.onClick.AddListener(Continuer);

        // Visuel initial
        nbJoueurs = 2;
        MettreAJourBoutonsJoueurs();
        MasquerAvertissement();
    }

    // ── Ouvre le panel ────────────────────────────────────────────────────────
    public void Ouvrir()
    {
        panelConfig?.SetActive(true);
        MettreAJourBoutonsJoueurs();
        MasquerAvertissement();
    }

    // ── Ferme le panel ────────────────────────────────────────────────────────
    public void Fermer()
    {
        if (sourceAudio != null && sfxRetour != null)
            sourceAudio.PlayOneShot(sfxRetour);

        panelConfig?.SetActive(false);
    }

    // ── Sélection joueurs ─────────────────────────────────────────────────────
    void SelectionnerJoueurs(int nb)
    {
        nbJoueurs = nb;
        MettreAJourBoutonsJoueurs();
        MasquerAvertissement();

        if (sourceAudio != null && sfxFocus != null)
            sourceAudio.PlayOneShot(sfxFocus);
    }

    void MettreAJourBoutonsJoueurs()
    {
        SurlígnerBouton(btn2J, nbJoueurs == 2);
        SurlígnerBouton(btn3J, nbJoueurs == 3);
        SurlígnerBouton(btn4J, nbJoueurs == 4);
    }

    // ── Slider manches ────────────────────────────────────────────────────────
    void OnSliderChange(float valeur)
    {
        nbManches = (int)valeur;
        MettreAJourTexteManches();
    }

    void MettreAJourTexteManches()
    {
        if (txtManches == null) return;

        if (modeTest)
            txtManches.text = "Mode test — pas de classement";
        else
            txtManches.text = $"Manches : {nbManches}";
    }

    // ── Mode test ─────────────────────────────────────────────────────────────
    void ToggleModeTest()
    {
        modeTest = !modeTest;

        // Désactive le slider en mode test
        sliderManches.interactable = !modeTest;

        // Visuel bouton mode test
        SurlígnerBouton(btnModeTest, modeTest);

        MettreAJourTexteManches();

        if (sourceAudio != null && sfxFocus != null)
            sourceAudio.PlayOneShot(sfxFocus);
    }

    // ── Continuer ─────────────────────────────────────────────────────────────
    void Continuer()
    {
        if (nbJoueurs < 2)
        {
            AfficherAvertissement("Sélectionne au moins 2 joueurs !");
            return;
        }

        if (sourceAudio != null && sfxValider != null)
            sourceAudio.PlayOneShot(sfxValider);

        // Enregistre dans GameData
        GameData.nombreJoueurs = nbJoueurs;

        // Démarre la session
        if (GameSessionManager.Instance != null)
        {
            string mode = modeTest ? "entrainement" : "manche";
            int manches = modeTest ? 1 : nbManches;

            // Jeux par défaut — sera modifié dans Scene_ChoixJeu
            GameSessionManager.Instance.DemarrerSession(
                mode,
                manches,
                new System.Collections.Generic.List<string> {
                    "maire", "ballon", "snake" },
                true);
        }

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

    // ── Visuel bouton surligné ────────────────────────────────────────────────
    void SurlígnerBouton(Button btn, bool actif)
    {
        if (btn == null) return;

        TextMeshProUGUI txt =
            btn.GetComponentInChildren<TextMeshProUGUI>();

        if (txt != null)
            txt.color = actif ? couleurSelectionne : couleurNormal;

        btn.transform.localScale = actif
            ? Vector3.one * 1.08f
            : Vector3.one;
    }
}