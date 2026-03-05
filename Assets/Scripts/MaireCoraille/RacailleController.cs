using UnityEngine;
using System.Collections;

public class RacailleController : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;
    public int playerID = 1;

    [Header("Etat")]
    public bool isMayor = false;
    public float sliderValue = 0f;
    public bool isFrozen = false;  // bloque les inputs seulement
    public bool isStunned = false;  // bloque les inputs seulement

    [Header("Dash")]
    public int munitionsDash = 0;
    private float dashCooldownActuel = 0f;
    private bool dashEnCours = false;

    [Header("References")]
    public MaireGameManager gameManager;

    // Composants
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private InputHandler inputHandler;
    private RacailleVisuel visuel;
    private float transfertCooldown = 0f;

    // Mouvement avec effet glace
    private Vector2 currentVelocity;
    private bool etaitEnMouvement = false;

    // ── Awake ─────────────────────────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        inputHandler = GetComponent<InputHandler>();
        gameManager = FindFirstObjectByType<MaireGameManager>();
        visuel = GetComponent<RacailleVisuel>();
    }

    void Start()
    {
        PlayerData data = GameData.GetJoueur(playerID);
        visuel?.AppliquerData(data);

        // Applique le rôle APRES les données visuelles
        // isMayor est déjà assigné par le GameManager avant Start()
        visuel?.SetRoleVisuel(isMayor);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    void Update()
    {
        if (transfertCooldown > 0)
            transfertCooldown -= Time.deltaTime;

        if (dashCooldownActuel > 0)
            dashCooldownActuel -= Time.deltaTime;

        // Dash — seulement si maire + munitions + pas en cooldown
        if (isMayor
            && !isFrozen
            && !isStunned
            && !dashEnCours
            && dashCooldownActuel <= 0
            && munitionsDash > 0
            && inputHandler != null
            && inputHandler.DashPressed)
        {
            StartCoroutine(EffectuerDash());
        }
    }

    void FixedUpdate()
    {
        bool bouge = currentVelocity.magnitude > 0.3f;
        if (bouge != etaitEnMouvement)
        {
            MaireAudioManager.Instance?.SetJoueurEnMouvement(bouge);
            etaitEnMouvement = bouge;
        }
        // Freeze/Stun bloquent les INPUTS mais pas le Rigidbody
        // → le knockback physique fonctionne toujours
        if (isFrozen || isStunned)
        {
            // On laisse le Rigidbody glisser naturellement
            // Juste on décélère progressivement (effet glace post-knockback)
            currentVelocity = rb.linearVelocity;
            return;
        }

        MoveWithIce();
    }

    // ── Mouvement effet glace ─────────────────────────────────────────────────
    void MoveWithIce()
    {
        if (inputHandler == null) return;

        float speed = config.GetSpeedFromSlider(sliderValue);
        Vector2 targetVelocity = inputHandler.MoveInput * speed;

        if (inputHandler.MoveInput.magnitude > 0.1f)
        {
            currentVelocity = Vector2.MoveTowards(
                currentVelocity,
                targetVelocity,
                config.iceAcceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            currentVelocity = Vector2.MoveTowards(
                currentVelocity,
                Vector2.zero,
                config.iceDeceleration * Time.fixedDeltaTime
            );
        }

        rb.linearVelocity = currentVelocity;

        // Rotation vers la direction de mouvement
        if (currentVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x)
                        * Mathf.Rad2Deg - 90f;
            float current = transform.rotation.eulerAngles.z;
            float newAngle = Mathf.LerpAngle(
                current, angle,
                config.iceTurnSpeed * Time.fixedDeltaTime
            );
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
    }

    // ── Slider ────────────────────────────────────────────────────────────────
    public void UpdateSlider(bool estMaire)
    {
        if (estMaire)
            sliderValue -= config.sliderRiseRate * config.mayorTimeTickRate; // descend
        else
            sliderValue += config.sliderFallRate * config.mayorTimeTickRate;  // monte
    }

    // ── Rôle Maire / Fugitif ──────────────────────────────────────────────────
    public void SetMayor(bool mayor)
    {
        isMayor = mayor;

        if (mayor)
        {
            transfertCooldown = config.transfertCooldownDuree;
        }
        else
        {
            transfertCooldown = 0f;
            munitionsDash = 0; // perd ses munitions en devenant poisson
            dashCooldownActuel = 0f;
            StartCoroutine(FreezeInputs(config.freezeDuration));
        }

        visuel?.SetRoleVisuel(mayor);
    }

    // ── Freeze inputs seulement (le Rigidbody continue de bouger) ─────────────
    public void ApplyFreeze(float duration)
    {
        StartCoroutine(FreezeInputs(duration));
    }

    public void ApplyStun(float duration)
    {
        StartCoroutine(StunInputs(duration));
    }

    IEnumerator FreezeInputs(float duration)
    {
        isFrozen = true;
        yield return new WaitForSeconds(duration);
        isFrozen = false;
        // Sync currentVelocity avec le vrai Rigidbody après le knockback
        currentVelocity = rb.linearVelocity;
    }

    IEnumerator StunInputs(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
        currentVelocity = rb.linearVelocity;
    }

    // ── Collision avec autre racaille ─────────────────────────────────────────
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Rebond contre un coraille
        if (collision.gameObject.GetComponent<Coraille>() != null)
        {
            visuel?.JouerRebond();
            return;
        }

        // Rebond contre un autre joueur
        RacailleController autre =
            collision.gameObject.GetComponent<RacailleController>();
        if (autre == null) return;

        visuel?.JouerRebond();
        autre.visuel?.JouerRebond();

        // Transfert — seulement côté MAIRE pour éviter le double appel
        if (isMayor && !autre.isMayor && transfertCooldown <= 0)
        {
            gameManager?.TenterTransfert(this, autre);
            return;
        }

        // Knockback poisson-poisson — seulement si aucun des 2 n'est maire
        if (!isMayor && !autre.isMayor)
        {
            Vector2 dir = ((Vector2)transform.position
                                 - (Vector2)autre.transform.position).normalized;
            Rigidbody2D rbAutre = autre.GetComponent<Rigidbody2D>();

            rb.AddForce(dir * config.knockbackPoissonPoisson,
                        ForceMode2D.Impulse);
            rbAutre.AddForce(-dir * config.knockbackPoissonPoisson,
                             ForceMode2D.Impulse);

            SyncVelocity(rb.linearVelocity);
            autre.SyncVelocity(rbAutre.linearVelocity);
        }
    }

    IEnumerator EffectuerDash()
    {
        dashEnCours = true;
        munitionsDash--;
        dashCooldownActuel = config.dashCooldown;
        MaireAudioManager.Instance?.JouerDash();

        // Direction du dash
        Vector2 dirDash = currentVelocity.normalized;
        if (dirDash == Vector2.zero && inputHandler.MoveInput != Vector2.zero)
            dirDash = inputHandler.MoveInput.normalized;
        if (dirDash == Vector2.zero)
            dirDash = Vector2.right;

        // Impulsion dash — on AJOUTE à la vitesse actuelle
        rb.AddForce(dirDash * config.dashForce, ForceMode2D.Impulse);
        currentVelocity += dirDash * config.dashForce;

        // Lance les bulles de dash
        StartCoroutine(BullesDash(dirDash));

        Debug.Log($"[Dash] J{playerID} ! Munitions : {munitionsDash}");

        yield return new WaitForSeconds(config.dashDuree);
        dashEnCours = false;
    }

    IEnumerator BullesDash(Vector2 dirDash)
    {
        // Direction opposée au dash = derrière le joueur
        Vector2 dirBulle = -dirDash;

        for (int i = 0; i < config.dashBulleNombre; i++)
        {
            // Crée une bulle derrière le joueur
            GameObject bulle = new GameObject("BulleDash");
            bulle.transform.position = (Vector2)transform.position
                                     + dirBulle * 0.3f
                                     + Random.insideUnitCircle * 0.15f;

            SpriteRenderer sr = bulle.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Creer("cercle",
                                 new Color(1f, 1f, 1f, 0.8f));
            sr.sortingOrder = 3;

            float taille = config.dashBulleTaille
                         * Random.Range(0.7f, 1.3f);
            bulle.transform.localScale = new Vector3(taille, taille, 1f);

            // Anime la bulle
            StartCoroutine(AnimerBulleDash(bulle, dirBulle));

            yield return new WaitForSeconds(config.dashBulleIntervalle);
        }
    }

    IEnumerator AnimerBulleDash(GameObject bulle, Vector2 direction)
    {
        if (bulle == null) yield break;

        SpriteRenderer sr = bulle.GetComponent<SpriteRenderer>();
        Vector3 pos = bulle.transform.position;
        float t = 0f;

        while (t < config.dashBulleduree)
        {
            if (bulle == null) yield break;

            float progress = t / config.dashBulleduree;

            // Monte légèrement et s'estompe
            bulle.transform.position = pos + new Vector3(
                direction.x * 0.3f * progress,
                direction.y * 0.3f * progress + 0.2f * progress,
                0f
            );

            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - progress;
                sr.color = c;
            }

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(bulle);
    }

    // Appelé quand on ramasse un item
    public void AjouterMunitionDash()
    {
        munitionsDash++;
        MaireAudioManager.Instance?.JouerCollecteDash();
        Debug.Log($"[Dash] J{playerID} ramasse une munition ! Total : {munitionsDash}");
    }

    public void StopFreeze()
    {
        StopAllCoroutines();
        isFrozen = false;
        isStunned = false;
        // Relance la rotation si nécessaire
        StartCoroutine(FreezeInputs(0f));
    }


    // Appelé depuis le Hub/JSON
    public void SetCouleur(Color couleur)
    {
        if (sr != null) sr.color = couleur;
    }
    public void SyncVelocity(Vector2 nouvelleVelocity)
    {
        currentVelocity = nouvelleVelocity;
    }
}