using UnityEngine;
using System.Collections;

public class RacailleVisuel : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite spriteCorps;
    public Sprite spriteDossard;
    public Sprite spriteJambes;
    public Sprite[] spritesTetes;

    [Header("Sprites Role")]
    public SpriteRenderer srAileronRequin;
    public SpriteRenderer srTeteRequin;
    public SpriteRenderer srQueuePoisson;
    public SpriteRenderer srTetePoisson;

    [Header("Details Requin (couleur fixe)")]
    public SpriteRenderer srDetailsRequin;

    [Header("Renderers")]
    public SpriteRenderer srCorps;
    public SpriteRenderer srDossard;
    public SpriteRenderer srJambes;
    public SpriteRenderer srTete;
    public SpriteRenderer srBouee;

    [Header("Couleur jambes fixe")]
    public Color couleurJambes = new Color(0.1f, 0.15f, 0.3f);

    [Header("Animation rebond")]
    public float scaleRebond = 1.3f;
    public float dureeRebond = 0.15f;

    // ── Awake — cache tout par défaut ─────────────────────────────────────────
    void Awake()
    {
        if (srCorps != null) srCorps.enabled = false;
        if (srDossard != null) srDossard.enabled = false;
        if (srJambes != null) srJambes.enabled = false;
        if (srTete != null) srTete.enabled = false;
        if (srBouee != null) srBouee.enabled = false;
        if (srAileronRequin != null) srAileronRequin.enabled = false;
        if (srTeteRequin != null) srTeteRequin.enabled = false;
        if (srQueuePoisson != null) srQueuePoisson.enabled = false;
        if (srTetePoisson != null) srTetePoisson.enabled = false;
        if (srDetailsRequin != null) srDetailsRequin.enabled = false;
    }

    // ── Applique les données joueur ───────────────────────────────────────────
    public void AppliquerData(PlayerData data)
    {
        // Réactive les renderers principaux
        if (srCorps != null) srCorps.enabled = true;
        if (srDossard != null) srDossard.enabled = true;
        if (srJambes != null) srJambes.enabled = true;
        if (srTete != null) srTete.enabled = true;
        if (srBouee != null) srBouee.enabled = true;

        if (srCorps != null)
        {
            srCorps.sprite = spriteCorps;
            srCorps.color = data.GetCouleurPeau();
        }

        if (srDossard != null)
        {
            srDossard.sprite = spriteDossard;
            srDossard.color = data.GetCouleurDossard();
        }

        if (srJambes != null)
        {
            srJambes.sprite = spriteJambes;
            srJambes.color = couleurJambes;
        }

        if (srTete != null)
        {
            if (spritesTetes != null &&
                data.indexTete < spritesTetes.Length &&
                spritesTetes[data.indexTete] != null)
                srTete.sprite = spritesTetes[data.indexTete];
            srTete.color = data.GetCouleurDossard();
        }

        Color coul = data.GetCouleurDossard();
        if (srAileronRequin != null) srAileronRequin.color = coul;
        if (srTeteRequin != null) srTeteRequin.color = coul;
        if (srQueuePoisson != null) srQueuePoisson.color = coul;
        if (srTetePoisson != null) srTetePoisson.color = coul;
        if (srBouee != null) srBouee.color = coul;
    }

    // ── Visuel rôle ───────────────────────────────────────────────────────────
    public void SetRoleVisuel(bool isMayor)
    {
        PlayerData data = GameData.GetJoueur(
            GetComponent<RacailleController>().playerID);
        Color coul = data.GetCouleurDossard();

        if (srAileronRequin != null)
        {
            srAileronRequin.color = coul;
            srAileronRequin.enabled = isMayor;
        }

        if (srTeteRequin != null)
        {
            srTeteRequin.color = coul;
            srTeteRequin.enabled = isMayor;
        }

        if (srDetailsRequin != null)
            srDetailsRequin.enabled = isMayor;

        if (srQueuePoisson != null)
        {
            srQueuePoisson.color = coul;
            srQueuePoisson.enabled = !isMayor;
        }

        if (srTetePoisson != null)
        {
            srTetePoisson.color = coul;
            srTetePoisson.enabled = !isMayor;
        }
    }

    // ── Animation rebond ──────────────────────────────────────────────────────
    public void JouerRebond()
    {
        StopCoroutine("EffetRebond");
        StartCoroutine(EffetRebond());
    }

    IEnumerator EffetRebond()
    {
        // Scale up
        float t = 0f;
        while (t < dureeRebond / 2f)
        {
            float scale = Mathf.Lerp(1f, scaleRebond, t / (dureeRebond / 2f));
            transform.localScale = new Vector3(scale, scale, 1f);
            t += Time.deltaTime;
            yield return null;
        }

        // Scale down
        t = 0f;
        while (t < dureeRebond / 2f)
        {
            float scale = Mathf.Lerp(scaleRebond, 1f, t / (dureeRebond / 2f));
            transform.localScale = new Vector3(scale, scale, 1f);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }
}