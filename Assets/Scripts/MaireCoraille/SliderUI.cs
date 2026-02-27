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
    public Sprite spriteAileron; // maire
    public Sprite spriteQueue;   // fugitif
    public TextMeshProUGUI labelJoueur;
    public TextMeshProUGUI labelValeur;
    public Image fillGauche;    // côté négatif (maire = rouge)
    public Image fillDroit;     // côté positif (fugitif = vert)

    [Header("Munitions Dash UI")]
    public Transform conteneurIcones;  // parent des icones
    public Sprite spriteIconeDash;

    private int munitionsAffichees = 0;
    private System.Collections.Generic.List<GameObject> iconesActives
        = new System.Collections.Generic.List<GameObject>();

    private RacailleController racaille;

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

    void Update()
    {
        if (racaille == null || config == null) return;
        MettreAJour(racaille.sliderValue, racaille.isMayor);
        MettreAJourMunitions();
    }

    void MettreAJour(float valeur, bool estMaire)
    {
        float valeurMax = config.sliderValeurMax;

        // Maire = négatif, fugitif = positif
        // On inverse : sliderValue monte quand fugitif, descend quand maire
        float valeurNorm = Mathf.Clamp(valeur / valeurMax, -1f, 1f);

        if (slider != null)
            slider.value = valeurNorm;

        // Handle : aileron si maire, queue si fugitif
        if (handleImage != null)
            handleImage.sprite = estMaire ? spriteAileron : spriteQueue;

        // Couleur fill gauche (zone négative = maire = mauvais = rouge)
        if (fillGauche != null)
            fillGauche.color = valeurNorm < 0
                ? new Color(0.9f, 0.3f, 0.3f, 1f)
                : new Color(0.5f, 0.5f, 0.5f, 0.3f);

        // Couleur fill droit (zone positive = fugitif = bon = vert)
        if (fillDroit != null)
            fillDroit.color = valeurNorm > 0
                ? new Color(0.3f, 0.8f, 0.4f, 1f)
                : new Color(0.5f, 0.5f, 0.5f, 0.3f);

        // Score affiché
        if (labelValeur != null)
            labelValeur.text = valeur.ToString("F1");
    }

    public void SetCouleurJoueur(Color couleur)
    {
        if (fillDroit != null) fillDroit.color = couleur;
    }

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

            // Génère étoile jaune si pas de sprite assigné
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