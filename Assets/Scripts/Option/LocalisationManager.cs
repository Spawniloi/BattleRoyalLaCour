using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LocalisationManager : MonoBehaviour
{
    public static LocalisationManager Instance;

    [Header("Google Sheets")]
    public string urlScript;

    [Header("Langue par défaut")]
    public string langueDefaut = "fr";

    // Données chargées
    private Dictionary<string, Dictionary<string, string>> traductions
        = new Dictionary<string, Dictionary<string, string>>();

    private string langueActuelle = "fr";
    private bool chargementTermine = false;

    // Callback quand les traductions sont prêtes
    public System.Action onTraductionsChargees;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        langueActuelle = PlayerPrefs.GetString("langue", langueDefaut);

        // ← Charge le JSON local immédiatement
        ChargerFallbackSilencieux();
    }

    void ChargerFallbackSilencieux()
    {
        TextAsset asset = Resources.Load<TextAsset>("languages");
        if (asset == null) return;
        ParseJSON(asset.text);
        // Pas de TerminerChargement() ici — juste les données
    }

    void Start()
    {
        StartCoroutine(ChargerDepuisSheets());
    }

    // ── Chargement Google Sheets ──────────────────────────────────────────────
    IEnumerator ChargerDepuisSheets()
    {
        if (string.IsNullOrEmpty(urlScript))
        {
            Debug.LogWarning("[Localisation] URL manquante — fallback JSON");
            ChargerFallback();
            yield break;
        }

        string url = urlScript + "?type=langues";
        Debug.Log($"[Localisation] Chargement depuis {url}");

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {

            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Localisation] Réponse brute : " +
            req.downloadHandler.text.Substring(0,
            Mathf.Min(200, req.downloadHandler.text.Length)));

                bool ok = ParseJSON(req.downloadHandler.text);

                if (ok)
                {
                    Debug.Log("[Localisation] ✅ Chargé depuis Sheets");
                    TerminerChargement();
                }
                else
                {
                    Debug.LogWarning(
                        "[Localisation] Parse échoué — fallback JSON");
                    ChargerFallback();
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[Localisation] Erreur réseau — fallback JSON\n" +
                    $"{req.error}");
                ChargerFallback();
            }
        }
    }

    // ── Parse JSON ────────────────────────────────────────────────────────────
    bool ParseJSON(string json)
    {
        try
        {
            // Parse manuel simple — évite dépendance Newtonsoft
            var wrapper = JsonUtility.FromJson<LangWrapper>(
                "{\"data\":" + json + "}");

            // JsonUtility ne gère pas les dict — on parse manuellement
            traductions.Clear();
            ParseLangueManuelle(json, "fr");
            ParseLangueManuelle(json, "en");
            ParseLangueManuelle(json, "cr");

            return traductions.Count > 0 &&
                   traductions.ContainsKey("fr") &&
                   traductions["fr"].Count > 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Localisation] Parse error: {e.Message}");
            return false;
        }
    }

    void ParseLangueManuelle(string json, string langue)
    {
        // Trouve le bloc "fr": { ... }
        string marker = $"\"{langue}\":{{";
        int start = json.IndexOf(marker);
        if (start < 0)
        {
            // Essaie avec espace
            marker = $"\"{langue}\": {{";
            start = json.IndexOf(marker);
        }
        if (start < 0) return;

        start += marker.Length;

        // Trouve la fermeture du bloc
        int depth = 1;
        int pos = start;
        while (pos < json.Length && depth > 0)
        {
            if (json[pos] == '{') depth++;
            if (json[pos] == '}') depth--;
            pos++;
        }

        string bloc = json.Substring(start, pos - start - 1);

        // Parse les paires clé:valeur
        var dict = new Dictionary<string, string>();
        int i = 0;

        while (i < bloc.Length)
        {
            // Cherche "clé"
            int kStart = bloc.IndexOf('"', i);
            if (kStart < 0) break;
            int kEnd = bloc.IndexOf('"', kStart + 1);
            if (kEnd < 0) break;

            string cle = bloc.Substring(kStart + 1, kEnd - kStart - 1);

            // Cherche ":"
            int colon = bloc.IndexOf(':', kEnd);
            if (colon < 0) break;

            // Cherche la valeur
            int vStart = bloc.IndexOf('"', colon);
            if (vStart < 0) break;

            // Cherche la fin de valeur (gère les échappements)
            int vEnd = vStart + 1;
            while (vEnd < bloc.Length)
            {
                if (bloc[vEnd] == '\\') { vEnd += 2; continue; }
                if (bloc[vEnd] == '"') break;
                vEnd++;
            }

            string valeur = bloc.Substring(vStart + 1, vEnd - vStart - 1);
            // Décode les échappements basiques
            valeur = valeur.Replace("\\n", "\n")
                           .Replace("\\\"", "\"")
                           .Replace("\\\\", "\\");

            if (!string.IsNullOrEmpty(cle))
                dict[cle] = valeur;

            i = vEnd + 1;
        }

        if (dict.Count > 0)
            traductions[langue] = dict;
    }

    // ── Fallback JSON local ───────────────────────────────────────────────────
    void ChargerFallback()
    {
        TextAsset asset = Resources.Load<TextAsset>("languages");

        if (asset == null)
        {
            Debug.LogError(
                "[Localisation] languages.json introuvable dans Resources !");
            TerminerChargement();
            return;
        }

        bool ok = ParseJSON(asset.text);

        if (ok)
            Debug.Log("[Localisation] ✅ Fallback JSON chargé");
        else
            Debug.LogError("[Localisation] ❌ Fallback JSON invalide !");

        TerminerChargement();
    }

    void TerminerChargement()
    {
        chargementTermine = true;
        onTraductionsChargees?.Invoke();

        // Notifie tous les TexteLocalise de la scène
        var textes = FindObjectsByType<TexteLocalise>(
            FindObjectsSortMode.None);
        foreach (var t in textes)
            t.AppliquerTraduction();

        Debug.Log($"[Localisation] Langue active : {langueActuelle}");
    }

    // ── API publique ──────────────────────────────────────────────────────────
    public string Get(string cle, params string[] args)
    {
        string texte = cle; // fallback ultime = la clé

        // Cherche dans la langue actuelle
        if (traductions.ContainsKey(langueActuelle) &&
            traductions[langueActuelle].ContainsKey(cle))
        {
            texte = traductions[langueActuelle][cle];
        }
        // Fallback FR
        else if (traductions.ContainsKey("fr") &&
                 traductions["fr"].ContainsKey(cle))
        {
            texte = traductions["fr"][cle];
        }

        for (int i = 0; i < args.Length; i++)
            texte = texte.Replace("{" + i + "}", args[i]);

        return texte;
    }

    public bool EstCharge() => chargementTermine;

    public string GetLangueActuelle() => langueActuelle;

    public void ChangerLangue(string langue)
    {
        if (!traductions.ContainsKey(langue))
        {
            Debug.LogWarning(
                $"[Localisation] Langue inconnue : {langue}");
            return;
        }

        langueActuelle = langue;
        PlayerPrefs.SetString("langue", langue);
        PlayerPrefs.Save();

        Debug.Log($"[Localisation] Langue changée → {langue}");

        // Notifie tous les TexteLocalise
        var textes = FindObjectsByType<TexteLocalise>(
            FindObjectsSortMode.None);
        foreach (var t in textes)
            t.AppliquerTraduction();
    }

    public List<string> GetLanguesDisponibles()
        => new List<string>(traductions.Keys);

    // Classe utilitaire (pas utilisée mais évite erreur compile)
    [System.Serializable]
    class LangWrapper { public string data; }
}