using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LigneClassement : MonoBehaviour
{
    [Header("UI")]
    public Image imgFond;
    public TextMeshProUGUI txtRang;
    public TextMeshProUGUI txtNom;
    public TextMeshProUGUI txtSucettes;

    [Header("Couleurs rang")]
    public Color couleur1er = new Color(1f, 0.84f, 0f, 0.3f);
    public Color couleur2e = new Color(0.75f, 0.75f, 0.75f, 0.3f);
    public Color couleur3e = new Color(0.80f, 0.50f, 0.20f, 0.3f);
    public Color couleurAutre = new Color(0.5f, 0.5f, 0.5f, 0.2f);
    public Color couleurZero = new Color(0.3f, 0.3f, 0.3f, 0.15f);

    public void Appliquer(
        int rang,
        int playerID,
        float score,
        int sucettes)
    {
        PlayerData data = GameData.GetJoueur(playerID);

        // ── Rang ──────────────────────────────────────────────────────────────
        if (txtRang != null)
            txtRang.text = rang switch
            {
                0 => "-",
                1 => "1er",
                2 => "2e",
                3 => "3e",
                _ => "4e"
            };

        // ── Nom coloré ────────────────────────────────────────────────────────
        if (txtNom != null && data != null)
        {
            string hex = ColorUtility.ToHtmlStringRGB(
                data.GetCouleurDossard());
            txtNom.text = $"<color=#{hex}>J{playerID}</color>";
        }

        // ── Sucettes ──────────────────────────────────────────────────────────
        if (txtSucettes != null)
            txtSucettes.text = sucettes > 0
                ? $"x{sucettes}" : "0";

        // ── Fond couleur rang ─────────────────────────────────────────────────
        if (imgFond != null)
            imgFond.color = rang switch
            {
                0 => couleurZero,
                1 => couleur1er,
                2 => couleur2e,
                3 => couleur3e,
                _ => couleurAutre
            };

        // ── PodiumVisuel ──────────────────────────────────────────────────────
        PodiumVisuel pv = GetComponentInChildren<PodiumVisuel>();
        if (pv != null && data != null)
        {
            pv.AppliquerData(data);
            Debug.Log($"[Classement] J{playerID} " +
                      $"couleur:{data.couleurDossard}");
        }
        else
        {
            Debug.LogWarning(
                $"[Classement] PodiumVisuel null pour J{playerID} !");
        }
    }
}