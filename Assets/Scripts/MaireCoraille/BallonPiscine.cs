using UnityEngine;
using System.Collections;

public class BallonPiscine : MonoBehaviour
{
    [Header("Ballon")]
    public SpriteRenderer srBallon;
    public float hauteurArc = 0.4f;
    public float dureeVol = 2.0f;
    public float vitesseRotation = 180f;

    [Header("Timing")]
    public float delaiAttenteMin = 8f;
    public float delaiAttenteMax = 20f;
    public int nbLancersMax = 4;

    [Header("Scale arc")]
    public float scaleMin = 0.8f;
    public float scaleMax = 1.0f;

    [Header("Enfant A — prefab + rails")]
    public Transform enfantA;
    public Transform enfantA_Point1;
    public Transform enfantA_Point2;
    public float vitesseEnfantA = 1.5f;

    [Header("Enfant B — prefab + rails")]
    public Transform enfantB;
    public Transform enfantB_Point1;
    public Transform enfantB_Point2;
    public float vitesseEnfantB = 1.5f;

    private Vector3 posEnfantA;
    private Vector3 posEnfantB;
    private bool enfantAVersPoint2 = true;
    private bool enfantBVersPoint2 = false;
    private bool enfantAEnPause = false;
    private bool enfantBEnPause = false;
    private bool partieActive = false;
    private int nbLancers = 0;
    private bool versB = true;

    void Start()
    {
        if (srBallon != null) srBallon.enabled = false;

        // Position initiale des enfants
        if (enfantA != null && enfantA_Point1 != null)
            posEnfantA = enfantA_Point1.position;
        if (enfantB != null && enfantB_Point1 != null)
            posEnfantB = enfantB_Point1.position;

        StartCoroutine(AttendreDemarrage());
    }

    void Update()
    {
        if (!partieActive) return;

        // ── Enfant A se promène ───────────────────────────────────────────────
        if (enfantA != null &&
            enfantA_Point1 != null &&
            enfantA_Point2 != null &&
            !enfantAEnPause)
        {
            Vector3 cible = enfantAVersPoint2
                ? enfantA_Point2.position
                : enfantA_Point1.position;

            posEnfantA = Vector3.MoveTowards(
                posEnfantA, cible,
                vitesseEnfantA * Time.deltaTime);

            enfantA.position = posEnfantA;

            if (Vector3.Distance(posEnfantA, cible) < 0.05f)
            {
                enfantAVersPoint2 = !enfantAVersPoint2;
                StartCoroutine(PauseEnfant(true));
            }
        }

        // ── Enfant B se promène ───────────────────────────────────────────────
        if (enfantB != null &&
            enfantB_Point1 != null &&
            enfantB_Point2 != null &&
            !enfantBEnPause)
        {
            Vector3 cible = enfantBVersPoint2
                ? enfantB_Point2.position
                : enfantB_Point1.position;

            posEnfantB = Vector3.MoveTowards(
                posEnfantB, cible,
                vitesseEnfantB * Time.deltaTime);

            enfantB.position = posEnfantB;

            if (Vector3.Distance(posEnfantB, cible) < 0.05f)
            {
                enfantBVersPoint2 = !enfantBVersPoint2;
                StartCoroutine(PauseEnfant(false));
            }
        }
    }

    IEnumerator PauseEnfant(bool estA)
    {
        if (estA) enfantAEnPause = true;
        else enfantBEnPause = true;

        yield return new WaitForSeconds(Random.Range(0.5f, 2.5f));

        if (estA)
        {
            vitesseEnfantA = Random.Range(1.0f, 2.5f);
            enfantAEnPause = false;
        }
        else
        {
            vitesseEnfantB = Random.Range(1.0f, 2.5f);
            enfantBEnPause = false;
        }
    }

    IEnumerator AttendreDemarrage()
    {
        MaireGameManager gm = null;
        while (gm == null || !gm.partieEnCours)
        {
            gm = FindFirstObjectByType<MaireGameManager>();
            yield return new WaitForSeconds(0.5f);
        }

        partieActive = true;
        StartCoroutine(BoucleBallon());
    }

    IEnumerator BoucleBallon()
    {
        while (partieActive && nbLancers < nbLancersMax)
        {
            float delai = Random.Range(delaiAttenteMin, delaiAttenteMax);
            yield return new WaitForSeconds(delai);

            MaireGameManager gm = FindFirstObjectByType<MaireGameManager>();
            if (gm == null || !gm.partieEnCours) yield break;

            yield return StartCoroutine(LancerBallon());
            nbLancers++;
            versB = !versB;
        }

        if (srBallon != null) srBallon.enabled = false;
    }

    IEnumerator LancerBallon()
    {
        Vector3 depart = versB ? posEnfantA : posEnfantB;
        Vector3 arrivee = versB ? posEnfantB : posEnfantA;

        if (srBallon != null)
        {
            srBallon.enabled = true;
            srBallon.transform.position = depart;
        }

        float t = 0f;
        while (t < dureeVol)
        {
            float progress = t / dureeVol;
            Vector3 pos = Vector3.Lerp(depart, arrivee, progress);

            // Arc léger en Y
            pos.y += Mathf.Sin(progress * Mathf.PI) * hauteurArc;

            // Scale profondeur
            float scale = Mathf.Lerp(scaleMax, scaleMin,
                          Mathf.Sin(progress * Mathf.PI));

            if (srBallon != null)
            {
                srBallon.transform.position = pos;
                srBallon.transform.localScale =
                    new Vector3(scale, scale, 1f);
                srBallon.transform.Rotate(
                    0, 0, vitesseRotation * Time.deltaTime);
            }

            t += Time.deltaTime;
            yield return null;
        }

        if (srBallon != null)
        {
            srBallon.transform.position = arrivee;
            srBallon.transform.localScale = Vector3.one;
            yield return StartCoroutine(SquishArrivee());
            srBallon.enabled = false;
        }
    }

    IEnumerator SquishArrivee()
    {
        float t = 0f, duree = 0.15f;
        while (t < duree)
        {
            float p = t / duree;
            float sx = Mathf.Lerp(1.3f, 1.0f, p);
            float sy = Mathf.Lerp(0.7f, 1.0f, p);
            srBallon.transform.localScale = new Vector3(sx, sy, 1f);
            t += Time.deltaTime;
            yield return null;
        }
        srBallon.transform.localScale = Vector3.one;
    }

    public void ArreterBallon()
    {
        partieActive = false;
        StopAllCoroutines();
        if (srBallon != null) srBallon.enabled = false;
    }
}