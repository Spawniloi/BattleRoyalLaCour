using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int playerID = 1;
    public string couleurPeau = "#F4C89A"; // beige
    public string couleurDossard = "#E63946"; // rouge J1
    public int indexTete = 0;         // quelle tête

    // Rétrocompatibilité — utilisé par les corailles etc.
    public Color GetCouleur() => GetCouleurDossard();

    public Color GetCouleurPeau()
    {
        ColorUtility.TryParseHtmlString(couleurPeau, out Color c);
        return c;
    }

    public Color GetCouleurDossard()
    {
        ColorUtility.TryParseHtmlString(couleurDossard, out Color c);
        return c;
    }
}