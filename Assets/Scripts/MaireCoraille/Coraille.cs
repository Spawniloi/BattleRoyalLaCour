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

    // ── Tentative d'accrochage ────────────────────────────────────────────────
    public void TenterAccrochage(RacailleController racaille, bool entreParCoteA)
    {
        // ── Le MAIRE (requin) → toujours répulsion + mini freeze ─────────────────
        if (racaille.isMayor)
        {
            Debug.Log($"[Coraille] Requin J{racaille.playerID} → Répulsion !");
            RepulsionAvecFreeze(racaille, entreParCoteA);
            return;
        }

        // ── FUGITIF (poisson) ─────────────────────────────────────────────────────

        // Cooldown actif ?
        if (enCooldown)
        {
            RepulsionAvecFreeze(racaille, entreParCoteA);
            return;
        }

        // Vérifie si allié présent
        if (!ContientEquipe(racaille.playerID))
        {
            Debug.Log($"[Coraille] Pas d'allié J{racaille.playerID} → Répulsion");
            RepulsionAvecFreeze(racaille, entreParCoteA);
            return;
        }

        // Vérifie le bon côté
        RacailleController allie = GetAllie(racaille.playerID);
        bool allieEstCoteA = (allie == slotA);
        bool doitEntrerParCoteA = !allieEstCoteA;

        if (entreParCoteA != doitEntrerParCoteA)
        {
            Debug.Log($"[Coraille] Mauvais côté J{racaille.playerID} → Répulsion");
            RepulsionAvecFreeze(racaille, entreParCoteA);
            return;
        }

        // ✅ Fusion réussie !
        Debug.Log($"[Coraille] Fusion réussie J{racaille.playerID} !");
        FusionReussie(racaille, entreParCoteA);
    }

    void RepulsionAvecFreeze(RacailleController racaille, bool entreParCoteA)
    {
        // Repousse dans le sens opposé d'où il venait
        Vector2 dirRepulsion = entreParCoteA ?
            -(Vector2)transform.right :   // venait de la gauche → repousse à gauche
             (Vector2)transform.right;    // venait de la droite → repousse à droite

        Rigidbody2D rb = racaille.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dirRepulsion * config.corailleRepulsionForce * 3f,
                    ForceMode2D.Impulse);

        // Mini freeze
        racaille.ApplyStun(config.stunDuration);
    }

    // ── Fusion réussie ────────────────────────────────────────────────────────
    void FusionReussie(RacailleController poisson, bool entreParCoteA)
    {
        // Direction d'entrée = position joueur → centre coraille
        Vector2 dirEntree = ((Vector2)transform.position
                           - (Vector2)poisson.transform.position).normalized;

        // TP de l'autre côté = continue dans la même direction
        Vector3 posTP = transform.position + (Vector3)(dirEntree * 1.5f);
        poisson.transform.position = posTP;

        // Impulsion dans le même sens que le TP
        Rigidbody2D rb = poisson.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dirEntree * config.propulsionForce, ForceMode2D.Impulse);

        // Effet visuel — passe la direction d'entrée pour le flip
        StartCoroutine(EffetVisuelCoraille(dirEntree));

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