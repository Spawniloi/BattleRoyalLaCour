using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelSessionFin : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelFin;

    [Header("Podium — spawns")]
    public Transform spawn1er;
    public Transform spawn2e;
    public Transform spawn3e;
    public Transform spawn4e;
    public GameObject podiumVisuelPrefab;
    public float echellePodium = 2f;

    [Header("Médailles — textes à côté du podium")]
    public TextMeshProUGUI txtMedaille1er;
    public TextMeshProUGUI txtMedaille2e;
    public TextMeshProUGUI txtMedaille3e;
    public TextMeshProUGUI txtMedaille4e;

    [Header("Texte gagnant")]
    public TextMeshProUGUI txtGagnant;

    [Header("Titre")]
    public TextMeshProUGUI txtTitre;

    [Header("Boutons")]
    public Button btnMenu;
    public Button btnQuitter;
    public BoutonStylee styleBtnMenu;
    public BoutonStylee styleBtnQuitter;

    [Header("Audio")]
    public AudioSource sourceAudio;
    public AudioClip sfxFocus;
    public AudioClip sfxValider;

    [Header("Couleurs")]
    public Color couleurOr = new Color(1f, 0.84f, 0f, 1f);
    public Color couleurArgent = new Color(0.75f, 0.75f, 0.75f, 1f);
    public Color couleurBronze = new Color(0.80f, 0.50f, 0.20f, 1f);
    public Color couleurCraie = new Color(0.95f, 0.95f, 0.90f, 1f);

    [Header("Fonte")]
    public TMP_FontAsset fonteCraie;

    private bool estVisible = false;
    private int navIndex = 0; // 0=menu 1=quitter
    private bool inputBloque = false;

    string L(string cle, params string[] args)
        => LocalisationManager.Instance != null
            ? LocalisationManager.Instance.Get(cle, args)
            : cle;

    void Start()
    {
        panelFin?.SetActive(false);
        btnMenu?.onClick.AddListener(RetourMenu);
        btnQuitter?.onClick.AddListener(Quitter);

        bool sessionFinie = GameSessionManager.Instance != null &&
                            GameSessionManager.Instance.SessionTerminee();

        Debug.Log($"[PanelFin] SessionTerminee = {sessionFinie}");

        if (sessionFinie)
            StartCoroutine(AfficherApresDelai());
    }

    IEnumerator AfficherApresDelai()
    {
        yield return new WaitForSeconds(0.3f);
        Afficher();
    }

    public void Afficher()
    {
        estVisible = true;
        panelFin?.SetActive(true);

        // Bloque les boutons jeux
        ChoixJeuManager choix =
            FindFirstObjectByType<ChoixJeuManager>();
        if (choix != null) choix.enabled = false;

        StartCoroutine(AnimerPodium());

        navIndex = 0;
        SurlígnerNav();
        JouerSFX(sfxValider);
    }

    IEnumerator AnimerPodium()
    {
        if (GameSessionManager.Instance == null) yield break;

        var classement = GameSessionManager.Instance.GetClassementFinal();

        // Même logique que ResultatsManager — du moins bon au meilleur
        var parScore = new List<(int playerID, float score, int sucettes)>(classement);
        parScore.Sort((a, b) => a.score.CompareTo(b.score));

        // Spawns dans le même ordre que ResultatsManager
        List<Transform> spawnsActifs = new List<Transform>();
        int nb = parScore.Count;
        if (nb >= 4 && spawn4e != null) spawnsActifs.Add(spawn1er);
        if (nb >= 3 && spawn3e != null) spawnsActifs.Add(spawn2e);
        if (nb >= 2 && spawn2e != null) spawnsActifs.Add(spawn3e);
        if (spawn1er != null) spawnsActifs.Add(spawn4e);

        TextMeshProUGUI[] txtMedailles =
        {
        txtMedaille4e,
        txtMedaille3e,
        txtMedaille2e,
        txtMedaille1er
    };

        for (int i = 0; i < parScore.Count; i++)
        {
            if (i >= spawnsActifs.Count) continue;

            var (playerID, score, sucettes) = parScore[i];
            Transform pos = spawnsActifs[i];

            GameObject go = Instantiate(
                podiumVisuelPrefab,
                pos.position,
                Quaternion.identity);
            go.transform.SetParent(pos, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.zero;

            PlayerData data = GameData.GetJoueur(playerID);
            PodiumVisuel pv = go.GetComponent<PodiumVisuel>();
            if (pv != null && data != null) pv.AppliquerData(data);

            yield return StartCoroutine(
                ScaleUpBounce(go.transform,
                              Vector3.one * echellePodium, 0.4f));

            // Médailles
            if (i < txtMedailles.Length && txtMedailles[i] != null)
            {
                string nomJ = L("classement_nom", playerID.ToString());
                txtMedailles[i].text = $"{nomJ}  🏅 x{sucettes}";

                int rang = parScore.Count - 1 - i;
                txtMedailles[i].color = rang == 0 ? couleurOr
                                      : rang == 1 ? couleurArgent
                                      : rang == 2 ? couleurBronze
                                      : couleurCraie;
            }

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.3f);
        AfficherGagnant(classement);
    }

    void AfficherGagnant(
        List<(int playerID, float score, int sucettes)> classement)
    {
        if (txtGagnant == null) return;

        var (playerID, score, sucettes) = classement[0];
        PlayerData data = GameData.GetJoueur(playerID);

        string nomJ = L("classement_nom", playerID.ToString());
        string hex = "FFFFFF";
        if (data != null)
            hex = ColorUtility.ToHtmlStringRGB(
                data.GetCouleurDossard());

        int nbManches = GameSessionManager.Instance != null
            ? GameSessionManager.Instance.session.nombreManches
            : 1;

        txtGagnant.text =
            $"<color=#{hex}>{nomJ}</color> " +
            $"🏅 x{sucettes} / {nbManches}";
    }

    IEnumerator ScaleUpBounce(Transform t, Vector3 cible, float duree)
    {
        float timer = 0f;
        while (timer < duree)
        {
            float p = timer / duree;
            float s = p < 0.7f
                ? Mathf.Lerp(0f, cible.x * 1.2f, p / 0.7f)
                : Mathf.Lerp(cible.x * 1.2f, cible.x,
                             (p - 0.7f) / 0.3f);
            t.localScale = new Vector3(s, s, 1f);
            timer += Time.deltaTime;
            yield return null;
        }
        t.localScale = cible;
    }

    void Update()
    {
        if (!estVisible || inputBloque) return;

        var kb = Keyboard.current;
        var gp0 = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

        bool gauche = false, droite = false;
        bool valider = false;

        if (kb != null)
        {
            gauche = kb.leftArrowKey.wasPressedThisFrame
                   || kb.qKey.wasPressedThisFrame;
            droite = kb.rightArrowKey.wasPressedThisFrame
                   || kb.dKey.wasPressedThisFrame;
            valider = kb.enterKey.wasPressedThisFrame
                   || kb.spaceKey.wasPressedThisFrame;
        }

        if (gp0 != null)
        {
            gauche = gauche || gp0.dpad.left.wasPressedThisFrame
                              || gp0.leftStick.left.wasPressedThisFrame;
            droite = droite || gp0.dpad.right.wasPressedThisFrame
                              || gp0.leftStick.right.wasPressedThisFrame;
            valider = valider || gp0.buttonSouth.wasPressedThisFrame;
        }

        if (gauche) Naviguer(-1);
        if (droite) Naviguer(1);
        if (valider) Valider();
    }

    void Naviguer(int dir)
    {
        navIndex = (navIndex + dir + 2) % 2;
        SurlígnerNav();
        JouerSFX(sfxFocus);
    }

    void Valider()
    {
        if (navIndex == 0) RetourMenu();
        else Quitter();
    }

    void SurlígnerNav()
    {
        styleBtnMenu?.SetSelectionne(navIndex == 0);
        styleBtnQuitter?.SetSelectionne(navIndex == 1);
    }

    void RetourMenu()
    {
        JouerSFX(sfxValider);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scene_Menu");
    }

    void Quitter()
    {
        JouerSFX(sfxValider);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void JouerSFX(AudioClip clip)
    {
        if (sourceAudio != null && clip != null)
            sourceAudio.PlayOneShot(clip);
    }
}
