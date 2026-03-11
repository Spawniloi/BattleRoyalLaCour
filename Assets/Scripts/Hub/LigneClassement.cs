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

    // ── Raccourci localisation ────────────────────────────────────────────────
    string L(string cle, params string[] args)
        => LocalisationManager.Instance != null
            ? LocalisationManager.Instance.Get(cle, args)
            : cle;

    public void Appliquer(
        int rang, int playerID, float score, int sucettes)
    {
        PlayerData data = GameData.GetJoueur(playerID);

        // ── Rang traduit ──────────────────────────────────────────────────────
        if (txtRang != null)
            txtRang.text = rang switch
            {
                0 => L("classement_rang_0"),
                1 => L("classement_rang_1"),
                2 => L("classement_rang_2"),
                3 => L("classement_rang_3"),
                _ => L("classement_rang_4"),
            };

        // ── Nom coloré traduit ────────────────────────────────────────────────
        if (txtNom != null && data != null)
        {
            string hex = ColorUtility.ToHtmlStringRGB(
                data.GetCouleurDossard());
            string nom = L("classement_nom", playerID.ToString());
            txtNom.text = $"<color=#{hex}>{nom}</color>";
        }

        // ── Sucettes traduites ────────────────────────────────────────────────
        if (txtSucettes != null)
            txtSucettes.text = sucettes > 0
                ? L("classement_sucettes", sucettes.ToString())
                : L("classement_sucettes_zero");

        // ── Fond couleur rang ─────────────────────────────────────────────────
        if (imgFond != null)
            imgFond.color = rang switch
            {
                0 => couleurZero,
                1 => couleur1er,
                2 => couleur2e,
                3 => couleur3e,
                _ => couleurAutre,
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