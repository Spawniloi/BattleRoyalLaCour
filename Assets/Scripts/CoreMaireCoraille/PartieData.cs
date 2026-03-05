using System;
using System.Collections.Generic;

[Serializable]
public class PartieData
{
    public string partieId;
    public string jeuActuel;   // "MaireCoraille", "Jeu2", "Jeu3"
    public int nbJoueurs;
    public float dureePartie;
    public int gagnant;

    public List<JoueurResultat> joueurs = new List<JoueurResultat>();
    public DevStats devStats = new DevStats();

    // Stats libres spécifiques à chaque jeu
    public List<StatKV> statsSpecifiques = new List<StatKV>();
}

// Clé-valeur sérialisable pour stats libres
[Serializable]
public class StatKV
{
    public string cle;
    public string valeur;

    public StatKV(string cle, string valeur)
    {
        this.cle = cle;
        this.valeur = valeur;
    }
}

[Serializable]
public class JoueurResultat
{
    public int playerID;
    public StatsJoueur stats = new StatsJoueur();
    public float score;
    public List<string> titres = new List<string>();
}

[Serializable]
public class StatsJoueur
{
    public float distanceParcourue;
    public float tempsPoissonMax;
    public int nbRebonds;
    public int nbPassagesCoraille;
    public float tempsMaire;
}

[Serializable]
public class DevStats
{
    public int nbTransfertsTotal;
    public float dureeMoyenneRegne;
    public int nbDashUtilises;
    public int nbDashCollectes;
    public float tempsAvantPremierTransfert;
    public int nbRebondsMur;
    public int nbRebondsCoraille;
    public int nbRebondsJoueur;
    public int nbDashMenantTransfert;
    public int nbFoisBloqueMur;
}