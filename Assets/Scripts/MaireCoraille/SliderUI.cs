using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderUI : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;
    public int playerID = 1;

    [Header("UI — Slider unique centré")]
    public Slider slider;         // 1 seul slider, value -1 à 1
    public Image handleImage;    // sprite sur le handle
    public Sprite spriteAileron;  // maire (positif)
    public Sprite spriteQueue;    // fugitif (negatif)

    [Header("Labels")]
    public TextMeshProUGUI labelJoueur;
    public TextMeshProUGUI labelValeur;

    [Header("Couleurs")]
    public Image fillPositif; // image fill du côté positif
    public Image fillNegatif; // image fill du côté négatif

    private RacailleController racaille;

    void Start()
    {
        if (config == null)
            config = FindFirstObjectByType<MaireGameManager>()?.config;

        // Slider de -1 à 1, commence à 0
        if (slider != null)
        {
            slider.minValue = -1f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.interactable = false;
            slider.direction = Slider.Direction.BottomToTop;
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
        Debug.Log($"[SliderUI] J{playerID} racaille trouvée !");
    }

    void Update()
    {
        if (racaille == null || config == null) return;
        MettreAJour(racaille.sliderValue, racaille.isMayor);
    }

    void MettreAJour(float valeur, bool estMaire)
    {
        float valeurMax = config.sliderValeurMax;

        // Normalise entre -1 et 1
        float valeurNorm = Mathf.Clamp(valeur / valeurMax, -1f, 1f);

        // Bouge le handle
        if (slider != null)
            slider.value = valeurNorm;

        // Sprite handle selon rôle
        if (handleImage != null)
            handleImage.sprite = estMaire ? spriteAileron : spriteQueue;

        // Affiche le score
        if (labelValeur != null)
            labelValeur.text = valeur.ToString("F1");

        // Couleur fill selon positif/négatif
        if (fillPositif != null)
            fillPositif.color = valeurNorm > 0
                ? new Color(0.9f, 0.3f, 0.3f, 1f)   // rouge = mauvais
                : new Color(0.9f, 0.3f, 0.3f, 0.2f); // transparent

        if (fillNegatif != null)
            fillNegatif.color = valeurNorm < 0
                ? new Color(0.3f, 0.8f, 0.4f, 1f)   // vert = bon
                : new Color(0.3f, 0.8f, 0.4f, 0.2f); // transparent
    }

    public void SetCouleurJoueur(Color couleur)
    {
        if (fillPositif != null) fillPositif.color = couleur;
    }
}