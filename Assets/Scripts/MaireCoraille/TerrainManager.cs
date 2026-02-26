using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;

    [Header("Murs invisibles")]
    public Transform murHaut;
    public Transform murBas;
    public Transform murGauche;
    public Transform murDroit;

    [Header("Visuel bordures")]
    public LineRenderer ligneBordure;
    public Color couleurBordure = Color.white;
    public float epaisseurLigne = 0.08f;

    [Header("Sprites cadre terrain")]
    public SpriteRenderer cadreTerrain;  // le SpriteRenderer dans la scène
    public Sprite cadre2J;       // sprite pour 2 joueurs
    public Sprite cadre3J;       // sprite pour 3 joueurs
    public Sprite cadre4J;       // sprite pour 4 joueurs

    [Header("Offset terrain (compense bande UI en haut)")]
    public float offsetY = -0.36f;

    public Vector2 TailleTerrain { get; private set; }

    public void InitTerrain(int nbJoueurs)
    {
        Vector2 taille = config.GetTerrainSize(nbJoueurs);
        TailleTerrain = taille;

        float w = taille.x / 2f;
        float h = taille.y / 2f;

        // ── Murs invisibles ───────────────────────────────────────────────────
        SetMur(murHaut, new Vector3(0, h + offsetY, 0),
                          new Vector3(taille.x + 1f, 0.5f, 1f));
        SetMur(murBas, new Vector3(0, -h + offsetY, 0),
                          new Vector3(taille.x + 1f, 0.5f, 1f));
        SetMur(murGauche, new Vector3(-w, offsetY, 0),
                          new Vector3(0.5f, taille.y + 1f, 1f));
        SetMur(murDroit, new Vector3(w, offsetY, 0),
                          new Vector3(0.5f, taille.y + 1f, 1f));

        // ── Sprite cadre selon nb joueurs ─────────────────────────────────────
        if (cadreTerrain != null)
        {
            // Choisit le bon sprite
            cadreTerrain.sprite = nbJoueurs switch
            {
                2 => cadre2J,
                3 => cadre3J,
                _ => cadre4J,
            };

            // Centre le cadre sur la zone de jeu
            cadreTerrain.transform.position = new Vector3(0, offsetY, 1f);

            // Passe derrière les joueurs
            cadreTerrain.sortingOrder = -1;

            Debug.Log($"[Terrain] Cadre {nbJoueurs}J appliqué");
        }

        // ── Bordure LineRenderer ──────────────────────────────────────────────
        if (ligneBordure != null)
        {
            ligneBordure.startColor = couleurBordure;
            ligneBordure.endColor = couleurBordure;
            ligneBordure.startWidth = epaisseurLigne;
            ligneBordure.endWidth = epaisseurLigne;
            ligneBordure.loop = true;
            ligneBordure.positionCount = 4;
            ligneBordure.useWorldSpace = true;
            ligneBordure.sortingOrder = 1;

            ligneBordure.SetPosition(0, new Vector3(-w, -h + offsetY, 0));
            ligneBordure.SetPosition(1, new Vector3(w, -h + offsetY, 0));
            ligneBordure.SetPosition(2, new Vector3(w, h + offsetY, 0));
            ligneBordure.SetPosition(3, new Vector3(-w, h + offsetY, 0));
        }

        Debug.Log($"[Terrain] {taille.x}x{taille.y} offsetY={offsetY}");
    }

    void SetMur(Transform mur, Vector3 pos, Vector3 scale)
    {
        if (mur == null) return;
        mur.position = pos;
        mur.localScale = scale;
    }
}