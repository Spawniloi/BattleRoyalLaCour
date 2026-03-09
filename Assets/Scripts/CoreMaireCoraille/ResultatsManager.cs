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
    public float echellePodium = 2f; // ← ajuste dans Inspector

    [Header("Sucette or")]
    public GameObject prefabSucetteOr;
    public Transform conteneurSucette;

    [Header("Boutons")]
    public Button btnContinuer;
    public Button btnMenu;
    public Button btnQuitter;

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

    void Start()
    {
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
        partie = GameData.dernierePartie;
        if (partie == null) partie = GenererMock();

        // Tableau — trié par playerID
        classes = partie.joueurs
            .OrderBy(j => j.playerID)
            .ToList();

        btnContinuer?.onClick.AddListener(Continuer);
        btnMenu?.onClick.AddListener(RetourMenu);
        btnQuitter?.onClick.AddListener(Quitter);

        StartCoroutine(AfficherResultats());
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

        // ── En-tête Jx colorés ────────────────────────────────────────────────
        for (int j = 0; j < classes.Count; j++)
        {
            PlayerData data = GameData.GetJoueur(classes[j].playerID);
            Color coul = data != null
                ? data.GetCouleurDossard() : couleurCraie;

            TextMeshProUGUI tmp = CreerCell(
                coul, tailleEntete, posX[j + 1]);
            tmp.text = $"J{classes[j].playerID}";
        }

        ligneActuelle++;
        yield return new WaitForSeconds(delaiEntreLignes);

        // ── Lignes stats ──────────────────────────────────────────────────────
        string[] labels = {
            "Splash !",
            "Cache !",
            "Hop !",
            "Ensemble !",
            "Mechant !",
            "Points !"
        };

        for (int statIndex = 0; statIndex < 6; statIndex++)
        {
            List<TextMeshProUGUI> tmps = new List<TextMeshProUGUI>();
            List<string> textes = new List<string>();

            // Label
            TextMeshProUGUI tmpLabel = CreerCell(
                couleurOr, tailleJoueurs, posX[0]);
            tmpLabel.text = "";
            tmps.Add(tmpLabel);
            textes.Add(labels[statIndex]);

            // Valeurs joueurs
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
        // Tri par score — plus faible en premier
        List<JoueurResultat> parScore = partie.joueurs
            .OrderBy(j => j.score)
            .ThenBy(j => j.stats.tempsMaire)
            .ToList();

        Transform[] spawns = { spawn1er, spawn2e, spawn3e, spawn4e };

        for (int i = 0; i < parScore.Count; i++)
        {
            if (i >= spawns.Length || spawns[i] == null) continue;

            JoueurResultat jr = parScore[i];
            Transform pos = spawns[i];

            GameObject go = Instantiate(
                podiumVisuelPrefab,
                pos.position,
                Quaternion.identity);

            go.transform.SetParent(pos, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.zero;

            PlayerData data = GameData.GetJoueur(jr.playerID);
            PodiumVisuel pv = go.GetComponent<PodiumVisuel>();

            if (pv != null && data != null)
                pv.AppliquerData(data);

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

        // Gagnant = score le plus haut
        JoueurResultat gagnant = partie.joueurs
            .OrderByDescending(j => j.score)
            .ThenByDescending(j => j.stats.tempsMaire)
            .First();

        PlayerData dataGagnant = GameData.GetJoueur(gagnant.playerID);
        Color coulGagnant = dataGagnant != null
            ? dataGagnant.GetCouleurDossard() : couleurOr;
        string hex = ColorUtility.ToHtmlStringRGB(coulGagnant);

        // Texte
        GameObject go = new GameObject("TexteSucette");
        go.transform.SetParent(conteneurSucette, false);
        go.transform.localPosition = Vector3.zero;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (fonteCraie != null) tmp.font = fonteCraie;
        tmp.color = couleurOr;
        tmp.fontSize = tailleSucette;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = "";

        string texteBrut =
            $"J{gagnant.playerID} gagne une sucette en OR !";

        foreach (char c in texteBrut)
        {
            tmp.text += c;
            JouerSonCraie();
            yield return new WaitForSeconds(vitesseEcriture * 0.5f);
        }

        tmp.text =
            $"<color=#{hex}>J{gagnant.playerID}</color>" +
            " gagne une sucette en OR !";

        // Prefab sucette
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
            SceneManager.LoadScene("Scene_MaireCoraille");
            return;
        }

        var gsm = GameSessionManager.Instance;

        // Manche terminée ?
        if (gsm.MancheTerminee())
        {
            // Dernière manche ?
            if (gsm.SessionTerminee())
                SceneManager.LoadScene("Scene_ClassementFinal");
            else
            {
                gsm.MancheSuivante();
                gsm.LancerProchainJeu();
            }
        }
        else
        {
            // Encore des jeux dans cette manche
            gsm.LancerProchainJeu();
        }
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
        if (dejàEnvoye) return; // ← évite le double envoi
        dejàEnvoye = true;

        GoogleSheetsExporter exporter =
            FindFirstObjectByType<GoogleSheetsExporter>();
        exporter?.Exporter(partie);
    }

    // ── Mock pour tester ──────────────────────────────────────────────────────
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