using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LigneClassement : MonoBehaviour
{
    [Header("UI")]
    public Image imgFond;        // fond couleur rang
    public TextMeshProUGUI txtRang;        // "1er" "2e" etc.
    public TextMeshProUGUI txtNom;         // "J1"
    public TextMeshProUGUI txtSucettes;    // "🏅 x2"
    public Image imgPodiumVisuel;

    [Header("Couleurs rang")]
    public Color couleur1er = new Color(1f, 0.84f, 0f, 0.3f);
    public Color couleur2e = new Color(0.75f, 0.75f, 0.75f, 0.3f);
    public Color couleur3e = new Color(0.80f, 0.50f, 0.20f, 0.3f);
    public Color couleurAutre = new Color(0.5f, 0.5f, 0.5f, 0.2f);

    public void Appliquer(
        int rang,
        int playerID,
        float score,
        int sucettes)
    {
        PlayerData data = GameData.GetJoueur(playerID);

        // Rang
        if (txtRang != null)
            txtRang.text = rang switch
            {
                1 => "1er",
                2 => "2e",
                3 => "3e",
                _ => "4e"
            };

        // Nom avec couleur dossard
        if (txtNom != null && data != null)
        {
            string hex = ColorUtility.ToHtmlStringRGB(
                data.GetCouleurDossard());
            txtNom.text = $"<color=#{hex}>J{playerID}</color>";
        }

        // Sucettes
        if (txtSucettes != null)
            txtSucettes.text = sucettes > 0
                ? $"x{sucettes}" : "-";

        // Fond couleur rang
        if (imgFond != null)
            imgFond.color = rang switch
            {
                1 => couleur1er,
                2 => couleur2e,
                3 => couleur3e,
                _ => couleurAutre
            };

        // Prefab PodiumVisuel
        PodiumVisuel pv = GetComponentInChildren<PodiumVisuel>();
        if (pv != null && data != null)
            pv.AppliquerData(data);
    }
}
