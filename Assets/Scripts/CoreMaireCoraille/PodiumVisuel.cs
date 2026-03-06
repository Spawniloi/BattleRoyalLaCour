using UnityEngine;
using UnityEngine.UI;

public class PodiumVisuel : MonoBehaviour
{
    [Header("Renderers UI")]
    public Image imgCorps;
    public Image imgDossard;
    public Image imgTete;
    public Image imgJambes;
    public Image imgBouee;

    [Header("Sprites")]
    public Sprite spriteCorps;
    public Sprite spriteDossard;
    public Sprite spriteJambes;
    public Sprite[] spritesTetes;
    public Color couleurJambes = new Color(0.1f, 0.15f, 0.3f);

    public void AppliquerData(PlayerData data)
    {
        if (imgCorps != null)
        {
            imgCorps.sprite = spriteCorps;
            imgCorps.color = data.GetCouleurPeau();
        }

        if (imgDossard != null)
        {
            imgDossard.sprite = spriteDossard;
            imgDossard.color = data.GetCouleurDossard();
        }

        if (imgJambes != null)
        {
            imgJambes.sprite = spriteJambes;
            imgJambes.color = couleurJambes;
        }

        if (imgTete != null)
        {
            if (spritesTetes != null &&
                data.indexTete < spritesTetes.Length &&
                spritesTetes[data.indexTete] != null)
                imgTete.sprite = spritesTetes[data.indexTete];

            imgTete.color = data.GetCouleurDossard(); // ← remplace GetCouleurPeau()
        }

        if (imgBouee != null)
            imgBouee.color = data.GetCouleurDossard();
    }
}