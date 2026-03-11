using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ResultatExporter : MonoBehaviour
{
    public static ResultatExporter Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Exporter(PartieData partie)
    {
        ExporterJSON(partie);
        ExporterCSV(partie);
    }

    // ── JSON ──────────────────────────────────────────────────────────────────
    void ExporterJSON(PartieData partie)
    {
        string json = JsonUtility.ToJson(partie, true);
        string path = Path.Combine(
            Application.persistentDataPath,
            $"{partie.jeuActuel}_{partie.partieId}.json");

        File.WriteAllText(path, json);
        Debug.Log($"[Export] JSON → {path}");
    }

    // ── CSV / Excel ───────────────────────────────────────────────────────────
    void ExporterCSV(PartieData partie)
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            $"{partie.jeuActuel}_{partie.partieId}.csv");

        using (StreamWriter sw = new StreamWriter(path,
               false, System.Text.Encoding.UTF8))
        {
            // ── Infos partie ──────────────────────────────────────────────────
            sw.WriteLine("PARTIE");
            sw.WriteLine($"Jeu,{partie.jeuActuel}");
            sw.WriteLine($"ID,{partie.partieId}");
            sw.WriteLine($"NbJoueurs,{partie.nbJoueurs}");
            sw.WriteLine($"Duree,{partie.dureePartie:F1}s");
            sw.WriteLine($"Gagnant,J{partie.gagnant}");
            sw.WriteLine("");

            // ── Stats joueurs ─────────────────────────────────────────────────
            sw.WriteLine("JOUEURS");
            sw.WriteLine(
                "PlayerID,Distance,TempsPoissonMax," +
                "NbRebonds,NbPassagesCoraille," +
                "TempsMaire,Score,Titres");

            foreach (var j in partie.joueurs)
            {
                string titres = string.Join("|", j.titres);
                sw.WriteLine(
                    $"J{j.playerID}," +
                    $"{j.stats.distanceParcourue:F2}," +
                    $"{j.stats.tempsPoissonMax:F2}," +
                    $"{j.stats.nbRebonds}," +
                    $"{j.stats.nbPassagesCoraille}," +
                    $"{j.stats.tempsMaire:F2}," +
                    $"{j.score:F2}," +
                    $"{titres}");
            }

            sw.WriteLine("");

            // ── Stats dev ─────────────────────────────────────────────────────
            sw.WriteLine("STATS DEV");
            sw.WriteLine($"NbTransferts,{partie.devStats.nbTransfertsTotal}");
            sw.WriteLine($"DureeMoyenneRegne,{partie.devStats.dureeMoyenneRegne:F2}");
            sw.WriteLine($"NbDashUtilises,{partie.devStats.nbDashUtilises}");
            sw.WriteLine($"NbDashCollectes,{partie.devStats.nbDashCollectes}");
            sw.WriteLine($"TempsAvantPremierTransfert,{partie.devStats.tempsAvantPremierTransfert:F2}");
            sw.WriteLine($"NbRebondsMur,{partie.devStats.nbRebondsMur}");
            sw.WriteLine($"NbRebondsCoraille,{partie.devStats.nbRebondsCoraille}");
            sw.WriteLine($"NbRebondsJoueur,{partie.devStats.nbRebondsJoueur}");
            sw.WriteLine($"NbDashMenantTransfert,{partie.devStats.nbDashMenantTransfert}");
            sw.WriteLine($"NbFoisBloqueMur,{partie.devStats.nbFoisBloqueMur}");

            // ── Stats spécifiques au jeu ──────────────────────────────────────
            if (partie.statsSpecifiques != null &&
                partie.statsSpecifiques.Count > 0)
            {
                sw.WriteLine("");
                sw.WriteLine("STATS SPECIFIQUES");
                foreach (var kvp in partie.statsSpecifiques)
                    sw.WriteLine($"{kvp.cle},{kvp.valeur}");
            }
        }

        Debug.Log($"[Export] CSV → {path}");
    }
}