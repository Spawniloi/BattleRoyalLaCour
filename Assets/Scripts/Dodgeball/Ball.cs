using System.Collections;

using UnityEngine;


public class Ball : MonoBehaviour
{
    [Header("Vitesse")]
    public float speed = 3f;
    public float throwSpeed = 12f;

    [Header("Etat")]
    public bool isHeld = false;
    public bool isThrown = false;
    public int OwnerTeam = -1;

    private Vector2 direction;
    private TrailRenderer trail;
    private SpriteRenderer sr;
    private Coroutine clignotement;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Trail — récupère ou crée
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
            trail = gameObject.AddComponent<TrailRenderer>();

        trail.time = 0.3f;
        trail.startWidth = 0.5f;
        trail.endWidth = 0f;
        trail.sortingOrder = 10;
        trail.startColor = new Color(1f, 0f, 0f, 1f);
        trail.endColor = new Color(1f, 0f, 0f, 0f);
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.sortingOrder = -1; // ← en dessous du ballon
        trail.enabled = false;
    }

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        if (isHeld) return;
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);
        CheckBounds();
    }

    void CheckBounds()
    {
        float limit = 20f;
        if (Mathf.Abs(transform.position.x) > limit ||
            Mathf.Abs(transform.position.y) > limit)
            Destroy(gameObject);
    }

    // ── Trigger ───────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Unit unit = collision.GetComponent<Unit>();
        if (unit == null) return;

        if (isThrown && unit.isAlive)
        {
            if (unit.teamID == OwnerTeam) return;
            HitUnit(unit);
            return;
        }

        if (!isThrown && !isHeld && !unit.HasBall)
            PickUp(unit);
    }

    // ── Ramassage ─────────────────────────────────────────────────────────────
    private void PickUp(Unit unit)
    {
        // Arrête clignotement
        if (clignotement != null) StopCoroutine(clignotement);
        if (sr != null) sr.color = Color.white;

        isHeld = true;
        unit.heldBall = this;

        // Désactive trail
        if (trail != null)
        {
            trail.enabled = false;
            trail.Clear();
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        transform.SetParent(unit.transform);
        transform.localPosition = new Vector3(0.5f, 0, 0);

        if (GameManager.Instance.teamScores.ContainsKey(unit.teamID))
            GameManager.Instance.teamScores[unit.teamID].ballsPicked++;
    }

    // ── Lancer ────────────────────────────────────────────────────────────────
    internal void Throw(Vector2 dir)
    {
        isHeld = false;
        isThrown = true;

        transform.SetParent(null);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.simulated = true;
        rb.linearVelocity = dir.normalized * throwSpeed;

        // Active trail
        if (trail != null)
        {
            trail.Clear();
            trail.enabled = true;
        }

        // Lance clignotement rouge/blanc
        if (clignotement != null) StopCoroutine(clignotement);
        clignotement = StartCoroutine(Clignoter());
    }

    IEnumerator Clignoter()
    {
        if (sr == null) yield break;
        while (true)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.08f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
        }
    }

    // ── Impact ────────────────────────────────────────────────────────────────
    void HitUnit(Unit unit)
    {
        // Arrête clignotement
        if (clignotement != null) StopCoroutine(clignotement);

        // Knockback + rebond visuel
        Vector2 knockDir = (unit.transform.position
                          - transform.position).normalized;
        DodgeballVisuel visuel =
            unit.GetComponentInChildren<DodgeballVisuel>();
        visuel?.JouerKnockback(knockDir);

        // Score
        if (GameManager.Instance.teamScores.ContainsKey(OwnerTeam))
            GameManager.Instance.teamScores[OwnerTeam].eliminations++;

        // Mort après knockback
        unit.StartCoroutine(MourirApresKnockback(unit));

        Destroy(gameObject);
    }

    IEnumerator MourirApresKnockback(Unit unit)
    {
        yield return new WaitForSeconds(0.25f);
        if (unit != null) unit.Die();
    }
}
