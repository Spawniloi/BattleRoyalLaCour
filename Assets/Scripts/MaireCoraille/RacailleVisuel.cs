using UnityEngine;

public class RacailleVisuel : MonoBehaviour
{
    [Header("Sprites formes")]
    public Sprite spriteCercle;
    public Sprite spriteCarre;
    public Sprite spriteTriangle;
    public Sprite spriteEtoile;
    public Sprite spriteLosange;

    [Header("Sprites role")]
    public Sprite spriteAileron;  // maire
    public Sprite spriteQueue;    // fugitif

    [Header("References")]
    public SpriteRenderer srCorps;    // sprite principal
    public SpriteRenderer srRole;     // aileron ou queue
    public SpriteRenderer srObjet;    // accessoire

    // ── Applique les données du joueur ────────────────────────────────────────
    public void AppliquerData(PlayerData data)
    {
        // Couleur
        if (srCorps != null)
        {
            srCorps.color = data.GetCouleur();
            srCorps.sprite = GetSpriteForme(data.forme);
        }

        // Accessoire (TODO quand les sprites seront prêts)
        // AppliquerObjet(data.objet);

        Debug.Log($"[RacailleVisuel] J{data.playerID} " +
                  $"→ {data.forme} {data.couleur}");
    }

    // ── Role visuel — requin ou poisson ───────────────────────────────────────
    public void SetRoleVisuel(bool isMayor)
    {
        if (srRole == null) return;

        srRole.enabled = true;

        if (isMayor)
        {
            srRole.sprite = spriteAileron;
            srRole.color = Color.white;
        }
        else
        {
            srRole.sprite = spriteQueue;
            srRole.color = Color.white;
        }
    }

    // ── Helper forme → sprite ─────────────────────────────────────────────────
    Sprite GetSpriteForme(string forme)
    {
        return forme switch
        {
            "cercle" => spriteCercle,
            "carre" => spriteCarre,
            "triangle" => spriteTriangle,
            "etoile" => spriteEtoile,
            "losange" => spriteLosange,
            _ => spriteCercle,
        };
    }
}