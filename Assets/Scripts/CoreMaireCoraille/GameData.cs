using System.Collections.Generic;

public static class GameData
{
    // ── Joueurs ───────────────────────────────────────────────────────────────
    public static int nombreJoueurs = 4;

    public static List<PlayerData> joueurs = new List<PlayerData>()
    {
        new PlayerData { playerID=1, couleurPeau="#F4C89A",
                         couleurDossard="#E63946", indexTete=0 },
        new PlayerData { playerID=2, couleurPeau="#F4C89A",
                         couleurDossard="#457B9D", indexTete=1 },
        new PlayerData { playerID=3, couleurPeau="#F4C89A",
                         couleurDossard="#2A9D8F", indexTete=2 },
        new PlayerData { playerID=4, couleurPeau="#F4C89A",
                         couleurDossard="#E9C46A", indexTete=3 },
    };

    public static PlayerData GetJoueur(int playerID)
    {
        int idx = playerID - 1;
        if (idx >= 0 && idx < joueurs.Count)
            return joueurs[idx];
        return new PlayerData { playerID = playerID };
    }

    // ── Partie en cours ───────────────────────────────────────────────────────
    public static string jeuActuel = "MaireCoraille";
    public static PartieData dernierePartie = null;

    // ── Progression globale ───────────────────────────────────────────────────
    public static int sucettesOr = 0;

    // ── Historique toutes parties ─────────────────────────────────────────────
    public static List<PartieData> historique = new List<PartieData>();

    public static void AjouterPartie(PartieData partie)
    {
        dernierePartie = partie;
        historique.Add(partie);
    }
}