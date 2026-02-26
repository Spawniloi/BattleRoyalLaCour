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
    public bool isFrozen = false;
    public bool isStunned = false;

    [Header("References")]
    public MaireGameManager gameManager;

    // Composants
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private InputHandler inputHandler;
    private RacailleVisuel visuel;

    // Mouvement avec effet glace
    private Vector2 currentVelocity;

    // ── Couleurs par défaut par joueur ────────────────────────────────────────
    private static readonly Color[] couleursDefaut = new Color[]
    {
        new Color(0.90f, 0.22f, 0.27f), // J1 rouge
        new Color(0.27f, 0.48f, 0.62f), // J2 bleu
        new Color(0.16f, 0.61f, 0.56f), // J3 vert
        new Color(0.91f, 0.77f, 0.41f), // J4 jaune
    };

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
        // Charge les données du Hub (ou défaut si Hub pas encore fait)
        PlayerData data = GameData.GetJoueur(playerID);
        visuel?.AppliquerData(data);
    }

    // ── Couleur ───────────────────────────────────────────────────────────────
    void AppliquerCouleurDefaut()
    {
        if (sr == null) return;
        int idx = Mathf.Clamp(playerID - 1, 0, couleursDefaut.Length - 1);
        sr.color = couleursDefaut[idx];
    }

    // Appelé depuis le Hub/JSON pour override la couleur
    public void SetCouleur(Color couleur)
    {
        if (sr != null) sr.color = couleur;
    }

    // ── Update ────────────────────────────────────────────────────────────────
    void Update()
    {
        if (isFrozen || isStunned) return;
    }

    void FixedUpdate()
    {
        if (isFrozen || isStunned)
        {
            rb.linearVelocity = Vector2.zero;
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
            // Accélération douce
            currentVelocity = Vector2.MoveTowards(
                currentVelocity,
                targetVelocity,
                config.iceAcceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // Décélération (glissement)
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
        visuel?.SetRoleVisuel(mayor);
        // TODO — changer sprite aileron/queue (étape suivante)
        Debug.Log($"[RacailleController] J{playerID} est maintenant " +
                  $"{(mayor ? "MAIRE (requin)" : "FUGITIF (poisson)")}");
    }

    // ── Freeze & Stun ─────────────────────────────────────────────────────────
    public void ApplyFreeze(float duration)
    {
        StartCoroutine(FreezeCoroutine(duration));
    }

    public void ApplyStun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    IEnumerator FreezeCoroutine(float duration)
    {
        isFrozen = true;
        currentVelocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        isFrozen = false;
    }

    IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        currentVelocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    // ── Collision avec autre racaille ─────────────────────────────────────────
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isMayor) return;

        RacailleController cible =
            collision.gameObject.GetComponent<RacailleController>();
        if (cible == null) return;
        if (cible == this) return;

        gameManager?.TenterTransfert(this, cible);
    }
}