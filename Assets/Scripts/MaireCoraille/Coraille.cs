using UnityEngine;
using System.Collections;

public class Coraille : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;

    [Header("Slots du binome")]
    public RacailleController slotA;
    public RacailleController slotB;

    [Header("Visuels des cotes")]
    public SpriteRenderer spriteA;
    public SpriteRenderer spriteB;

    [Header("Colliders")]
    public Collider2D colliderCoteA;
    public Collider2D colliderCoteB;
    public Collider2D colliderCorps;

    [Header("Etat")]
    public bool enCooldown = false;
    public float cooldownRestant = 0f;

    // ── Init visuel ───────────────────────────────────────────────────────────
    public void InitVisuel()
    {
        if (slotA != null && spriteA != null)
        {
            PlayerData dataA = GameData.GetJoueur(slotA.playerID);
            spriteA.sprite = SpriteFactory.Creer(dataA.forme, dataA.GetCouleur());
            spriteA.color = Color.white;
        }

        if (slotB != null && spriteB != null)
        {
            PlayerData dataB = GameData.GetJoueur(slotB.playerID);
            spriteB.sprite = SpriteFactory.Creer(dataB.forme, dataB.GetCouleur());
            spriteB.color = Color.white;
        }
    }

    // ── Collision physique sur le corps (BoxCollider2D pas trigger) ───────────
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

    // ── Tentative d'accrochage (appelé par CorailleCoteDetector) ─────────────
    public void TenterAccrochage(RacailleController racaille, bool entreParCoteA)
    {
        // Le maire (requin) → rebond simple, pas de freeze
        if (racaille.isMayor)
        {
            Debug.Log($"[Coraille] Requin J{racaille.playerID} → Rebond !");
            Repulsion(racaille);
            return;
        }

        // Cooldown actif → rebond
        if (enCooldown)
        {
            Debug.Log($"[Coraille] Cooldown actif → Rebond J{racaille.playerID}");
            Repulsion(racaille);
            return;
        }

        // Pas d'allié dans le binôme → rebond
        if (!ContientEquipe(racaille.playerID))
        {
            Debug.Log($"[Coraille] Pas d'allié J{racaille.playerID} → Rebond");
            Repulsion(racaille);
            return;
        }

        // Mauvais côté → rebond
        RacailleController allie = GetAllie(racaille.playerID);
        bool allieEstCoteA = (allie == slotA);
        bool doitEntrerParCoteA = !allieEstCoteA;

        if (entreParCoteA != doitEntrerParCoteA)
        {
            Debug.Log($"[Coraille] Mauvais côté J{racaille.playerID} → Rebond");
            Repulsion(racaille);
            return;
        }

        // ✅ Fusion réussie !
        Debug.Log($"[Coraille] Fusion réussie J{racaille.playerID} !");
        FusionReussie(racaille);
    }

    // ── Rebond simple — comme une balle sur un mur ────────────────────────────
    void Repulsion(RacailleController racaille)
    {
        Vector2 dir = ((Vector2)racaille.transform.position
                     - (Vector2)transform.position).normalized;

        Rigidbody2D rb = racaille.GetComponent<Rigidbody2D>();
        Vector2 vitesseReflechie = Vector2.Reflect(rb.linearVelocity, dir);

        if (vitesseReflechie.magnitude < config.corailleRebondForceMin)
            vitesseReflechie = dir * config.corailleRebondForceMin;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(vitesseReflechie * config.knockbackMultiplier, ForceMode2D.Impulse);
        racaille.SyncVelocity(vitesseReflechie * config.knockbackMultiplier);
    }

    // ── Fusion réussie ────────────────────────────────────────────────────────
    void FusionReussie(RacailleController poisson)
    {
        // Direction d'entrée = joueur → coraille
        Vector2 dirEntree = ((Vector2)transform.position
                           - (Vector2)poisson.transform.position).normalized;

        // TP de l'autre côté du coraille
        Vector3 posTP = transform.position + (Vector3)(dirEntree * 1.5f);
        poisson.transform.position = posTP;

        // Impulsion forte dans le même sens que le TP
        Rigidbody2D rb = poisson.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dirEntree * config.propulsionForce, ForceMode2D.Impulse);

        // Effet visuel du coraille
        StartCoroutine(EffetVisuelCoraille(dirEntree));

        // Démarre le cooldown
        StartCoroutine(DemarrerCooldown());
    }

    // ── Effet visuel coraille ─────────────────────────────────────────────────
    IEnumerator EffetVisuelCoraille(Vector2 dirEntree)
    {
        // Flip du sprite
        Vector3 scaleOriginal = transform.localScale;
        transform.localScale = new Vector3(
            -scaleOriginal.x,
             scaleOriginal.y,
             scaleOriginal.z
        );

        // Légèrement dans le sens opposé au TP (effet d'accrochage)
        Vector3 posOriginal = transform.position;
        Vector3 posDecalee = posOriginal + (Vector3)(-dirEntree * 0.3f);

        float t = 0f;
        while (t < 0.15f)
        {
            transform.position = Vector3.Lerp(posOriginal, posDecalee, t / 0.15f);
            t += Time.deltaTime;
            yield return null;
        }

        // Revient à la position originale
        t = 0f;
        while (t < 0.15f)
        {
            transform.position = Vector3.Lerp(posDecalee, posOriginal, t / 0.15f);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = posOriginal;
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