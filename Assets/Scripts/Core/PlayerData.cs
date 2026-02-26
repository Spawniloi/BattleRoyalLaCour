using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int playerID = 1;
    public string couleur = "#E63946"; // rouge par défaut J1
    public string forme = "cercle";  // cercle, carre, triangle, etoile, losange
    public string objet = "aucun";   // casquette, lunettes, couronne, etc.

    // Convertit la couleur hex en Color Unity
    public Color GetCouleur()
    {
        Color c;
        ColorUtility.TryParseHtmlString(couleur, out c);
        return c;
    }
}