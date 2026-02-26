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
    }

    // ── Update ────────────────────────────────────────────────────────────────
    void Update()
    {
        if (transfertCooldown > 0)
            transfertCooldown -= Time.deltaTime;
    }

    void FixedUpdate()
    {
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
            sliderValue += config.sliderRiseRate * config.mayorTimeTickRate;
        else
            sliderValue -= config.sliderFallRate * config.mayorTimeTickRate;
    }

    // ── Rôle Maire / Fugitif ──────────────────────────────────────────────────
    public void SetMayor(bool mayor)
    {
        isMayor = mayor;

        if (mayor)
            transfertCooldown = config.transfertCooldownDuree;
        else
        {
            transfertCooldown = 0f;
            StartCoroutine(FreezeInputs(config.freezeDuration));
        }

        visuel?.SetRoleVisuel(mayor);
        Debug.Log($"[RacailleController] J{playerID} → " +
                  $"{(mayor ? "MAIRE" : "FUGITIF")}");
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
        RacailleController autre =
            collision.gameObject.GetComponent<RacailleController>();
        if (autre == null) return;
        if (autre == this) return;

        // Maire touche poisson → transfert
        if (isMayor && !autre.isMayor)
        {
            if (isFrozen || isStunned || transfertCooldown > 0) return;
            transfertCooldown = config.transfertCooldownDuree;
            gameManager?.TenterTransfert(this, autre);
            return;
        }

        // Poisson touche poisson → knockback léger
        if (!isMayor && !autre.isMayor)
        {
            Vector2 dir = (autre.transform.position
                         - transform.position).normalized;
            if (dir == Vector2.zero)
                dir = Random.insideUnitCircle.normalized;

            rb.AddForce(-dir * config.knockbackPoissonPoisson, ForceMode2D.Impulse);
            SyncVelocity(rb.linearVelocity);
        }
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