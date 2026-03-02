using UnityEngine;
using System.Collections;

public class FondEau : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer sr;
    public TerrainManager terrainManager;

    [Header("Couleurs eau")]
    public Color couleurEau = new Color(0.15f, 0.55f, 0.85f);
    public Color couleurReflets = new Color(0.5f, 0.8f, 1.0f);

    [Header("Animation")]
    public float vitesse = 0.5f;
    public float intensite = 0.08f;

    void Start()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.color = couleurEau;
        StartCoroutine(InitApresDelai());
    }

    IEnumerator InitApresDelai()
    {
        // Attend 1 frame que TerrainManager ait fini InitTerrain()
        yield return null;
        yield return null;

        if (terrainManager == null) yield break;

        Vector2 taille = terrainManager.TailleTerrain;

        Debug.Log($"[FondEau] Taille terrain : {taille}");

        if (taille == Vector2.zero)
        {
            Debug.LogWarning("[FondEau] TailleTerrain est (0,0) !");
            yield break;
        }

        // Taille + position
        transform.localScale = new Vector3(taille.x, taille.y, 1f);
        transform.position = new Vector3(0f, terrainManager.offsetY, 0f);

        sr.color = couleurEau;

        Debug.Log($"[FondEau] Initialisé ! Scale={transform.localScale}");
    }

    void Update()
    {
        if (sr == null) return;
        float t = (Mathf.Sin(Time.time * vitesse) + 1f) / 2f;
        sr.color = Color.Lerp(couleurEau, couleurReflets, t * intensite);
    }
}