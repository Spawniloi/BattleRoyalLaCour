using UnityEngine;

public class RacailleController : MonoBehaviour
{
    [Header("Config")]
    public MaireBalanceConfig config;
    public int playerID = 1;          // 1, 2, 3 ou 4
    public bool isMayor = false;      // requin ou poisson

    [Header("Etat")]
    public float sliderValue = 0f;    // score + modificateur vitesse
    public bool isFrozen = false;
    public bool isStunned = false;

    // Références
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // Input
    private Vector2 moveInput;

    // Vitesse courante
    private Vector2 currentVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isFrozen || isStunned) return;
        ReadInput();
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

    void ReadInput()
    {
        // Chaque joueur a ses propres axes — à connecter via Input System
        // Pour l'instant : clavier ZQSD pour J1, flèches pour J2
        string hAxis = playerID == 1 ? "Horizontal" : "Horizontal2";
        string vAxis = playerID == 1 ? "Vertical" : "Vertical2";
        moveInput = new Vector2(Input.GetAxis(hAxis), Input.GetAxis(vAxis));
    }

    void MoveWithIce()
    {
        float speed = config.GetSpeedFromSlider(sliderValue);

        // Effet glace : accélération et décélération progressives
        Vector2 targetVelocity = moveInput * speed;

        if (moveInput.magnitude > 0.1f)
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
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.rotation.eulerAngles.z;
            float newAngle = Mathf.LerpAngle(currentAngle, angle,
                             config.iceTurnSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
    }

    // Appelé par le GameManager toutes les mayorTimeTickRate secondes
    public void UpdateSlider(bool currentlyMayor)
    {
        if (currentlyMayor)
            sliderValue += config.sliderRiseRate * config.mayorTimeTickRate;
        else
            sliderValue -= config.sliderFallRate * config.mayorTimeTickRate;
    }

    public void ApplyFreeze(float duration)
    {
        StartCoroutine(FreezeCoroutine(duration));
    }

    public void ApplyStun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    private System.Collections.IEnumerator FreezeCoroutine(float duration)
    {
        isFrozen = true;
        currentVelocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        isFrozen = false;
    }

    private System.Collections.IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        currentVelocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    public void SetMayor(bool mayor)
    {
        isMayor = mayor;
        // TODO Étape suivante : changer sprite aileron/queue
    }
}