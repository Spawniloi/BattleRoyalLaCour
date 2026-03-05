using UnityEngine;
using System.Collections;

public class RacailleVisuel : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite spriteCorps;
    public Sprite spriteDossard;
    public Sprite spriteJambes;
    public Sprite[] spritesTetes;

    [Header("Sprites Role")]
    public SpriteRenderer srAileronRequin;
    public SpriteRenderer srTeteRequin;
    public SpriteRenderer srQueuePoisson;
    public SpriteRenderer srTetePoisson;

    [Header("Details Requin (couleur fixe)")]
    public SpriteRenderer srDetailsRequin;

    [Header("Renderers")]
    public SpriteRenderer srCorps;
    public SpriteRenderer srDossard;
    public SpriteRenderer srJambes;
    public SpriteRenderer srTete;
    public SpriteRenderer srBouee;

    [Header("Couleur jambes fixe")]
    public Color couleurJambes = new Color(0.1f, 0.15f, 0.3f);

    [Header("Animation rebond")]
    public float scaleRebond = 1.3f;
    public float dureeRebond = 0.15f;

    [Header("Animation marche — 4 frames")]
    public Sprite[] framesMarche;
    public float vitesseAnim = 8f;

    [Header("Ondulation eau")]
    public float intervalleOnde = 0.3f;
    public float dureeOnde = 0.6f;
    public float tailleOndeMax = 1.2f;
    public Color couleurOnde = new Color(1f, 1f, 1f, 0.4f);

    private int frameActuelle = 0;
    private float tempsFrame = 0f;
    private float tempsOnde = 0f;
    private RacailleController rc;

    // ── Awake — cache tout par défaut ─────────────────────────────────────────
    void Awake()
    {
        rc = GetComponent<RacailleController>();
        if (srCorps != null) srCorps.enabled = false;
        if (srDossard != null) srDossard.enabled = false;
        if (srJambes != null) srJambes.enabled = false;
        if (srTete != null) srTete.enabled = false;
        if (srBouee != null) srBouee.enabled = false;
        if (srAileronRequin != null) srAileronRequin.enabled = false;
        if (srTeteRequin != null) srTeteRequin.enabled = false;
        if (srQueuePoisson != null) srQueuePoisson.enabled = false;
        if (srTetePoisson != null) srTetePoisson.enabled = false;
        if (srDetailsRequin != null) srDetailsRequin.enabled = false;
    }

    // ── Update — animation + ondulation ──────────────────────────────────────
    void Update()
    {
        if (rc == null) return;

        Rigidbody2D rb = rc.GetComponent<Rigidbody2D>();
        bool bouge = rb != null && rb.linearVelocity.magnitude > 0.3f;

        // ── Animation frames ──────────────────────────────────────────────────
        if (framesMarche != null && framesMarche.Length > 0 && srCorps != null)
        {
            if (bouge)
            {
                tempsFrame += Time.deltaTime * vitesseAnim;
                if (tempsFrame >= 1f)
                {
                    tempsFrame -= 1f;
                    frameActuelle = (frameActuelle + 1) % framesMarche.Length;
                    srCorps.sprite = framesMarche[frameActuelle];
                }
            }
            else
            {
                if (frameActuelle != 0)
                {
                    frameActuelle = 0;
                    tempsFrame = 0f;
                    srCorps.sprite = framesMarche[0];
                }
            }
        }

        // ── Ondulation eau ────────────────────────────────────────────────────
        if (bouge)
        {
            tempsOnde += Time.deltaTime;
            if (tempsOnde >= intervalleOnde)
            {
                tempsOnde = 0f;
                StartCoroutine(SpawnOndulation());
            }
        }
    }

    IEnumerator SpawnOndulation()
    {
        GameObject onde = new GameObject("Ondulation");
        onde.transform.position = transform.position;

        // Ajoute un MeshFilter + MeshRenderer pour l'anneau
        MeshFilter mf = onde.AddComponent<MeshFilter>();
        MeshRenderer mr = onde.AddComponent<MeshRenderer>();

        mr.material = new Material(Shader.Find("Sprites/Default"));
        mr.material.color = couleurOnde;
        mr.sortingOrder = -5;

        mf.mesh = CreerMeshAnneau(0.4f, 0.5f, 32); // rayon interne, externe, segments

        float t = 0f;
        while (t < dureeOnde)
        {
            if (onde == null) yield break;

            float progress = t / dureeOnde;

            // Grandit
            float taille = Mathf.Lerp(0f, tailleOndeMax, progress);
            onde.transform.localScale = new Vector3(taille, taille, 1f);

            // Disparait
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
        int nbVerts = segments * 2;

        Vector3[] vertices = new Vector3[nbVerts];
        int[] triangles = new int[segments * 6];
        Color[] colors = new Color[nbVerts];

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            vertices[i * 2] = new Vector3(cos * rayonInterne, sin * rayonInterne, 0);
            vertices[i * 2 + 1] = new Vector3(cos * rayonExterne, sin * rayonExterne, 0);
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


    // ── Applique les données joueur ───────────────────────────────────────────
    public void AppliquerData(PlayerData data)
    {
        if (srCorps != null) srCorps.enabled = true;
        if (srDossard != null) srDossard.enabled = true;
        if (srJambes != null) srJambes.enabled = true;
        if (srTete != null) srTete.enabled = true;
        if (srBouee != null) srBouee.enabled = true;

        if (srCorps != null)
        {
            // Frame idle par défaut si frames disponibles
            srCorps.sprite = (framesMarche != null && framesMarche.Length > 0)
                           ? framesMarche[0]
                           : spriteCorps;
            srCorps.color = data.GetCouleurPeau();
        }

        if (srDossard != null)
        {
            srDossard.sprite = spriteDossard;
            srDossard.color = data.GetCouleurDossard();
        }

        if (srJambes != null)
        {
            srJambes.sprite = spriteJambes;
            srJambes.color = couleurJambes;
        }

        if (srTete != null)
        {
            if (spritesTetes != null &&
                data.indexTete < spritesTetes.Length &&
                spritesTetes[data.indexTete] != null)
                srTete.sprite = spritesTetes[data.indexTete];
            srTete.color = data.GetCouleurDossard();
        }

        Color coul = data.GetCouleurDossard();
        if (srAileronRequin != null) srAileronRequin.color = coul;
        if (srTeteRequin != null) srTeteRequin.color = coul;
        if (srQueuePoisson != null) srQueuePoisson.color = coul;
        if (srTetePoisson != null) srTetePoisson.color = coul;
        if (srBouee != null) srBouee.color = coul;
    }

    // ── Visuel rôle ───────────────────────────────────────────────────────────
    public void SetRoleVisuel(bool isMayor)
    {
        PlayerData data = GameData.GetJoueur(
            GetComponent<RacailleController>().playerID);
        Color coul = data.GetCouleurDossard();

        if (srAileronRequin != null)
        {
            srAileronRequin.color = coul;
            srAileronRequin.enabled = isMayor;
        }
        if (srTeteRequin != null)
        {
            srTeteRequin.color = coul;
            srTeteRequin.enabled = isMayor;
        }
        if (srDetailsRequin != null)
            srDetailsRequin.enabled = isMayor;

        if (srQueuePoisson != null)
        {
            srQueuePoisson.color = coul;
            srQueuePoisson.enabled = !isMayor;
        }
        if (srTetePoisson != null)
        {
            srTetePoisson.color = coul;
            srTetePoisson.enabled = !isMayor;
        }
    }

    // ── Animation rebond ──────────────────────────────────────────────────────
    public void JouerRebond()
    {
        MaireAudioManager.Instance?.JouerRebond();
        StopCoroutine("EffetRebond");
        StartCoroutine(EffetRebond());
    }

    IEnumerator EffetRebond()
    {
        float t = 0f;
        while (t < dureeRebond / 2f)
        {
            float scale = Mathf.Lerp(1f, scaleRebond, t / (dureeRebond / 2f));
            transform.localScale = new Vector3(scale, scale, 1f);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < dureeRebond / 2f)
        {
            float scale = Mathf.Lerp(scaleRebond, 1f, t / (dureeRebond / 2f));
            transform.localScale = new Vector3(scale, scale, 1f);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }
}