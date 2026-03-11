using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attache ce script sur chaque bouton
public class BoutonStylee : MonoBehaviour
{
    [Header("Visuel")]
    public Image imgFond;
    public Image imgContour;
    public TextMeshProUGUI txtLabel;

    [Header("Couleurs normal")]
    public Color couleurFondNormal = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color couleurContourNormal = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color couleurTexteNormal = Color.white;

    [Header("Couleurs selectionne")]
    public Color couleurFondSelect = new Color(0.15f, 0.15f, 0.15f, 0.95f);
    public Color couleurContourSelect = new Color(1f, 0.84f, 0f, 1f);
    public Color couleurTexteSelect = new Color(1f, 0.84f, 0f, 1f);

    [Header("Echelle")]
    public float echelleNormal = 1f;
    public float echelleSelect = 1.08f;
    public float vitesseAnimation = 8f;

    [Header("Contour")]
    public float epaisseurNormal = 2f;
    public float epaisseurSelect = 4f;

    private bool selectionne = false;
    private Vector3 cibleScale;

    void Awake()
    {
        cibleScale = Vector3.one * echelleNormal;
        AppliquerStyle(false, true);
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            cibleScale,
            Time.deltaTime * vitesseAnimation);
    }

    public void SetSelectionne(bool actif)
    {
        selectionne = actif;
        cibleScale = Vector3.one * (actif ? echelleSelect : echelleNormal);
        AppliquerStyle(actif, false);
    }

    void AppliquerStyle(bool actif, bool instantane)
    {
        if (imgFond != null)
            imgFond.color = actif
                ? couleurFondSelect : couleurFondNormal;

        if (imgContour != null)
        {
            imgContour.color = actif
                ? couleurContourSelect : couleurContourNormal;

            // Epaisseur via Outline component
            Outline outline = imgContour.GetComponent<Outline>();
            if (outline != null)
                outline.effectDistance = actif
                    ? new Vector2(epaisseurSelect, epaisseurSelect)
                    : new Vector2(epaisseurNormal, epaisseurNormal);
        }

        if (txtLabel != null)
            txtLabel.color = actif
                ? couleurTexteSelect : couleurTexteNormal;

        if (instantane)
            transform.localScale = Vector3.one *
                (actif ? echelleSelect : echelleNormal);
    }
}