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
    public SpriteRenderer spriteA; // côté gauche
    public SpriteRenderer spriteB; // côté droit

    [Header("Colliders")]
    public Collider2D colliderCoteA;
    public Collider2D colliderCoteB;
    public Collider2D colliderCorps; // collider physique central

    [Header("Etat")]
    public bool enCooldown = false;
    public float cooldownRestant = 0f;

    // ── Init visuel ───────────────────────────────────────────────────────────
    public void InitVisuel()
    {
        if (slotA != null && spriteA != null)
        {
            SpriteRenderer srA = slotA.GetComponent<SpriteRenderer>();
            if (srA != null)
            {
                spriteA.sprite = srA.sprite;
                spriteA.color = srA.color;
            }
        }

        if (slotB != null && spriteB != null)
        {
            SpriteRenderer srB = slotB.GetComponent<SpriteRenderer>();
            if (srB != null)
            {
                spriteB.sprite = srB.sprite;
                spriteB.color = srB.color;
            }
        }
    }

    // ── Tentative d'accrochage ────────────────────────────────────────────────
    public void TenterAccrochage(RacailleController poisson, bool entreParCoteA)
    {
        // Seul un fugitif peut s'accrocher
        if (poisson.isMayor)
        {
            Debug.Log("[Coraille] Le requin ne peut pas s'accrocher !");
            return;
        }

        // Cooldown actif ?
        if (enCooldown)
        {
            Debug.Log("[Coraille] En cooldown !");
            Repulsion(poisson);
            return;
        }

        // Vérifie si une racaille alliée est dans le binôme
        bool alliePresent = ContientEquipe(poisson.playerID);

        if (!alliePresent)
        {
            Debug.Log($"[Coraille] Pas d'allié pour J{poisson.playerID} → Répulsion");
            Repulsion(poisson);
            return;
        }

        // Vérifie le bon côté
        // Le poisson doit entrer par le côté de l'ennemi
        RacailleController allie = GetAllie(poisson.playerID);
        bool allieEstCoteA = (allie == slotA);
        bool doitEntrerParCoteA = !allieEstCoteA;

        if (entreParCoteA != doitEntrerParCoteA)
        {
            Debug.Log($"[Coraille] Mauvais côté J{poisson.playerID} → Répulsion");
            Repulsion(poisson);
            return;
        }

        // ✅ Fusion réussie !
        Debug.Log($"[Coraille] Fusion réussie J{poisson.playerID} !");
        FusionReussie(poisson);
    }

    // ── Fusion réussie ────────────────────────────────────────────────────────
    void FusionReussie(RacailleController poisson)
    {
        // Direction d'entrée du poisson vers le coraille
        Vector2 dirEntree = (transform.position
                           - poisson.transform.position).normalized;

        // TP de l'autre côté du coraille
        Vector3 posTP = transform.position + (Vector3)(dirEntree * 1.2f);
        poisson.transform.position = posTP;

        // Impulsion forte dans le même sens que le TP
        Rigidbody2D rb = poisson.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dirEntree * config.propulsionForce, ForceMode2D.Impulse);

        // Effet visuel du coraille
        StartCoroutine(EffetVisuelCoraille(dirEntree));

        // Cooldown
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

        // Avance légèrement dans le sens opposé au TP
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

    // ── Répulsion ─────────────────────────────────────────────────────────────
    void Repulsion(RacailleController poisson)
    {
        Vector2 dirRepulsion = (poisson.transform.position
                              - transform.position).normalized;

        Rigidbody2D rb = poisson.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dirRepulsion * config.corailleRepulsionForce * 3f,
                    ForceMode2D.Impulse);

        poisson.ApplyStun(config.stunDuration);
        Debug.Log($"[Coraille] Répulsion J{poisson.playerID} !");
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