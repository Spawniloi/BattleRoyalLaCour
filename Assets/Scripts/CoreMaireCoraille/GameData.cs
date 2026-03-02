using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static int nombreJoueurs = 4;

    // Données par défaut — overridées par le Hub via JSON
    public static List<PlayerData> joueurs = new List<PlayerData>()
    {
        new PlayerData { playerID = 1, couleur = "#E63946", forme = "cercle",   objet = "casquette" },
        new PlayerData { playerID = 2, couleur = "#457B9D", forme = "carre",    objet = "lunettes"  },
        new PlayerData { playerID = 3, couleur = "#2A9D8F", forme = "triangle", objet = "couronne"  },
        new PlayerData { playerID = 4, couleur = "#E9C46A", forme = "etoile",   objet = "aucun"     },
    };

    // Retourne les données d'un joueur par son ID
    public static PlayerData GetJoueur(int playerID)
    {
        int idx = playerID - 1;
        if (idx >= 0 && idx < joueurs.Count)
            return joueurs[idx];
        return new PlayerData { playerID = playerID };
    }
}