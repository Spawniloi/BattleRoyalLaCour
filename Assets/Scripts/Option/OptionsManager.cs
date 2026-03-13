using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

    // 0=sliderMusique 1=sliderVFX 2=langue 3=retour 4=quitterJeu
    private int navIndex = 3;
    private bool estOuvert = false;
    private bool etaitEnJeu = false;
    private bool inputBloque = false;

    private static readonly HashSet<string> scenesJeu = new HashSet<string>
    {
        "Scene_MaireCoraille",
        "Scene_Dodgeball",
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

        for (int i = 0; i < btnsLangue.Count; i++)
        {
            int idx = i;
            btnsLangue[i]?.onClick.AddListener(
                () => SelectionnerLangue(idx));
        }

        btnRetour?.onClick.AddListener(Fermer);
        btnQuitterJeu?.onClick.AddListener(QuitterJeu);
        MettreAJourLangueVisuel();
    }

    void Update()
    {
        if (inputBloque) return;

        var kb = Keyboard.current;
        var gp0 = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        // ── Panel fermé — détecte ouverture ──────────────────────────────────
        if (!estOuvert)
        {
            bool ouvrir = false;

            if (kb != null && kb.escapeKey.wasPressedThisFrame)
                ouvrir = true;

            foreach (var gp in Gamepad.all)
                if (gp.startButton.wasPressedThisFrame)
                    ouvrir = true;

            if (ouvrir) Ouvrir();
            return;
        }

        // ── Inputs navigation ─────────────────────────────────────────────────
        bool haut = false, bas = false;
        bool valider = false, fermer = false;

        if (kb != null)
        {
            haut = kb.upArrowKey.wasPressedThisFrame
                   || kb.zKey.wasPressedThisFrame;
            bas = kb.downArrowKey.wasPressedThisFrame
                   || kb.sKey.wasPressedThisFrame;
            valider = kb.spaceKey.wasPressedThisFrame
                   || kb.enterKey.wasPressedThisFrame;
            fermer = kb.escapeKey.wasPressedThisFrame;
        }

        if (gp0 != null)
        {
            haut = haut || gp0.dpad.up.wasPressedThisFrame
                              || gp0.leftStick.up.wasPressedThisFrame;
            bas = bas || gp0.dpad.down.wasPressedThisFrame
                              || gp0.leftStick.down.wasPressedThisFrame;
            valider = valider || gp0.buttonSouth.wasPressedThisFrame;
            fermer = fermer || gp0.buttonEast.wasPressedThisFrame
                              || gp0.startButton.wasPressedThisFrame;
        }

        // ── Gachettes LB/RB + clavier A/E — sliders et langue ────────────────
        if (gp0 != null)
        {
            bool lb = gp0.leftShoulder.wasPressedThisFrame;
            bool rb = gp0.rightShoulder.wasPressedThisFrame;

            if (navIndex == 0 && sliderMusique != null)
            {
                if (rb) sliderMusique.value =
                    Mathf.Clamp01(sliderMusique.value + 0.05f);
                if (lb) sliderMusique.value =
                    Mathf.Clamp01(sliderMusique.value - 0.05f);
            }
            if (navIndex == 1 && sliderVFX != null)
            {
                if (rb) sliderVFX.value =
                    Mathf.Clamp01(sliderVFX.value + 0.05f);
                if (lb) sliderVFX.value =
                    Mathf.Clamp01(sliderVFX.value - 0.05f);
            }
            if (navIndex == 2)
            {
                if (rb) NaviguerLangue(1);
                if (lb) NaviguerLangue(-1);
            }
        }

        if (kb != null)
        {
            bool gauche = kb.aKey.wasPressedThisFrame
                       || kb.leftArrowKey.wasPressedThisFrame
                       || kb.qKey.wasPressedThisFrame;
            bool droite = kb.eKey.wasPressedThisFrame
                       || kb.rightArrowKey.wasPressedThisFrame
                       || kb.dKey.wasPressedThisFrame;

            if (navIndex == 0 && sliderMusique != null)
            {
                if (droite) sliderMusique.value =
                    Mathf.Clamp01(sliderMusique.value + 0.05f);
                if (gauche) sliderMusique.value =
                    Mathf.Clamp01(sliderMusique.value - 0.05f);
            }
            if (navIndex == 1 && sliderVFX != null)
            {
                if (droite) sliderVFX.value =
                    Mathf.Clamp01(sliderVFX.value + 0.05f);
                if (gauche) sliderVFX.value =
                    Mathf.Clamp01(sliderVFX.value - 0.05f);
            }
            if (navIndex == 2)
            {
                if (droite) NaviguerLangue(1);
                if (gauche) NaviguerLangue(-1);
            }
        }

        // ── Navigation ↑↓ ────────────────────────────────────────────────────
        if (haut) Naviguer(-1);
        if (bas) Naviguer(1);
        if (valider) ValiderNav();
        if (fermer) Fermer();
    }

    // ── Ouvrir ────────────────────────────────────────────────────────────────
    public void Ouvrir()
    {
        estOuvert = true;
        string scene = SceneManager.GetActiveScene().name;
        etaitEnJeu = scenesJeu.Contains(scene);

        if (etaitEnJeu) Time.timeScale = 0f;

        zoneQuitterJeu?.SetActive(etaitEnJeu);
        panelOptions?.SetActive(true);

        if (sliderMusique != null && AudioManager.Instance != null)
            sliderMusique.value = AudioManager.Instance.GetVolMusique();
        if (sliderVFX != null && AudioManager.Instance != null)
            sliderVFX.value = AudioManager.Instance.GetVolVFX();

        MettreAJourLangueVisuel();
        navIndex = 3;
        SurlígnerNav();
        JouerSFX(sfxValider);
    }

    // ── Fermer ────────────────────────────────────────────────────────────────
    public void Fermer()
    {
        estOuvert = false;
        if (etaitEnJeu) Time.timeScale = 1f;
        AudioManager.Instance?.Sauvegarder();
        JouerSFX(sfxRetour);
        StartCoroutine(BloquerJusquAuRelachement());
        panelOptions?.SetActive(false);
    }

    IEnumerator BloquerJusquAuRelachement()
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

    // ── Quitter vers Hub ──────────────────────────────────────────────────────
    void QuitterJeu()
    {
        Time.timeScale = 1f;
        estOuvert = false;
        AudioManager.Instance?.Sauvegarder();
        panelOptions?.SetActive(false);
        SceneManager.LoadScene("Scene_Hub");
        JouerSFX(sfxValider);
    }

    // ── Navigation ───────────────────────────────────────────────────────────
    void Naviguer(int dir)
    {
        int max = etaitEnJeu ? 4 : 3;
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

    void NaviguerLangue(int dir)
    {
        int idx = codesLangue.IndexOf(
            LocalisationManager.Instance?.GetLangueActuelle() ?? "fr");
        int newIdx = (idx + dir + codesLangue.Count) % codesLangue.Count;
        SelectionnerLangue(newIdx);
    }

    void SurlígnerNav()
    {
        // Sliders
        if (sliderMusique != null)
            sliderMusique.transform.localScale =
                navIndex == 0 ? Vector3.one * 1.05f : Vector3.one;
        if (sliderVFX != null)
            sliderVFX.transform.localScale =
                navIndex == 1 ? Vector3.one * 1.05f : Vector3.one;

        // Boutons langue — tous grossissent quand navIndex == 2
        string langueActuelle =
            LocalisationManager.Instance?.GetLangueActuelle() ?? "fr";

        for (int i = 0; i < btnsLangue.Count; i++)
        {
            if (btnsLangue[i] != null)
                btnsLangue[i].transform.localScale =
                    navIndex == 2 ? Vector3.one * 1.08f : Vector3.one;

            if (i < stylesLangue.Count)
                stylesLangue[i]?.SetSelectionne(
                    navIndex == 2 &&
                    i < codesLangue.Count &&
                    codesLangue[i] == langueActuelle);
        }

        styleBtnRetour?.SetSelectionne(navIndex == 3);
        styleBtnQuitterJeu?.SetSelectionne(navIndex == 4);
    }

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