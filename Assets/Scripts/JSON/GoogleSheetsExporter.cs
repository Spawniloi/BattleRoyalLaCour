using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using System.Globalization;

public class GoogleSheetsExporter : MonoBehaviour
{
    public static GoogleSheetsExporter Instance;

    [Header("URL Google Apps Script")]
    public string urlScript = "COLLE_TON_URL_ICI";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Exporter(PartieData partie)
    {
        StartCoroutine(EnvoyerPartie(partie));
        StartCoroutine(EnvoyerDevStats(partie));
    }

    IEnumerator EnvoyerPartie(PartieData partie)
    {
        // Construit le JSON manuellement
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.Append($"\"type\":\"partie\",");
        sb.Append($"\"date\":\"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\",");
        sb.Append($"\"jeu\":\"{partie.jeuActuel}\",");
        sb.Append($"\"duree\":{partie.dureePartie.ToString("F1", CultureInfo.InvariantCulture)},");
        sb.Append($"\"nbJoueurs\":{partie.nbJoueurs},");
        sb.Append($"\"gagnant\":{partie.gagnant},");
        sb.Append("\"joueurs\":[");

        for (int i = 0; i < partie.joueurs.Count; i++)
        {
            var j = partie.joueurs[i];
            string titres = string.Join("/", j.titres);

            sb.Append("{");
            sb.Append($"\"playerID\":{j.playerID},");
            sb.Append($"\"score\":{j.score:F0},");
            sb.Append($"\"splash\":{j.stats.distanceParcourue:F0},");
            sb.Append($"\"cache\":{j.stats.tempsPoissonMax.ToString("F1", CultureInfo.InvariantCulture)},");
            sb.Append($"\"hop\":{j.stats.nbRebonds},");
            sb.Append($"\"ensemble\":{j.stats.nbPassagesCoraille},");
            sb.Append($"\"mechant\":{j.stats.tempsMaire.ToString("F1", CultureInfo.InvariantCulture)},");
            sb.Append($"\"titres\":\"{titres}\"");
            sb.Append("}");

            if (i < partie.joueurs.Count - 1)
                sb.Append(",");
        }

        sb.Append("]}");

        yield return StartCoroutine(Envoyer(sb.ToString()));
    }

    IEnumerator EnvoyerDevStats(PartieData partie)
    {
        var d = partie.devStats;

        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.Append($"\"type\":\"devStats\",");
        sb.Append($"\"date\":\"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\",");
        sb.Append($"\"jeu\":\"{partie.jeuActuel}\",");
        sb.Append($"\"nbTransferts\":{d.nbTransfertsTotal},");
        sb.Append($"\"dureeMoyenneRegne\":{d.dureeMoyenneRegne.ToString("F2", CultureInfo.InvariantCulture)},");
        sb.Append($"\"nbDashUtilises\":{d.nbDashUtilises},");
        sb.Append($"\"nbDashCollectes\":{d.nbDashCollectes},");
        sb.Append($"\"tempsAvantPremierTransfert\":{d.tempsAvantPremierTransfert.ToString("F2", CultureInfo.InvariantCulture)},");
        sb.Append($"\"nbRebondsMur\":{d.nbRebondsMur},");
        sb.Append($"\"nbRebondsCoraille\":{d.nbRebondsCoraille},");
        sb.Append($"\"nbRebondsJoueur\":{d.nbRebondsJoueur}");
        sb.Append("}");

        yield return StartCoroutine(Envoyer(sb.ToString()));
    }

    IEnumerator Envoyer(string json)
    {
        Debug.Log($"[Sheets] Envoi vers : {urlScript}");
        Debug.Log($"[Sheets] JSON : {json}");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = new UnityWebRequest(urlScript, "POST");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            Debug.Log($"[Sheets] Envoi OK : {req.downloadHandler.text}");
        else
            Debug.LogWarning($"[Sheets] Erreur : {req.error}");
    }
}