using UnityEngine;

public class RacailleVisuel : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite spriteCorps;
    public Sprite spriteDossard;
    public Sprite spriteJambes;
    public Sprite[] spritesTetes; // 6 têtes

    [Header("Sprites Role")]
    public SpriteRenderer srAileronRequin;
    public SpriteRenderer srTeteRequin;
    public SpriteRenderer srQueuePoisson;
    public SpriteRenderer srTetePoisson;   // enfant Queue

    [Header("Renderers")]
    public SpriteRenderer srCorps;
    public SpriteRenderer srDossard;
    public SpriteRenderer srJambes;
    public SpriteRenderer srTete;
    

    [Header("Couleur jambes fixe")]
    public Color couleurJambes = new Color(0.1f, 0.15f, 0.3f); // bleu marine

    // ── Applique les données joueur ───────────────────────────────────────────
    public void AppliquerData(PlayerData data)
    {
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

        // Couleur dossard sur les 4 parties rôle
        Color coul = data.GetCouleurDossard();
        if (srAileronRequin != null) srAileronRequin.color = coul;
        if (srTeteRequin != null) srTeteRequin.color = coul;
        if (srQueuePoisson != null) srQueuePoisson.color = coul;
        if (srTetePoisson != null) srTetePoisson.color = coul;

        // NE PAS appeler SetRoleVisuel ici — le GameManager s'en charge
    }

    // ── Visuel rôle ───────────────────────────────────────────────────────────
    public void SetRoleVisuel(bool isMayor)
    {
        PlayerData data = GameData.GetJoueur(
            GetComponent<RacailleController>().playerID);
        Color coul = data.GetCouleurDossard();

        // Requin — aileron + tête requin
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

        // Poisson — queue + tête poisson
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
}