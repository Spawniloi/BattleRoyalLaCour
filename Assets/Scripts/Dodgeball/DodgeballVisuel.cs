using System.Collections;
using UnityEngine;

public class DodgeballVisuel : MonoBehaviour
{
    [Header("Renderers")]
    public SpriteRenderer srCorps;
    public SpriteRenderer srDossard;
    public SpriteRenderer srJambes;
    public SpriteRenderer srTete;
    public SpriteRenderer srCercle;

    [Header("Sprites fixes")]
    public Sprite spriteCorps;
    public Sprite spriteDossard;
    public Sprite spriteJambes;

    [Header("Sprites tetes")]
    public Sprite[] spritesTetes;

    [Header("Animation marche — 4 frames")]
    public Sprite[] framesMarche;
    public float vitesseAnim = 8f;

    [Header("Couleur jambes fixe")]
    public Color couleurJambes = new Color(0.1f, 0.15f, 0.3f);

    [Header("Cercle selection")]
    [Range(0f, 1f)]
    public float alphaCercle = 0.4f;
    public float scaleCercle = 2f;

    [Header("Knockback + rebond")]
    public float forceKnockback = 12f;
    public float dureeKnockback = 0.25f;
    public float scaleRebond = 1.4f;
    public float dureeRebond = 0.15f;

    private Rigidbody2D rb;
    private Collider2D col;
    private PlayerController pc;
    private bool dataAppliquee = false;
    private Color couleurDossard = Color.white;
    private Color couleurPeau = Color.white;
    private int frameActuelle = 0;
    private float tempsFrame = 0f;
    private Vector3 lastPosition;

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        col = GetComponentInParent<Collider2D>();
        pc = GetComponentInParent<PlayerController>();
    }

    void Start()
    {
        lastPosition = transform.position;
        if (!dataAppliquee)
        {
            if (srCorps != null) srCorps.enabled = false;
            if (srDossard != null) srDossard.enabled = false;
            if (srJambes != null) srJambes.enabled = false;
            if (srTete != null) srTete.enabled = false;
        }

        if (srCercle != null)
        {
            srCercle.enabled = false;
            srCercle.transform.localScale =
                new Vector3(scaleCercle, scaleCercle, 1f);
        }
    }

    void Update()
    {
        AnimerMarche();
    }

    // ── Animation marche — utilise moveInput du PlayerController ─────────────
    void AnimerMarche()
    {
        if (srCorps == null) return;
        if (framesMarche == null || framesMarche.Length == 0) return;

        // Détecte mouvement par différence de position — marche pour AI et joueur
        float deplacement = Vector3.Distance(
            transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        bool bouge = deplacement > 0.1f;

        // Si c'est un joueur controlé — utilise moveInput
        if (pc != null && GetComponentInParent<Unit>()?.isControlled == true)
            bouge = pc.moveInput.magnitude > 0.1f;

        if (bouge)
        {
            tempsFrame += Time.deltaTime * vitesseAnim;
            if (tempsFrame >= 1f)
            {
                tempsFrame -= 1f;
                frameActuelle = (frameActuelle + 1) % framesMarche.Length;
                if (framesMarche[frameActuelle] != null)
                    srCorps.sprite = framesMarche[frameActuelle];
            }
        }
        else
        {
            frameActuelle = 0;
            tempsFrame = 0f;
            if (framesMarche[0] != null)
                srCorps.sprite = framesMarche[0];
        }
    }

    // ── Applique données joueur ───────────────────────────────────────────────
    public void AppliquerData(PlayerData data)
    {
        if (data == null) return;
        dataAppliquee = true;
        couleurDossard = data.GetCouleurDossard();
        couleurPeau = data.GetCouleurPeau();

        if (srCorps != null)
        {
            srCorps.enabled = true;
            if (framesMarche != null &&
                framesMarche.Length > 0 &&
                framesMarche[0] != null)
                srCorps.sprite = framesMarche[0];
            else if (spriteCorps != null)
                srCorps.sprite = spriteCorps;
            srCorps.color = couleurPeau;
        }

        if (srDossard != null)
        {
            srDossard.enabled = true;
            if (spriteDossard != null)
                srDossard.sprite = spriteDossard;
            srDossard.color = couleurDossard;
        }

        if (srJambes != null)
        {
            srJambes.enabled = true;
            if (spriteJambes != null)
                srJambes.sprite = spriteJambes;
            srJambes.color = couleurJambes;
        }

        if (srTete != null)
        {
            if (data.indexTete <= 0)
            {
                srTete.enabled = false;
            }
            else
            {
                srTete.enabled = true;
                if (spritesTetes != null &&
                    data.indexTete < spritesTetes.Length &&
                    spritesTetes[data.indexTete] != null)
                    srTete.sprite = spritesTetes[data.indexTete];
                srTete.color = couleurDossard;
            }
        }

        if (srCercle != null)
        {
            Color c = couleurDossard;
            c.a = alphaCercle;
            srCercle.color = c;
            srCercle.enabled = false;
            srCercle.transform.localScale =
                new Vector3(scaleCercle, scaleCercle, 1f);
        }
    }

    // ── Cercle selection ──────────────────────────────────────────────────────
    public void SetControle(bool value)
    {
        if (srCercle == null) return;
        srCercle.enabled = value;
        if (value) StartCoroutine(PopCercle());
    }

    IEnumerator PopCercle()
    {
        float cible = scaleCercle;
        float grand = scaleCercle * 1.3f;
        float t = 0f;
        float duree = 0.12f;

        while (t < duree / 2f)
        {
            float s = Mathf.Lerp(cible, grand, t / (duree / 2f));
            if (srCercle != null)
                srCercle.transform.localScale = new Vector3(s, s, 1f);
            t += Time.deltaTime;
            yield return null;
        }
        t = 0f;
        while (t < duree / 2f)
        {
            float s = Mathf.Lerp(grand, cible, t / (duree / 2f));
            if (srCercle != null)
                srCercle.transform.localScale = new Vector3(s, s, 1f);
            t += Time.deltaTime;
            yield return null;
        }
        if (srCercle != null)
            srCercle.transform.localScale = new Vector3(cible, cible, 1f);
    }

    // ── Rebond scale ──────────────────────────────────────────────────────────
    public void JouerRebond()
    {
        StopCoroutine("EffetRebond");
        StartCoroutine(EffetRebond());
    }

    IEnumerator EffetRebond()
    {
        float t = 0f;
        while (t < dureeRebond / 2f)
        {
            float s = Mathf.Lerp(1f, scaleRebond, t / (dureeRebond / 2f));
            transform.localScale = new Vector3(s, s, 1f);
            t += Time.deltaTime;
            yield return null;
        }
        t = 0f;
        while (t < dureeRebond / 2f)
        {
            float s = Mathf.Lerp(scaleRebond, 1f, t / (dureeRebond / 2f));
            transform.localScale = new Vector3(s, s, 1f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    // ── Knockback + flash + collider off ─────────────────────────────────────
    public void JouerKnockback(Vector2 direction)
    {
        if (rb == null) return;
        JouerRebond();
        StartCoroutine(EffetKnockback(direction));
    }

    IEnumerator EffetKnockback(Vector2 direction)
    {
        // Désactive collider pendant knockback
        if (col != null) col.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * forceKnockback,
                    ForceMode2D.Impulse);

        float t = 0f;
        while (t < dureeKnockback)
        {
            float p = t / dureeKnockback;
            if (srCorps != null)
                srCorps.color = Color.Lerp(Color.white, couleurPeau, p);
            if (srDossard != null)
                srDossard.color = Color.Lerp(Color.white, couleurDossard, p);
            if (srJambes != null)
                srJambes.color = Color.Lerp(Color.white, couleurJambes, p);
            t += Time.deltaTime;
            yield return null;
        }

        // Remet couleurs
        if (srCorps != null) srCorps.color = couleurPeau;
        if (srDossard != null) srDossard.color = couleurDossard;
        if (srJambes != null) srJambes.color = couleurJambes;

        // Réactive collider
        if (col != null) col.enabled = true;
    }
}