using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TexteLocalise : MonoBehaviour
{
    [Header("Clé de traduction")]
    public string cle;

    [Header("Paramètres dynamiques")]
    [Tooltip("Laisser vide si pas de {0} {1} dans le texte")]
    public string[] parametres;

    private TextMeshProUGUI tmp;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        if (LocalisationManager.Instance != null &&
            LocalisationManager.Instance.EstCharge())
        {
            AppliquerTraduction();
        }
        else if (LocalisationManager.Instance != null)
        {
            // Attend que les traductions soient chargées
            LocalisationManager.Instance.onTraductionsChargees
                += AppliquerTraduction;
        }
    }

    void OnDestroy()
    {
        if (LocalisationManager.Instance != null)
            LocalisationManager.Instance.onTraductionsChargees
                -= AppliquerTraduction;
    }

    public void AppliquerTraduction()
    {
        if (tmp == null) tmp = GetComponent<TextMeshProUGUI>();
        if (tmp == null || string.IsNullOrEmpty(cle)) return;

        tmp.text = LocalisationManager.Instance != null
            ? LocalisationManager.Instance.Get(cle, parametres)
            : cle;
    }

    // Pour changer les paramètres dynamiquement par code
    public void SetParametres(params string[] args)
    {
        parametres = args;
        AppliquerTraduction();
    }
}
