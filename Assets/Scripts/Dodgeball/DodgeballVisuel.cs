using System.Net;
using UnityEngine;

public class DodgeballVisuel : MonoBehaviour
{
    [Header("Renderers")]
    public SpriteRenderer srCorps;
    public SpriteRenderer srDossard;
    public SpriteRenderer srJambes;
    public SpriteRenderer srTete;

    [Header("Sprites")]
    public Sprite spriteCorps;
    public Sprite spriteDossard;
    public Sprite spriteJambes;
    public Sprite[] spritesTetes;

    [Header("Couleur jambes fixe")]
    public Color couleurJambes = new Color(0.1f, 0.15f, 0.3f);

    [Header("Cercle selection")]
    public GameObject cercleSel;

    private bool dataAppliquee = false;

    void Start()
    {
        // Cache tout seulement si AppliquerData pas encore appelé
        if (!dataAppliquee)
        {
            if (srCorps != null) srCorps.enabled = false;
            if (srDossard != null) srDossard.enabled = false;
            if (srJambes != null) srJambes.enabled = false;
            if (srTete != null) srTete.enabled = false;
        }
        if (cercleSel != null) cercleSel.SetActive(false);
    }

    public void AppliquerData(PlayerData data)
    {
        if (data == null) return;
        dataAppliquee = true;

        // Corps
        if (srCorps != null)
        {
            srCorps.enabled = true;
            srCorps.sprite = spriteCorps;
            srCorps.color = data.GetCouleurPeau();
        }

        // Dossard
        if (srDossard != null)
        {
            srDossard.enabled = true;
            srDossard.sprite = spriteDossard;
            srDossard.color = data.GetCouleurDossard();
        }

        // Jambes
        if (srJambes != null)
        {
            srJambes.enabled = true;
            srJambes.sprite = spriteJambes;
            srJambes.color = couleurJambes;
        }

        // Tete
        if (srTete != null)
        {
            if (data.indexTete == 0)
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
                srTete.color = data.GetCouleurDossard();
            }
        }
    }

    public void SetControle(bool value)
    {
        if (cercleSel != null) cercleSel.SetActive(value);
    }
}
