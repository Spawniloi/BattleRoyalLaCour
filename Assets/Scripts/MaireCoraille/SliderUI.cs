using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderUI : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;
    public int playerID = 1;

    [Header("UI")]
    public Slider slider;
    public Image handleImage;
    public Image fillImage;
    public Sprite spriteAileron;
    public Sprite spriteQueue;
    public TextMeshProUGUI labelJoueur;
    public TextMeshProUGUI labelValeur;

    [Header("Munitions Dash UI")]
    public Transform conteneurIcones;
    public Sprite spriteIconeDash;

    private int munitionsAffichees = 0;
    private System.Collections.Generic.List<GameObject> iconesActives
        = new System.Collections.Generic.List<GameObject>();

    private RacailleController racaille;

    // ── Start ─────────────────────────────────────────────────────────────────
    void Start()
    {
        if (config == null)
            config = FindFirstObjectByType<MaireGameManager>()?.config;

        if (slider != null)
        {
            slider.minValue = -1f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.interactable = false;
            slider.direction = Slider.Direction.LeftToRight;
        }

        if (labelJoueur != null)
            labelJoueur.text = $"J{playerID}";

        StartCoroutine(ChercherRacaille());
    }

    // ── Cherche la racaille ───────────────────────────────────────────────────
    System.Collections.IEnumerator ChercherRacaille()
    {
        while (racaille == null)
        {
            foreach (var r in FindObjectsByType<RacailleController>(
                              FindObjectsSortMode.None))
            {
                if (r.playerID == playerID) { racaille = r; break; }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    // ── Update ────────────────────────────────────────────────────────────────
    void Update()
    {
        if (racaille == null || config == null) return;
        MettreAJour(racaille.sliderValue, racaille.isMayor);
        MettreAJourMunitions();
    }

    // ── Mise à jour slider ────────────────────────────────────────────────────
    void MettreAJour(float valeur, bool estMaire)
    {
        float valeurNorm = Mathf.Clamp(valeur / config.sliderValeurMax, -1f, 1f);

        if (slider != null)
            slider.value = valeurNorm;

        // Rouge = requin, Vert = poisson
        Color couleurRole = estMaire
            ? new Color(0.9f, 0.3f, 0.3f, 1f)
            : new Color(0.3f, 0.8f, 0.4f, 1f);

        if (fillImage != null)
            fillImage.color = couleurRole;

        if (handleImage != null)
        {
            if (spriteAileron != null && spriteQueue != null)
                handleImage.sprite = estMaire ? spriteAileron : spriteQueue;
            else
                handleImage.color = couleurRole;
        }

        if (labelValeur != null)
            labelValeur.text = valeur.ToString("F1");
    }

    // ── Couleur joueur ────────────────────────────────────────────────────────
    public void SetCouleurJoueur(Color couleur)
    {
        if (fillImage != null) fillImage.color = couleur;
    }

    // ── Munitions dash ────────────────────────────────────────────────────────
    void MettreAJourMunitions()
    {
        int munitions = racaille.isMayor ? racaille.munitionsDash : 0;
        if (munitions == munitionsAffichees) return;

        munitionsAffichees = munitions;

        foreach (var ico in iconesActives)
            Destroy(ico);
        iconesActives.Clear();

        if (conteneurIcones != null)
            conteneurIcones.gameObject.SetActive(racaille.isMayor);

        for (int i = 0; i < munitions; i++)
        {
            if (conteneurIcones == null) break;

            GameObject ico = new GameObject($"IconeDash_{i}");
            ico.transform.SetParent(conteneurIcones);
            ico.transform.localScale = Vector3.one;
            ico.transform.localPosition = new Vector3(
                i * config.iconesDashEspacement, 0f, 0f);

            var img = ico.AddComponent<UnityEngine.UI.Image>();
            img.sprite = spriteIconeDash != null
                ? spriteIconeDash
                : SpriteFactory.Creer("etoile", new Color(1f, 0.85f, 0.1f));
            img.color = Color.white;

            var rt = ico.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(
                config.iconesDashTaille,
                config.iconesDashTaille);

            iconesActives.Add(ico);
        }
    }
}