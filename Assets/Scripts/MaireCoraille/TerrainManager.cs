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

    [Header("Cadres piscine — positionnes manuellement")]
    public GameObject cadre2J;
    public GameObject cadre3J;
    public GameObject cadre4J;

    [Header("Offset terrain (compense bande UI en haut)")]
    public float offsetY = -0.36f;

    [Header("Preview dans editeur")]
    public int previewNbJoueurs = 2;

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

        // ── Cadres piscine ────────────────────────────────────────────────────
        if (cadre2J != null) cadre2J.SetActive(nbJoueurs == 2);
        if (cadre3J != null) cadre3J.SetActive(nbJoueurs == 3);
        if (cadre4J != null) cadre4J.SetActive(nbJoueurs == 4);

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

    // ── Gizmos éditeur ────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (config == null) return;

        int nb = Application.isPlaying
                       ? GameData.nombreJoueurs
                       : previewNbJoueurs;
        Vector2 taille = config.GetTerrainSize(nb);
        float w = taille.x / 2f;
        float h = taille.y / 2f;

        // Murs — rouge
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.8f);
        Gizmos.DrawWireCube(new Vector3(0, h + offsetY, 0),
                            new Vector3(taille.x + 1f, 0.5f, 0f));
        Gizmos.DrawWireCube(new Vector3(0, -h + offsetY, 0),
                            new Vector3(taille.x + 1f, 0.5f, 0f));
        Gizmos.DrawWireCube(new Vector3(-w, offsetY, 0),
                            new Vector3(0.5f, taille.y + 1f, 0f));
        Gizmos.DrawWireCube(new Vector3(w, offsetY, 0),
                            new Vector3(0.5f, taille.y + 1f, 0f));

        // Zone de jeu — vert
        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.5f);
        Gizmos.DrawWireCube(new Vector3(0, offsetY, 0),
                            new Vector3(taille.x, taille.y, 0f));
    }
}