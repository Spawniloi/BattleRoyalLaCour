using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChoixJeuManager : MonoBehaviour
{
    [Header("Progression")]
    public TextMeshProUGUI txtProgression;

    [Header("Classement")]
    public RectTransform conteneurClassement;
    public GameObject prefabLigneClassement;

    [Header("Classement — points de spawn")]
    public Transform[] spawnsClassement;

    [Header("Boutons jeux")]
    public Button btnMaireCoraille;
    public Button btnBallonPrisonnier;
    public Button btnSnakeRacaille;
    public Button btnAleatoire;

    [Header("Styles boutons jeux")]
    public List<BoutonStylee> styles = new List<BoutonStylee>();

    [Header("Images jeux")]
    public Image imgJeuSelectionne;
    public Sprite spriteMaireCoraille;
    public Sprite spriteBallonPrisonnier;
    public Sprite spriteSnakeRacaille;

    [Header("Audio")]
    public AudioSource sourceAudio;
    public AudioClip sfxFocus;
    public AudioClip sfxValider;

    [Header("Couleurs")]
    public Color couleurNormal = Color.white;
    public Color couleurSelectionne = new Color(1f, 0.84f, 0f, 1f);
    public Color couleurDesactive = new Color(0.5f, 0.5f, 0.5f, 1f);

    private List<Button> boutons = new List<Button>();
    private int indexActuel = 0;

    string L(string cle, params string[] args)
    => LocalisationManager.Instance != null
        ? LocalisationManager.Instance.Get(cle, args)
        : cle;

    void Start()
    {
        AfficherProgression();
        AfficherClassement();
        InitBoutons();
        SurlígnerBouton(indexActuel);
    }

    // ── Progression manches ───────────────────────────────────────────────────
    void AfficherProgression()
    {
        if (txtProgression == null) return;

        if (GameSessionManager.Instance == null ||
            GameSessionManager.Instance.EstEntrainement())
        {
            txtProgression.text = L("choix_progression_test");
            return;
        }

        var session = GameSessionManager.Instance.session;
        txtProgression.text = L("choix_progression_manche",
            session.mancheActuelle.ToString(),
            session.nombreManches.ToString());
    }

    // ── Classement ────────────────────────────────────────────────────────────

    void AfficherClassement()
    {
        if (spawnsClassement == null || spawnsClassement.Length == 0) return;
        if (prefabLigneClassement == null) return;

        List<(int playerID, float score, int sucettes)> classement;

        if (GameSessionManager.Instance != null)
            classement = GameSessionManager.Instance.GetClassementFinal();
        else
        {
            classement = new List<(int, float, int)>();
            for (int i = 1; i <= GameData.nombreJoueurs; i++)
                classement.Add((i, 0f, 1));
        }

        for (int i = 0; i < classement.Count; i++)
        {
            if (i >= spawnsClassement.Length) break;

            var (playerID, score, sucettes) = classement[i];

            GameObject go = Instantiate(
                prefabLigneClassement,
                spawnsClassement[i].position,
                Quaternion.identity);

            go.transform.SetParent(spawnsClassement[i], false);
            go.transform.localPosition = Vector3.zero;

            LigneClassement ligne = go.GetComponent<LigneClassement>();
            ligne?.Appliquer(i + 1, playerID, score, sucettes);
        }
    }

    // ── Boutons jeux ──────────────────────────────────────────────────────────
    void InitBoutons()
    {
        boutons.Clear();
        boutons.Add(btnMaireCoraille);
        boutons.Add(btnBallonPrisonnier);
        boutons.Add(btnSnakeRacaille);
        boutons.Add(btnAleatoire);

        // ← ajoute après
        SurlígnerBouton(0); // Maire Coraille sélectionné par défaut

        btnMaireCoraille?.onClick.AddListener(
            () => LancerJeu("maire"));
        btnBallonPrisonnier?.onClick.AddListener(
            () => LancerJeu("ballon"));
        btnSnakeRacaille?.onClick.AddListener(
            () => LancerJeu("snake"));
        btnAleatoire?.onClick.AddListener(
            LancerAleatoire);
    }

    // ── Navigation manette ────────────────────────────────────────────────────
    void Update()
    {
        var kb = Keyboard.current;
        var gp = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        bool haut = false, bas = false, valider = false;

        if (kb != null)
        {
            haut = kb.upArrowKey.wasPressedThisFrame
                   || kb.zKey.wasPressedThisFrame;
            bas = kb.downArrowKey.wasPressedThisFrame
                   || kb.sKey.wasPressedThisFrame;
            valider = kb.enterKey.wasPressedThisFrame
                   || kb.spaceKey.wasPressedThisFrame;
        }

        if (gp != null)
        {
            haut = haut || gp.leftStick.up.wasPressedThisFrame
                              || gp.dpad.up.wasPressedThisFrame;
            bas = bas || gp.leftStick.down.wasPressedThisFrame
                              || gp.dpad.down.wasPressedThisFrame;
            valider = valider || gp.buttonSouth.wasPressedThisFrame;
        }

        if (haut) Naviguer(-1);
        if (bas) Naviguer(1);
        if (valider) Valider();
    }

    void Naviguer(int direction)
    {
        indexActuel = (indexActuel + direction + boutons.Count)
                    % boutons.Count;

        SurlígnerBouton(indexActuel);

        // Affiche image du jeu surligné
        MettreAJourImageJeu(indexActuel);

        if (sourceAudio != null && sfxFocus != null)
            sourceAudio.PlayOneShot(sfxFocus);
    }

    void Valider()
    {
        boutons[indexActuel]?.onClick.Invoke();
    }

    // GARDE SEULEMENT CELLE-CI :
    void SurlígnerBouton(int index)
    {
        for (int i = 0; i < styles.Count; i++)
            styles[i]?.SetSelectionne(i == index);

        MettreAJourImageJeu(index);
    }

    void MettreAJourImageJeu(int index)
    {
        if (imgJeuSelectionne == null) return;

        imgJeuSelectionne.sprite = index switch
        {
            0 => spriteMaireCoraille,
            1 => spriteBallonPrisonnier,
            2 => spriteSnakeRacaille,
            3 => null,
            _ => null
        };

        imgJeuSelectionne.enabled =
            imgJeuSelectionne.sprite != null;
    }

    // ── Lancement ─────────────────────────────────────────────────────────────
    void LancerJeu(string jeu)
    {
        if (sourceAudio != null && sfxValider != null)
            sourceAudio.PlayOneShot(sfxValider);

        GameData.jeuActuel = jeu;

        string scene = jeu switch
        {
            "maire" => "Scene_MaireCoraille",
            "ballon" => "Scene_Dodgeball",
            "snake" => "Scene_SnakeRacaille",
            _ => "Scene_MaireCoraille"
        };

        SceneManager.LoadScene(scene);
    }

    void LancerAleatoire()
    {
        string[] jeux = { "maire", "ballon", "snake" };
        string jeu = jeux[Random.Range(0, jeux.Length)];
        LancerJeu(jeu);
    }


}
