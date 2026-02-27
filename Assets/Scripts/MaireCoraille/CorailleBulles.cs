using UnityEngine;
using System.Collections;

public class CorailleBulles : MonoBehaviour
{
    [Header("Sprites bulles (assigne ou génère par code)")]
    public Sprite spriteBulleGrosse;
    public Sprite spriteBulleMoyenne;
    public Sprite spriteBullePetite;

    [Header("Config")]
    public float hauteurMontee = 1.2f;  // distance montée
    public float dureeMontee = 1.0f;  // durée montée
    public float delaiEntreLoop = 0.3f;  // délai avant de relancer

    // Couleurs
    private Color couleurVert = new Color(0.3f, 0.9f, 0.4f, 0.9f);
    private Color couleurRouge = new Color(0.9f, 0.3f, 0.3f, 0.9f);

    // Les 3 bulles
    private SpriteRenderer bulleGrosse;
    private SpriteRenderer bulleMoyenne;
    private SpriteRenderer bullePetite;

    // Positions locales de départ
    private Vector3 posGrosseDepart = new Vector3(-0.3f, 0.4f, -0.1f);
    private Vector3 posMoyenneDepart = new Vector3(0.0f, 0.5f, -0.1f);
    private Vector3 posPetiteDepart = new Vector3(0.3f, 0.3f, -0.1f);

    private Coraille coraille;
    private bool estActif = false;

    void Awake()
    {
        coraille = GetComponentInParent<Coraille>();

        // Crée les 3 bulles
        bulleGrosse = CreerBulle("BulleGrosse", 0.28f, posGrosseDepart);
        bulleMoyenne = CreerBulle("BulleMoyenne", 0.20f, posMoyenneDepart);
        bullePetite = CreerBulle("BullePetite", 0.14f, posPetiteDepart);

        // Sprites
        if (spriteBulleGrosse != null) bulleGrosse.sprite = spriteBulleGrosse;
        if (spriteBulleMoyenne != null) bulleMoyenne.sprite = spriteBulleMoyenne;
        if (spriteBullePetite != null) bullePetite.sprite = spriteBullePetite;

        // Cache au départ
        SetAlpha(bulleGrosse, 0f);
        SetAlpha(bulleMoyenne, 0f);
        SetAlpha(bullePetite, 0f);
    }

    void Start()
    {
        StartCoroutine(LoopBulles());
    }

    // ── Boucle principale ─────────────────────────────────────────────────────
    IEnumerator LoopBulles()
    {
        while (true)
        {
            bool enCooldown = coraille != null && coraille.enCooldown;
            Color couleur = enCooldown ? couleurRouge : couleurVert;

            // Calcule les positions de départ en world space
            // (tient compte de la rotation du coraille)
            Vector3 posGrosseWorld = transform.TransformPoint(posGrosseDepart);
            Vector3 posMoyenneWorld = transform.TransformPoint(posMoyenneDepart);
            Vector3 posPetiteWorld = transform.TransformPoint(posPetiteDepart);

            StartCoroutine(AnimerBulle(bulleGrosse, posGrosseWorld, couleur, 0.0f));
            StartCoroutine(AnimerBulle(bulleMoyenne, posMoyenneWorld, couleur, 0.35f));
            StartCoroutine(AnimerBulle(bullePetite, posPetiteWorld, couleur, 0.65f));

            yield return new WaitForSeconds(dureeMontee + 0.65f + delaiEntreLoop);
        }
    }

    // ── Animation d'une bulle ─────────────────────────────────────────────────
    IEnumerator AnimerBulle(SpriteRenderer bulle, Vector3 posDepart,
                         Color couleur, float delai)
    {
        yield return new WaitForSeconds(delai);

        // Détache temporairement du parent pour bouger en world space
        bulle.transform.SetParent(null);
        bulle.transform.position = posDepart;
        bulle.transform.rotation = Quaternion.identity; // toujours droit
        bulle.color = new Color(couleur.r, couleur.g, couleur.b, 0f);

        // Monte toujours vers le HAUT de l'écran (world space)
        Vector3 posCible = posDepart + new Vector3(0f, hauteurMontee, 0f);

        float t = 0f;
        while (t < dureeMontee)
        {
            float progress = t / dureeMontee;

            bulle.transform.position = Vector3.Lerp(posDepart, posCible, progress);

            float alpha;
            if (progress < 0.2f)
                alpha = progress / 0.2f;
            else
                alpha = 1f - (progress - 0.2f) / 0.8f;

            bulle.color = new Color(couleur.r, couleur.g, couleur.b, alpha * 0.9f);

            t += Time.deltaTime;
            yield return null;
        }

        // Remet la bulle comme enfant du coraille
        bulle.transform.SetParent(transform);
        bulle.transform.localPosition = posDepart; // reset local
        SetAlpha(bulle, 0f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    SpriteRenderer CreerBulle(string nom, float taille, Vector3 posLocale)
    {
        GameObject go = new GameObject(nom);
        go.transform.SetParent(transform);
        go.transform.localPosition = posLocale;
        go.transform.localScale = new Vector3(taille, taille, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;

        // Génère un cercle vert par défaut
        sr.sprite = SpriteFactory.Creer("cercle", couleurVert);

        return sr;
    }

    void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}
