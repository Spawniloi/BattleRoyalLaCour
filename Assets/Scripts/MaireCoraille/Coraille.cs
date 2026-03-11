using UnityEngine;
using System.Collections;

public class Coraille : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;

    [Header("Slots du binome")]
    public RacailleController slotA;
    public RacailleController slotB;

    [Header("Visuels Cote A")]
    public SpriteRenderer coteA_Jambes;
    public SpriteRenderer coteA_Corps;
    public SpriteRenderer coteA_Dossard;
    public SpriteRenderer coteA_Tete;
    public SpriteRenderer coteA_Bouee;

    [Header("Visuels Cote B")]
    public SpriteRenderer coteB_Jambes;
    public SpriteRenderer coteB_Corps;
    public SpriteRenderer coteB_Dossard;
    public SpriteRenderer coteB_Tete;
    public SpriteRenderer coteB_Bouee;

    [Header("Colliders")]
    public Collider2D colliderCoteA;
    public Collider2D colliderCoteB;
    public Collider2D colliderCorps;

    [Header("Etat")]
    public bool enCooldown = false;
    public float cooldownRestant = 0f;

    [Header("Ondulation rotation")]
    public float intervalleOndeRot = 0.4f;
    public float dureeOndeRot = 0.5f;
    public float tailleOndeMaxRot = 1.5f;
    public Color couleurOndeRot = new Color(0.5f, 0.85f, 1f, 0.35f);

    [Header("Spawn ondes")]
    public Transform spawnOndeA;
    public Transform spawnOndeB;

    private float tempsOndeRot = 0f;
    private float anglePrec = 0f;

    private bool effetEnCours = false;

    void Start()
    {
        StartCoroutine(RotationAleatoire());
    }

    // ── Init visuel ───────────────────────────────────────────────────────────
    public void InitVisuel()
    {
        AppliquerVisuelCote(slotA,
            coteA_Jambes, coteA_Corps, coteA_Dossard, coteA_Tete, coteA_Bouee);
        AppliquerVisuelCote(slotB,
            coteB_Jambes, coteB_Corps, coteB_Dossard, coteB_Tete, coteB_Bouee);
    }

    void AppliquerVisuelCote(RacailleController racaille,
        SpriteRenderer srJambes,
        SpriteRenderer srCorps,
        SpriteRenderer srDossard,
        SpriteRenderer srTete,
        SpriteRenderer srBouee)
    {
        if (racaille == null) return;

        PlayerData data = GameData.GetJoueur(racaille.playerID);
        RacailleVisuel visuel = racaille.GetComponent<RacailleVisuel>();

        if (visuel == null) return;

        if (srJambes != null)
        {
            srJambes.sprite = visuel.spriteJambes;
            srJambes.color = visuel.couleurJambes;
        }

        if (srCorps != null)
        {
            srCorps.sprite = visuel.spriteCorps;
            srCorps.color = data.GetCouleurPeau();
        }

        if (srDossard != null)
        {
            srDossard.sprite = visuel.spriteDossard;
            srDossard.color = data.GetCouleurDossard();
        }

        if (srTete != null)
        {
            if (visuel.spritesTetes != null &&
                data.indexTete < visuel.spritesTetes.Length &&
                visuel.spritesTetes[data.indexTete] != null)
                srTete.sprite = visuel.spritesTetes[data.indexTete];
            srTete.color = data.GetCouleurDossard();
        }

        // Bouée — couleur dossard
        if (srBouee != null)
            srBouee.color = data.GetCouleurDossard();
    }

    // ── Rotation aleatoire ────────────────────────────────────────────────────
    IEnumerator RotationAleatoire()
    {
        while (true)
        {
            while (effetEnCours)
                yield return null;

            float angleCible = Random.Range(config.corailleAngleMin,
                                             config.corailleAngleMax);
            float angleDepart = transform.eulerAngles.z;
            if (angleDepart > 180f) angleDepart -= 360f;

            float duree = Random.Range(config.corailleDureeRotMin,
                                       config.corailleDureeRotMax);

            float t = 0f;
            while (t < duree)
            {
                if (effetEnCours) { yield return null; continue; }

                float angle = Mathf.LerpAngle(angleDepart, angleCible, t / duree);
                transform.rotation = Quaternion.Euler(0f, 0f, angle);

                // Ondulation pendant la rotation
                tempsOndeRot += Time.deltaTime;
                if (tempsOndeRot >= intervalleOndeRot)
                {
                    tempsOndeRot = 0f;
                    StartCoroutine(SpawnOndulationCoraille());
                }

                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(
                Random.Range(config.coraillePauseRotMin,
                             config.coraillePauseRotMax));
        }
    }

    IEnumerator SpawnOndulationCoraille()
    {
        // Lance 2 ondes — une par point de spawn
        if (spawnOndeA != null)
            StartCoroutine(AnimerOnde(spawnOndeA.position));
        else
            StartCoroutine(AnimerOnde(transform.position));

        if (spawnOndeB != null)
            StartCoroutine(AnimerOnde(spawnOndeB.position));
        else
            StartCoroutine(AnimerOnde(transform.position));

        yield break;
    }

    IEnumerator AnimerOnde(Vector3 position)
    {
        GameObject onde = new GameObject("OndulationCoraille");
        onde.transform.position = position;

        MeshFilter mf = onde.AddComponent<MeshFilter>();
        MeshRenderer mr = onde.AddComponent<MeshRenderer>();

        mr.material = new Material(Shader.Find("Sprites/Default"));
        mr.material.color = couleurOndeRot;
        mr.sortingOrder = -5;
        mf.mesh = CreerMeshAnneau(0.4f, 0.5f, 32);

        float t = 0f;
        while (t < dureeOndeRot)
        {
            if (onde == null) yield break;

            float progress = t / dureeOndeRot;
            float taille = Mathf.Lerp(0f, tailleOndeMaxRot, progress);
            onde.transform.localScale = new Vector3(taille, taille, 1f);

            Color c = couleurOndeRot;
            c.a = Mathf.Lerp(couleurOndeRot.a, 0f, progress);
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

    // ── Collision physique corps ──────────────────────────────────────────────
    void OnCollisionEnter2D(Collision2D collision)
    {
        RacailleController racaille =
            collision.gameObject.GetComponent<RacailleController>();
        if (racaille == null) return;

        Vector2 dirRebond = ((Vector2)racaille.transform.position
                           - (Vector2)transform.position).normalized;

        Rigidbody2D rb = racaille.GetComponent<Rigidbody2D>();
        Vector2 vitesseReflechie = Vector2.Reflect(rb.linearVelocity, dirRebond);

        if (vitesseReflechie.magnitude < config.corailleRebondForceMin)
            vitesseReflechie = dirRebond * config.corailleRebondForceMin;

        rb.linearVelocity = vitesseReflechie * config.knockbackMultiplier;
        racaille.SyncVelocity(rb.linearVelocity);
    }

    // ── Tentative d'accrochage ────────────────────────────────────────────────
    public void TenterAccrochage(RacailleController racaille, bool entreParCoteA)
    {
        if (racaille.isMayor) { Repulsion(racaille); return; }
        if (enCooldown) { Repulsion(racaille); return; }

        if (!ContientEquipe(racaille.playerID))
        {
            Debug.Log($"[Coraille] Pas d'allié J{racaille.playerID} → Rebond");
            Repulsion(racaille);
            return;
        }

        RacailleController allie = GetAllie(racaille.playerID);
        bool allieEstCoteA = (allie == slotA);
        bool doitEntrerParCoteA = !allieEstCoteA;

        if (entreParCoteA != doitEntrerParCoteA)
        {
            Debug.Log($"[Coraille] Mauvais côté J{racaille.playerID} → Rebond");
            Repulsion(racaille);
            return;
        }

        Debug.Log($"[Coraille] Fusion réussie J{racaille.playerID} !");
        FusionReussie(racaille);
    }

    // ── Repulsion ─────────────────────────────────────────────────────────────
    void Repulsion(RacailleController racaille)
    {
        racaille.GetComponent<RacailleVisuel>()?.JouerRebond();

        Vector2 dir = ((Vector2)racaille.transform.position
                     - (Vector2)transform.position).normalized;

        Rigidbody2D rb = racaille.GetComponent<Rigidbody2D>();
        Vector2 vitesseReflechie = Vector2.Reflect(rb.linearVelocity, dir);

        if (vitesseReflechie.magnitude < config.corailleRebondForceMin)
            vitesseReflechie = dir * config.corailleRebondForceMin;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(vitesseReflechie * config.knockbackMultiplier,
                    ForceMode2D.Impulse);
        racaille.SyncVelocity(vitesseReflechie * config.knockbackMultiplier);
    }

    // ── Fusion reussie ────────────────────────────────────────────────────────
    void FusionReussie(RacailleController poisson)
    {
        Vector2 dirEntree = ((Vector2)transform.position
                           - (Vector2)poisson.transform.position).normalized;

        Vector3 posTP = transform.position + (Vector3)(dirEntree * 1.5f);
        poisson.transform.position = posTP;

        Rigidbody2D rb = poisson.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dirEntree * config.propulsionForce, ForceMode2D.Impulse);

        MaireAudioManager.Instance?.JouerFusion();
        StatsTracker.Instance?.OnPassageCoraille(poisson.playerID);

        StartCoroutine(EffetVisuelCoraille(dirEntree));
        StartCoroutine(DemarrerCooldown());
    }

    // ── Effet visuel ──────────────────────────────────────────────────────────
    IEnumerator EffetVisuelCoraille(Vector2 dirEntree)
    {
        effetEnCours = true;

        Vector3 posFixe = transform.position;
        Vector3 scaleActuel = transform.localScale;
        Vector3 scaleAbs = new Vector3(
            Mathf.Abs(scaleActuel.x),
            Mathf.Abs(scaleActuel.y),
            1f
        );

        Vector3 scaleFlip = new Vector3(-scaleActuel.x, scaleAbs.y, 1f);
        transform.localScale = scaleFlip;
        transform.position = posFixe;

        Vector3 scaleSquish = new Vector3(
            scaleFlip.x * 1.2f,
            scaleFlip.y * 0.8f,
            1f
        );

        float t = 0f;
        while (t < 0.08f)
        {
            transform.localScale = Vector3.Lerp(scaleFlip, scaleSquish, t / 0.08f);
            transform.position = posFixe;
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < 0.12f)
        {
            transform.localScale = Vector3.Lerp(scaleSquish, scaleFlip, t / 0.12f);
            transform.position = posFixe;
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = scaleFlip;
        transform.position = posFixe;
        effetEnCours = false;
    }

    // ── Cooldown ──────────────────────────────────────────────────────────────
    IEnumerator DemarrerCooldown()
    {
        enCooldown = true;
        cooldownRestant = config.corailleCooldown;

        while (cooldownRestant > 0)
        {
            cooldownRestant -= Time.deltaTime;
            yield return null;
        }

        enCooldown = false;
        cooldownRestant = 0f;
        Debug.Log("[Coraille] Cooldown terminé !");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    public bool ContientEquipe(int playerID)
    {
        if (slotA != null && slotA.playerID == playerID) return true;
        if (slotB != null && slotB.playerID == playerID) return true;
        return false;
    }

    public RacailleController GetAllie(int playerID)
    {
        if (slotA != null && slotA.playerID == playerID) return slotA;
        if (slotB != null && slotB.playerID == playerID) return slotB;
        return null;
    }

    public RacailleController GetEnnemi(int playerID)
    {
        if (slotA != null && slotA.playerID != playerID) return slotA;
        if (slotB != null && slotB.playerID != playerID) return slotB;
        return null;
    }
}