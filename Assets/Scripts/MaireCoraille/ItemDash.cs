using UnityEngine;
using System.Collections;

public class ItemDash : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;
    public ItemDashManager manager;

    [Header("Sprite")]
    public Sprite spriteItem;
    private SpriteRenderer sr;

    [Header("Balancement")]
    public float ampBalancement = 0.08f; // hauteur du balancement
    public float vitesseBalance = 1.2f;  // vitesse
    public float ampRotation = 8f;    // degrés de rotation
    private Vector3 posBase;

    [Header("Ondes")]
    public Transform spawnOnde;           // point de spawn des ondes
    public float intervalleOnde = 0.8f;
    public float dureeOnde = 0.6f;
    public float tailleOndeMax = 0.8f;
    public Color couleurOnde = new Color(0.5f, 0.85f, 1f, 0.3f);

    private float tempsOnde = 0f;
    private bool estActif = true;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            if (spriteItem != null)
                sr.sprite = spriteItem;
            else
                sr.sprite = SpriteFactory.Creer("etoile",
                            new Color(1f, 0.85f, 0.1f));
        }
    }

    void Start()
    {
        posBase = transform.position;
        tempsOnde = 0f;
        StartCoroutine(SpawnOndulation()); // ← onde dès le départ
    }

    void Update()
    {
        if (!estActif) return;

        // ── Balancement ───────────────────────────────────────────────────────
        float offsetY = Mathf.Sin(Time.time * vitesseBalance) * ampBalancement;
        float rotation = Mathf.Sin(Time.time * vitesseBalance * 0.8f) * ampRotation;

        transform.position = posBase + new Vector3(0, offsetY, 0);
        transform.rotation = Quaternion.Euler(0, 0, rotation);

        // ── Ondes — toujours actives ──────────────────────────────────────────
        tempsOnde += Time.deltaTime;
        if (tempsOnde >= intervalleOnde)
        {
            tempsOnde = 0f;
            StartCoroutine(SpawnOndulation());
        }

        // ── Collecte ──────────────────────────────────────────────────────────
        RacailleController[] racailles =
            FindObjectsByType<RacailleController>(FindObjectsSortMode.None);

        foreach (var r in racailles)
        {
            if (!r.isMayor) continue;
            if (Vector2.Distance(transform.position, r.transform.position)
                <= config.itemRayonCollecte)
            {
                Collecter(r);
                return;
            }
        }
    }

    void Collecter(RacailleController requin)
    {
        estActif = false;
        requin.AjouterMunitionDash();

        if (sr != null) sr.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(config.itemRespawnDelai);

        Vector2 nouvellePos = manager != null
            ? manager.GetPositionAleatoire()
            : Vector2.zero;

        transform.position = new Vector3(nouvellePos.x, nouvellePos.y, 0f);
        posBase = transform.position; // ← reset posBase !
        tempsOnde = 0f;                 // ← reset timer ondes

        if (sr != null) sr.enabled = true;
        GetComponent<Collider2D>().enabled = true;
        estActif = true;

        // Lance une onde immédiatement au respawn
        StartCoroutine(SpawnOndulation());
    }

    IEnumerator SpawnOndulation()
    {
        // Capture la position au moment du spawn — pas celle du spawnOnde qui bouge
        Vector3 pos = spawnOnde != null
            ? spawnOnde.position
            : transform.position;

        GameObject onde = new GameObject("OndulationItem");
        onde.transform.position = pos;
        onde.transform.SetParent(null); // ← détache du parent pour rester fixe

        MeshFilter mf = onde.AddComponent<MeshFilter>();
        MeshRenderer mr = onde.AddComponent<MeshRenderer>();

        mr.material = new Material(Shader.Find("Sprites/Default"));
        mr.material.color = couleurOnde;
        mr.sortingOrder = -5;
        mf.mesh = CreerMeshAnneau(0.3f, 0.4f, 32);

        float t = 0f;
        while (t < dureeOnde)
        {
            if (onde == null) yield break;

            float progress = t / dureeOnde;
            float taille = Mathf.Lerp(0f, tailleOndeMax, progress);
            onde.transform.localScale = new Vector3(taille, taille, 1f);

            Color c = couleurOnde;
            c.a = Mathf.Lerp(couleurOnde.a, 0f, progress);
            mr.material.color = c;

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(onde);
    }

    Mesh CreerMeshAnneau(float rayonInterne, float rayonExterne, int segments)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments * 2];
        int[] triangles = new int[segments * 6];
        Color[] colors = new Color[segments * 2];

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            vertices[i * 2] = new Vector3(cos * rayonInterne,
                                              sin * rayonInterne, 0);
            vertices[i * 2 + 1] = new Vector3(cos * rayonExterne,
                                              sin * rayonExterne, 0);
            colors[i * 2] = Color.white;
            colors[i * 2 + 1] = Color.white;
        }

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int idx = i * 6;
            int v0 = i * 2;
            int v1 = i * 2 + 1;
            int v2 = next * 2;
            int v3 = next * 2 + 1;

            triangles[idx] = v0;
            triangles[idx + 1] = v1;
            triangles[idx + 2] = v2;
            triangles[idx + 3] = v2;
            triangles[idx + 4] = v1;
            triangles[idx + 5] = v3;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        return mesh;
    }
}