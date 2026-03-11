using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultatsManager : MonoBehaviour
{
    [Header("Ardoise fond")]
    public Image ardoise;
    public Sprite ardoiseMaireCoraille;
    public Sprite ardoiseJeu2;
    public Sprite ardoiseJeu3;

    [Header("Tailles texte")]
    public float tailleEntete = 28f;
    public float tailleJoueurs = 26f;
    public float tailleSucette = 26f;

    [Header("Positions colonnes X")]
    public float colLabel = 0f;
    public float colJ1 = 200f;
    public float colJ2 = 310f;
    public float colJ3 = 420f;
    public float colJ4 = 530f;

    [Header("Largeur colonnes")]
    public float largeurLabel = 180f;
    public float largeurValeur = 100f;

    [Header("Espacement lignes")]
    public float hauteurLigne = 40f;
    public float delaiEntreLignes = 0.05f;

    [Header("Tableau")]
    public RectTransform conteneurTableau;
    public TMP_FontAsset fonteCraie;

    [Header("Podium — points de spawn")]
    public Transform spawn1er;
    public Transform spawn2e;
    public Transform spawn3e;
    public Transform spawn4e;
    public GameObject podiumVisuelPrefab;

    [Header("Podium — taille prefabs")]
    public float echellePodium = 2f;

    [Header("Sucette or")]
    public GameObject prefabSucetteOr;
    public Transform conteneurSucette;

    [Header("Boutons")]
    public Button btnContinuer;
    public Button btnMenu;
    public Button btnQuitter;

    [Header("Textes boutons")]
    public TextMeshProUGUI txtBtnContinuer;
    public TextMeshProUGUI txtBtnMenu;
    public TextMeshProUGUI txtBtnQuitter;

    [Header("Audio")]
    public AudioSource sourceAudio;
    public AudioClip sfxCraie;

    [Header("Reglages ecriture")]
    public float vitesseEcriture = 0.03f;

    [Header("Couleurs")]
    public Color couleurCraie = new Color(0.95f, 0.95f, 0.90f, 1f);
    public Color couleurOr = new Color(1f, 0.84f, 0f, 1f);
    public Color couleurArgent = new Color(0.75f, 0.75f, 0.75f, 1f);
    public Color couleurBronze = new Color(0.80f, 0.50f, 0.20f, 1f);

    private PartieData partie;
    private List<JoueurResultat> classes;
    private int ligneActuelle = 0;
    private bool dejàEnvoye = false;

    // ── Raccourci localisation ────────────────────────────────────────────────
    string L(string cle, params string[] args)
        => LocalisationManager.Instance != null
            ? LocalisationManager.Instance.Get(cle, args)
            : cle;

    void Start()
    {
        // ── Ardoise selon jeu ─────────────────────────────────────────────────
        if (ardoise != null)
        {
            switch (GameData.jeuActuel)
            {
                case "MaireCoraille":
                    if (ardoiseMaireCoraille != null)
                        ardoise.sprite = ardoiseMaireCoraille;
                    break;
                case "Jeu2":
                    if (ardoiseJeu2 != null)
                        ardoise.sprite = ardoiseJeu2;
                    break;
                case "Jeu3":
                    if (ardoiseJeu3 != null)
                        ardoise.sprite = ardoiseJeu3;
                    break;
            }
        }

        // ── Textes boutons traduits ───────────────────────────────────────────
        if (txtBtnContinuer != null)
            txtBtnContinuer.text = L("resultats_btn_continuer");
        if (txtBtnMenu != null)
            txtBtnMenu.text = L("resultats_btn_menu");
        if (txtBtnQuitter != null)
            txtBtnQuitter.text = L("resultats_btn_quitter");

        // ── Abonne aux changements de langue ──────────────────────────────────
        if (LocalisationManager.Instance != null)
            LocalisationManager.Instance.onTraductionsChargees
                += MettreAJourTextesBoutons;

        partie = GameData.dernierePartie;
        if (partie == null) partie = GenererMock();

        classes = partie.joueurs
            .OrderBy(j => j.playerID)
            .ToList();

        btnContinuer?.onClick.AddListener(Continuer);
        btnMenu?.onClick.AddListener(RetourMenu);
        btnQuitter?.onClick.AddListener(Quitter);

        StartCoroutine(AfficherResultats());
    }

    void OnDestroy()
    {
        if (LocalisationManager.Instance != null)
            LocalisationManager.Instance.onTraductionsChargees
                -= MettreAJourTextesBoutons;
    }

    void MettreAJourTextesBoutons()
    {
        if (txtBtnContinuer != null)
            txtBtnContinuer.text = L("resultats_btn_continuer");
        if (txtBtnMenu != null)
            txtBtnMenu.text = L("resultats_btn_menu");
        if (txtBtnQuitter != null)
            txtBtnQuitter.text = L("resultats_btn_quitter");
    }

    // ── Séquence principale ───────────────────────────────────────────────────
    IEnumerator AfficherResultats()
    {
        yield return StartCoroutine(EcrireTableau());
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(AnimerPodium());
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(AnimerSucette());
    }

    // ── Tableau ───────────────────────────────────────────────────────────────
    IEnumerator EcrireTableau()
    {
        ligneActuelle = 0;

        float[] posX = { colLabel, colJ1, colJ2, colJ3, colJ4 };

        // ── En-tête joueurs colorés ───────────────────────────────────────────
        for (int j = 0; j < classes.Count; j++)
        {
            PlayerData data = GameData.GetJoueur(classes[j].playerID);
            Color coul = data != null
                ? data.GetCouleurDossard() : couleurCraie;

            TextMeshProUGUI tmp = CreerCell(
                coul, tailleEntete, posX[j + 1]);
            tmp.text = L("classement_nom",
                         classes[j].playerID.ToString());
        }

        ligneActuelle++;
        yield return new WaitForSeconds(delaiEntreLignes);

        // ── Lignes stats traduites ────────────────────────────────────────────
        string[] labels =
        {
            L("resultats_stat_splash"),
            L("resultats_stat_cache"),
            L("resultats_stat_hop"),
            L("resultats_stat_ensemble"),
            L("resultats_stat_mechant"),
            L("resultats_stat_points"),
        };

        for (int statIndex = 0; statIndex < 6; statIndex++)
        {
            List<TextMeshProUGUI> tmps = new List<TextMeshProUGUI>();
            List<string> textes = new List<string>();

            TextMeshProUGUI tmpLabel = CreerCell(
                couleurOr, tailleJoueurs, posX[0]);
            tmpLabel.text = "";
            tmps.Add(tmpLabel);
            textes.Add(labels[statIndex]);

            for (int j = 0; j < classes.Count; j++)
            {
                var jr = classes[j];
                string valeur = statIndex switch
                {
                    0 => $"{jr.stats.distanceParcourue:F0}u",
                    1 => $"{jr.stats.tempsPoissonMax:F1}s",
                    2 => $"{jr.stats.nbRebonds}",
                    3 => $"{jr.stats.nbPassagesCoraille}",
                    4 => $"{jr.stats.tempsMaire:F1}",
                    5 => $"{jr.score:F0}pts",
                    _ => ""
                };

                PlayerData data = GameData.GetJoueur(jr.playerID);
                Color coul = data != null
                    ? data.GetCouleurDossard() : couleurCraie;

                TextMeshProUGUI tmpVal = CreerCell(
                    coul, tailleJoueurs, posX[j + 1]);
                tmpVal.text = "";
                tmps.Add(tmpVal);
                textes.Add(valeur);
            }

            yield return StartCoroutine(EcrireParallele(tmps, textes));
            ligneActuelle++;
            yield return new WaitForSeconds(delaiEntreLignes);
        }
    }

    // ── Cellule à position absolue ────────────────────────────────────────────
    TextMeshProUGUI CreerCell(Color couleur, float taille, float posX)
    {
        GameObject go = new GameObject("Cell");
        go.transform.SetParent(conteneurTableau, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(
            posX, -ligneActuelle * hauteurLigne);
        rt.sizeDelta = new Vector2(
            posX == colLabel ? largeurLabel : largeurValeur,
            hauteurLigne);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (fonteCraie != null) tmp.font = fonteCraie;
        tmp.color = couleur;
        tmp.fontSize = taille;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.enableWordWrapping = false;

        return tmp;
    }

    // ── Écriture parallèle lettre par lettre ──────────────────────────────────
    IEnumerator EcrireParallele(
        List<TextMeshProUGUI> tmps,
        List<string> textes)
    {
        int maxLen = textes.Max(t => t.Length);

        for (int i = 0; i < maxLen; i++)
        {
            JouerSonCraie();
            for (int j = 0; j < tmps.Count; j++)
                if (i < textes[j].Length)
                    tmps[j].text += textes[j][i];
            yield return new WaitForSeconds(vitesseEcriture);
        }
    }

    void JouerSonCraie()
    {
        if (sourceAudio == null || sfxCraie == null) return;
        sourceAudio.pitch = Random.Range(0.9f, 1.1f);
        sourceAudio.PlayOneShot(sfxCraie, 0.3f);
    }

    // ── Podium ────────────────────────────────────────────────────────────────
    IEnumerator AnimerPodium()
    {
        List<JoueurResultat> parScore = partie.joueurs
            .OrderBy(j => j.score)
            .ThenBy(j => j.stats.tempsMaire)
            .ToList();

        List<Transform> spawnsActifs = new List<Transform>();
        int nb = partie.nbJoueurs;
        if (nb >= 4 && spawn4e != null) spawnsActifs.Add(spawn1er);
        if (nb >= 3 && spawn3e != null) spawnsActifs.Add(spawn2e);
        if (nb >= 2 && spawn2e != null) spawnsActifs.Add(spawn3e);
        if (spawn1er != null) spawnsActifs.Add(spawn4e);

        for (int i = 0; i < parScore.Count; i++)
        {
            if (i >= spawnsActifs.Count) continue;

            JoueurResultat jr = parScore[i];
            Transform pos = spawnsActifs[i];

            GameObject go = Instantiate(
                podiumVisuelPrefab, pos.position, Quaternion.identity);
            go.transform.SetParent(pos, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.zero;

            PlayerData data = GameData.GetJoueur(jr.playerID);
            PodiumVisuel pv = go.GetComponent<PodiumVisuel>();
            if (pv != null && data != null) pv.AppliquerData(data);

            yield return StartCoroutine(
                ScaleUpBounce(go.transform,
                              Vector3.one * echellePodium, 0.4f));
            yield return new WaitForSeconds(0.2f);
        }
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

    // ── Sucette or ────────────────────────────────────────────────────────────
    IEnumerator AnimerSucette()
    {
        if (conteneurSucette == null) yield break;

        JoueurResultat gagnant = partie.joueurs
            .OrderByDescending(j => j.score)
            .ThenByDescending(j => j.stats.tempsMaire)
            .First();

        PlayerData dataGagnant = GameData.GetJoueur(gagnant.playerID);
        Color coulGagnant = dataGagnant != null
            ? dataGagnant.GetCouleurDossard() : couleurOr;
        string hex = ColorUtility.ToHtmlStringRGB(coulGagnant);

        GameObject go = new GameObject("TexteSucette");
        go.transform.SetParent(conteneurSucette, false);
        go.transform.localPosition = Vector3.zero;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (fonteCraie != null) tmp.font = fonteCraie;
        tmp.color = couleurOr;
        tmp.fontSize = tailleSucette;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = "";

        // Nom sans couleur pour l'écriture lettre par lettre
        string nomBrut = L("classement_nom", gagnant.playerID.ToString());
        string texteBrut = L("resultats_sucette", nomBrut);

        foreach (char c in texteBrut)
        {
            tmp.text += c;
            JouerSonCraie();
            yield return new WaitForSeconds(vitesseEcriture * 0.5f);
        }

        // Remet le nom en couleur
        string nomColore = $"<color=#{hex}>{nomBrut}</color>";
        tmp.text = texteBrut.Replace(nomBrut, nomColore);

        // ── Prefab sucette ────────────────────────────────────────────────────
        if (prefabSucetteOr != null)
        {
            GameObject sucette = Instantiate(
                prefabSucetteOr,
                conteneurSucette.position,
                Quaternion.identity);
            sucette.transform.SetParent(conteneurSucette, false);
            sucette.transform.localPosition = new Vector3(200f, 0f, 0f);
            sucette.transform.localScale = Vector3.zero;

            float timer = 0f;
            while (timer < 0.8f)
            {
                float p = timer / 0.8f;
                float s = Mathf.Lerp(0f, 1f,
                              Mathf.Sin(p * Mathf.PI * 0.5f));
                float rot = Mathf.Lerp(-540f, 0f, p);
                sucette.transform.localScale = new Vector3(s, s, 1f);
                sucette.transform.localRotation =
                    Quaternion.Euler(0, 0, rot);
                timer += Time.deltaTime;
                yield return null;
            }
            sucette.transform.localScale = Vector3.one;
            sucette.transform.localRotation = Quaternion.identity;
        }

        GameData.sucettesOr++;
    }

    // ── Boutons ───────────────────────────────────────────────────────────────
    void Continuer()
    {
        EnvoyerDonnees();

        if (GameSessionManager.Instance == null)
        {
            SceneManager.LoadScene("Scene_ChoixJeu");
            return;
        }

        var gsm = GameSessionManager.Instance;

        if (gsm.MancheTerminee())
        {
            if (gsm.SessionTerminee())
                SceneManager.LoadScene("Scene_ClassementFinal");
            else
            {
                gsm.MancheSuivante();
                SceneManager.LoadScene("Scene_ChoixJeu");
            }
        }
        else
            SceneManager.LoadScene("Scene_ChoixJeu");
    }

    void RetourMenu()
    {
        EnvoyerDonnees();
        SceneManager.LoadScene("Scene_Menu");
    }

    void Quitter()
    {
        EnvoyerDonnees();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void EnvoyerDonnees()
    {
        if (dejàEnvoye) return;
        dejàEnvoye = true;
        GoogleSheetsExporter exporter =
            FindFirstObjectByType<GoogleSheetsExporter>();
        exporter?.Exporter(partie);
    }

    // ── Mock ──────────────────────────────────────────────────────────────────
    PartieData GenererMock()
    {
        PartieData p = new PartieData();
        p.partieId = "mock";
        p.jeuActuel = "MaireCoraille";
        p.nbJoueurs = 4;
        p.dureePartie = 120f;

        p.joueurs.Add(new JoueurResultat
        {
            playerID = 1,
            score = 85f,
            stats = new StatsJoueur
            {
                distanceParcourue = 230f,
                tempsPoissonMax = 18f,
                nbRebonds = 5,
                nbPassagesCoraille = 3,
                tempsMaire = 40f
            },
            titres = new List<string> { "Nageur" }
        });
        p.joueurs.Add(new JoueurResultat
        {
            playerID = 2,
            score = 60f,
            stats = new StatsJoueur
            {
                distanceParcourue = 180f,
                tempsPoissonMax = 12f,
                nbRebonds = 9,
                nbPassagesCoraille = 1,
                tempsMaire = 80f
            },
            titres = new List<string> { "Collectif" }
        });
        p.joueurs.Add(new JoueurResultat
        {
            playerID = 3,
            score = 72f,
            stats = new StatsJoueur
            {
                distanceParcourue = 210f,
                tempsPoissonMax = 8f,
                nbRebonds = 3,
                nbPassagesCoraille = 4,
                tempsMaire = 55f
            },
            titres = new List<string> { "Hop !" }
        });
        p.joueurs.Add(new JoueurResultat
        {
            playerID = 4,
            score = 45f,
            stats = new StatsJoueur
            {
                distanceParcourue = 190f,
                tempsPoissonMax = 15f,
                nbRebonds = 7,
                nbPassagesCoraille = 2,
                tempsMaire = 30f
            },
            titres = new List<string> { "Mechant !" }
        });

        p.gagnant = 1;
        return p;
    }
}