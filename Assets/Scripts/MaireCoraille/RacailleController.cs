using UnityEngine;
using System.Collections;

public class Coraille : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;

    [Header("Slots du binome")]
    public RacailleController slotA; // racaille côté A
    public RacailleController slotB; // racaille côté B

    [Header("Colliders des côtés")]
    public Collider2D colliderCoteA; // trigger côté A
    public Collider2D colliderCoteB; // trigger côté B

    [Header("Etat")]
    public bool enCooldown = false;
    public float cooldownRestant = 0f;

    // ── Vérifications ─────────────────────────────────────────────────────────

    // Est-ce qu'une racaille d'une équipe est dans ce binôme ?
    public bool ContientEquipe(int playerID)
    {
        if (slotA != null && slotA.playerID == playerID) return true;
        if (slotB != null && slotB.playerID == playerID) return true;
        return false;
    }

    // Retourne la racaille alliée dans le binôme
    public RacailleController GetAllie(int playerID)
    {
        if (slotA != null && slotA.playerID == playerID) return slotA;
        if (slotB != null && slotB.playerID == playerID) return slotB;
        return null;
    }

    // Retourne la racaille ennemie dans le binôme
    public RacailleController GetEnnemi(int playerID)
    {
        if (slotA != null && slotA.playerID != playerID) return slotA;
        if (slotB != null && slotB.playerID != playerID) return slotB;
        return null;
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
            // Cas 1 : pas d'allié → répulsion + stun
            Debug.Log($"[Coraille] Pas d'allié pour J{poisson.playerID} → Répulsion");
            Repulsion(poisson);
            return;
        }

        // Vérifie le bon côté
        // Le poisson doit entrer par le côté de l'ennemi
        RacailleController allie = GetAllie(poisson.playerID);
        RacailleController ennemi = GetEnnemi(poisson.playerID);

        bool allieEstCoteA = (allie == slotA);
        bool doitEntrerParCoteA = !allieEstCoteA; // entre par le côté opposé

        if (entreParCoteA != doitEntrerParCoteA)
        {
            // Cas 2 : mauvais côté → répulsion + stun
            Debug.Log($"[Coraille] Mauvais côté pour J{poisson.playerID} → Répulsion");
            Repulsion(poisson);
            return;
        }

        // ✅ Fusion réussie !
        Debug.Log($"[Coraille] Fusion réussie pour J{poisson.playerID} !");
        FusionReussie(poisson, ennemi);
    }

    void FusionReussie(RacailleController poisson, RacailleController ennemi)
    {
        // Propulsion pour fuir
        Vector2 dirFuite = (poisson.transform.position
                          - transform.position).normalized;
        poisson.GetComponent<Rigidbody2D>()
               .AddForce(dirFuite * config.propulsionForce, ForceMode2D.Impulse);

        // Démarre le cooldown
        StartCoroutine(DemarrerCooldown());

        Debug.Log($"[Coraille] Propulsion J{poisson.playerID} !");
    }

    void Repulsion(RacailleController poisson)
    {
        // Repousse le poisson
        Vector2 dirRepulsion = (poisson.transform.position
                              - transform.position).normalized;
        poisson.GetComponent<Rigidbody2D>()
               .AddForce(dirRepulsion * config.corailleRepulsionForce,
                         ForceMode2D.Impulse);

        // Stun
        poisson.ApplyStun(config.stunDuration);
    }

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
}