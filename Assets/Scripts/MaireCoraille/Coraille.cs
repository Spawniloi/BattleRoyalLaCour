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

    private bool effetEnCours = false; // bloque la rotation pendant l'effet

    void Start()
    {
        StartCoroutine(RotationAleatoire());
    }

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

    // ── Rotation aleatoire ────────────────────────────────────────────────────
    IEnumerator RotationAleatoire()
    {
        while (true)
        {
            // Attend si effet en cours
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
                // Pause si effet déclenché en cours de rotation
                if (effetEnCours)
                {
                    yield return null;
                    continue;
                }

                float angle = Mathf.LerpAngle(angleDepart, angleCible, t / duree);
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(
                Random.Range(config.coraillePauseRotMin,
                             config.coraillePauseRotMax));
        }
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
        if (racaille.isMayor)
        {
            Repulsion(racaille);
            return;
        }

        if (enCooldown)
        {
            Repulsion(racaille);
            return;
        }

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

        StartCoroutine(EffetVisuelCoraille(dirEntree));
        StartCoroutine(DemarrerCooldown());
    }

    // ── Effet visuel ──────────────────────────────────────────────────────────
    IEnumerator EffetVisuelCoraille(Vector2 dirEntree)
    {
        effetEnCours = true; // pause la rotation

        Vector3 posFixe = transform.position;
        Vector3 scaleActuel = transform.localScale;
        Vector3 scaleAbs = new Vector3(
            Mathf.Abs(scaleActuel.x),
            Mathf.Abs(scaleActuel.y),
            1f
        );

        // Flip immédiat
        Vector3 scaleFlip = new Vector3(-scaleActuel.x, scaleAbs.y, 1f);
        transform.localScale = scaleFlip;
        transform.position = posFixe;

        // Squeeze
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

        // Retour scale flippé
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

        effetEnCours = false; // reprend la rotation
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