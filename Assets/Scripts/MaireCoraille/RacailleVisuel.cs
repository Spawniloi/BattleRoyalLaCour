using UnityEngine;

public class RacailleVisuel : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer srCorps;
    public SpriteRenderer srRole;

    // ── Applique les données joueur ───────────────────────────────────────────
    public void AppliquerData(PlayerData data)
    {
        if (srCorps == null) return;

        // Génère le sprite selon forme + couleur
        srCorps.sprite = SpriteFactory.Creer(data.forme, data.GetCouleur());
        srCorps.color = Color.white; // couleur déjà dans le sprite

        Debug.Log($"[RacailleVisuel] J{data.playerID} → {data.forme} {data.couleur}");
    }

    // ── Visuel rôle requin/poisson ────────────────────────────────────────────
    public void SetRoleVisuel(bool isMayor)
    {
        if (srRole == null) return;

        srRole.enabled = true;

        if (isMayor)
        {
            // Aileron = triangle blanc au dessus
            srRole.sprite = SpriteFactory.Creer("triangle", Color.white);
            srRole.transform.localPosition = new Vector3(0, 0.6f, 0);
            srRole.transform.localScale = new Vector3(0.4f, 0.5f, 1f);
        }
        else
        {
            // Queue = losange derrière
            srRole.sprite = SpriteFactory.Creer("losange", Color.white);
            srRole.transform.localPosition = new Vector3(0, -0.6f, 0);
            srRole.transform.localScale = new Vector3(0.5f, 0.4f, 1f);
        }
    }
}