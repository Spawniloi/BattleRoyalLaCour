using UnityEngine;
using System.Collections;

public class FondEau : MonoBehaviour
{
    [Header("References")]
    public TerrainManager terrainManager;

    [Header("Couche 1 — Fond")]
    public SpriteRenderer srFond;
    public Color couleurFond = new Color(0.1f, 0.45f, 0.75f, 1f);

    [Header("Couche 2 — Vagues")]
    public SpriteRenderer srVagues;
    public Sprite spriteVagues;
    public Color couleurVagues = new Color(0.5f, 0.85f, 1f, 0.4f);

    [Header("Taille sprite vagues (PPU=1010)")]
    public float largeurSpriteUnites = 1.584f;  // 1600/1010
    public float hauteurSpriteUnites = 1.188f;  // 1200/1010

    [Header("Animation vagues")]
    public float ampOscillation = 0.3f;
    public float vitesseOsc = 0.8f;
    public float alphaMin = 0.05f;
    public float alphaMax = 0.4f;
    public float vitesseAlpha = 0.6f;

    private Vector3 posVaguesBase;
    private Vector2 taille;
    private bool pret = false;

    void Start()
    {
        StartCoroutine(InitApresDelai());
    }

    IEnumerator InitApresDelai()
    {
        yield return null;
        yield return null;

        if (terrainManager == null) yield break;

        taille = terrainManager.TailleTerrain;
        if (taille == Vector2.zero) yield break;

        float y = terrainManager.offsetY;

        // ── Fond ──────────────────────────────────────────────────────────────
        if (srFond != null)
        {
            srFond.color = couleurFond;
            srFond.sortingOrder = -10;
            srFond.transform.position = new Vector3(0, y, 0);
            srFond.transform.localScale = new Vector3(taille.x, taille.y, 1f);
        }

        // ── Vagues ────────────────────────────────────────────────────────────
        if (srVagues != null && spriteVagues != null)
        {
            srVagues.sprite = spriteVagues;
            srVagues.color = couleurVagues;
            srVagues.sortingOrder = -9;

            float scaleX = taille.x / largeurSpriteUnites;
            float scaleY = taille.y / hauteurSpriteUnites;
            srVagues.transform.localScale = new Vector3(scaleX, scaleY, 1f);

            posVaguesBase = new Vector3(0, y, 0);
            srVagues.transform.position = posVaguesBase;
        }

        pret = true;
        Debug.Log($"[FondEau] Initialisé ! terrain={taille}");
    }

    void Update()
    {
        if (!pret || srVagues == null) return;

        // Oscillation haut / bas
        float offsetY = Mathf.Sin(Time.time * vitesseOsc) * ampOscillation;
        srVagues.transform.position = posVaguesBase
                                    + new Vector3(0, offsetY, 0);

        // Alpha diminue et remonte
        float alpha = Mathf.Lerp(alphaMin, alphaMax,
                         (Mathf.Sin(Time.time * vitesseAlpha) + 1f) / 2f);
        Color c = couleurVagues;
        c.a = alpha;
        srVagues.color = c;
    }
}